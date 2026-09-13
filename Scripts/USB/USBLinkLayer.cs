using Switch2USBSniffer.Data.Gamepad;
using Switch2USBSniffer.Data.USB.Packets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Switch2USBSniffer.Data.USB
{
	internal class USBLinkLayer
	{
		private const int maxDataLength = 64;

		enum State
		{
			Listening,
			Init,
			WaitingForAck
		}

		enum Expecting
		{
			Any,
			Response
		}

		private State _state;
		private Expecting _expecting;

		private USBLLPID _stateToken;
		private USBLLPID _currentToken;
		private USBLLPID _previousToken;

		private int _dataSize;
		private bool _setupNAK;
		private readonly List<byte[]> _dataFragments = [];
		private SetupRequest? _currentSetupRequest = null;

		public delegate void ReceivedHIDReportEventHandler(byte[] data, uint timestamp);

		public delegate void ReceivedCommandEventHandler(string info);

		public delegate void ReceivedSetupEventHandler(string info);

		public event ReceivedHIDReportEventHandler? ReceivedHIDReport;

		public event ReceivedCommandEventHandler? ReceivedCommand;

		public event ReceivedSetupEventHandler? ReceivedSetup;

		public void ProcessPacket(byte[] packetData, uint timestamp)
		{
			USBLLPID pid = (USBLLPID)packetData[0];
			if (!Enum.IsDefined(pid))
			{
				//throw new ArgumentException("Invalid PID");
				return;
			}

			if (pid == USBLLPID.TokenIn && packetData.Length == 1)
			{
				// search broadcast?
				return;
			}


			IUSBPacket usbPacket = pid switch
			{
				USBLLPID.TokenOut 
				or USBLLPID.TokenIn 
				or USBLLPID.TokenSetup => new TokenPacket(),

				USBLLPID.TokenSof => new SOFPacket(),

				USBLLPID.Data0 
				or USBLLPID.Data1 
				or USBLLPID.Data2 
				or USBLLPID.DataM => new DataPacket(),

				USBLLPID.HandshakeAck 
				or USBLLPID.HandshakeNak 
				or USBLLPID.HandshakeStall 
				or USBLLPID.HandshakeNyet => new HandshakePacket(),

				_ => throw new NotImplementedException(),
			};

			try
			{
				if(!usbPacket.FromData(packetData))
				{
					return;
				}
			}
			catch(ArgumentException)
			{
				return;
			}

			UpdateState(usbPacket, timestamp);
		}

		private byte[]? AddFragment(byte[] data)
		{
			if(_dataFragments.Count == 0 && data.Length < maxDataLength)
			{
				return data;
			}

			_dataFragments.Add(data);
			if(data.Length >= maxDataLength)
			{
				return null;
			}

			return FinalizeFragments();
		}

		private byte[] FinalizeFragments()
		{
			byte[] result = [.. _dataFragments.SelectMany(x => x)];
			_dataFragments.Clear();
			return result;
		}

		private void PrintCommand(byte[] data)
		{
			if(data.Length == 0)
			{
				return;
			}
			else if (Command.DecodeCommand(data, out CommandInfo? info))
			{
				ReceivedCommand?.Invoke(info.Value.ToString());
			}
			else
			{
				ReceivedCommand?.Invoke("INVALID DATA: " + string.Join(' ', data.Select(x => x.ToString("X2"))));
			}
		}

		private int ProcessData(DataPacket packet, uint timestamp)
		{
			switch (_stateToken)
			{
				case USBLLPID.TokenIn:
					if(packet.Data.Length == 64)
					{
						ReceivedHIDReport?.Invoke(packet.Data, timestamp);
					}
					return packet.Data.Length;
				case USBLLPID.TokenOut:
					if (_previousToken == USBLLPID.TokenIn && _currentToken == USBLLPID.TokenOut)
					{
						_dataFragments.Clear();
					}

					if (_previousToken == USBLLPID.TokenOut && _currentToken == USBLLPID.TokenIn)
					{
						PrintCommand(FinalizeFragments());
					}

					byte[]? data = AddFragment(packet.Data);

					if (data == null)
					{
						return 0;
						
					}

					PrintCommand(data);
					return data.Length;
				case USBLLPID.TokenSetup:
					switch (_expecting)
					{
						case Expecting.Any:
							_currentSetupRequest = new();
							_currentSetupRequest.FromData(packet.Data);

							ReceivedSetup?.Invoke($"<- {_currentSetupRequest}");
							return packet.Data.Length;
						case Expecting.Response:
							if(_currentToken == USBLLPID.TokenIn)
							{
								if(_dataFragments.Count == 0 && packet.Data.Length == 0)
								{
									ReceivedSetup?.Invoke($"-> [0]");
									return -1;
								}

								_dataFragments.Add(packet.Data);
								return 0;
							}
							else if(!_setupNAK)
							{
								byte[] responseData = FinalizeFragments();
								ReceivedSetup?.Invoke($"-> {_currentSetupRequest!.ProcessResponse(responseData)}");
								return responseData.Length;
							}
							break;
					}
					
					break;
			}

			return -2;
		}

		private void UpdateState(IUSBPacket packet, uint timestamp)
		{
			//string baseLog = $"{_state.ToString().PadRight(17, '.')} {_expecting.ToString().PadRight(8, '.')} {packet.PID.ToString().PadRight(23, '.')}";
			//if (packet.PID is USBLLPID.Data0 or USBLLPID.Data1)
			//{
			//	Console.Write(baseLog);
			//	Console.WriteLine($"[{((DataPacket)packet).Data.Length}]");
			//}
			//else
			//{
			//	Console.WriteLine(baseLog);
			//}


			switch (_state)
			{
				case State.Listening:
					if(packet.PID is USBLLPID.TokenOut or USBLLPID.TokenIn or USBLLPID.TokenSetup)
					{
						_state = State.Init;
						_previousToken = _currentToken;
						_currentToken = packet.PID;

						if(_expecting == Expecting.Response && (
							(
								_stateToken == USBLLPID.TokenSetup 
								&& packet.PID == USBLLPID.TokenSetup
							) || (
								_stateToken == USBLLPID.TokenOut 
								&& packet.PID != USBLLPID.TokenIn
							)))
						{
							_expecting = Expecting.Any;
						}

						if(_expecting == Expecting.Any)
						{
							_stateToken = packet.PID;
							_dataSize = 0;
						}

						return;
					}
					break;
				case State.Init:
					
					_setupNAK = packet.PID == USBLLPID.HandshakeNak
						&& _stateToken == USBLLPID.TokenSetup
						&& _expecting == Expecting.Response;

					if(packet.PID is USBLLPID.Data0 or USBLLPID.Data1)
					{
						_dataSize = ProcessData((DataPacket)packet, timestamp);
						_state = State.WaitingForAck;
						return;
					}

					break;
				case State.WaitingForAck:

					if(packet.PID == USBLLPID.HandshakeAck)
					{
						if(_expecting == Expecting.Any && _stateToken is USBLLPID.TokenOut or USBLLPID.TokenSetup && _dataSize != 0)
						{
							_expecting = Expecting.Response;
						}
						else if(_expecting == Expecting.Any || (_expecting == Expecting.Response && _dataSize != 0))
						{
							_expecting = Expecting.Any;
							_stateToken = default;
						}

						_dataSize = 0;
					}
					break;
			}

			_state = State.Listening;
		}

	}
}
