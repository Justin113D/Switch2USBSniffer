using RJCP.IO.Ports;
using Switch2USBSniffer.Data.Serial.Event;
using Switch2USBSniffer.Data.Serial.Packets;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Switch2USBSniffer.Data.Serial
{
	internal class SnifferConnector : IDisposable
	{
		private readonly SerialPortStream _serialPortStream;
		private Stream Stream => _serialPortStream;
		private readonly PacketSerializer.DeserializeBuffer _readBuffer;

		public string Port => _serialPortStream.PortName;
		public event USBPacketReceivedEventHandler? USBPackedReceived;
		public event DisconnectedEventHandler? Disconnected;

		private SnifferConnector(SerialPortStream serialPortStream)
		{
			_serialPortStream = serialPortStream;
			_readBuffer = new();
		}

		public static (string port, string description)[] ListAvailablePorts()
		{
			return SerialPortStream.GetPortDescriptions().Select(x => (x.Port, x.Description)).OrderBy(x => x.Port).ToArray();
		}

		public enum ConnectionResult
		{
			Success,
			NotFound,
			OpenFailed,
			UnknownError
		}

		public static async Task<(ConnectionResult, SnifferConnector?)> TryEstablishConnection(string port)
		{
			SerialPortStream serialPort;

			try
			{
				serialPort = new(port, 9600, 8, Parity.None, StopBits.One)
				{
					DtrEnable = true,
					RtsEnable = true
				};

			}
			catch
			{
				return (ConnectionResult.OpenFailed, null);
			}

			try
			{
				serialPort.Open();
			}
			catch
			{
				serialPort.Dispose();
				return (ConnectionResult.OpenFailed, null);
			}

			SnifferConnector connector = new(serialPort);
			try
			{
				_ = Task.Run(connector.ReadAsyncTask);
				await connector.SendStart();
				await connector.SendSetPIDFilter(PacketCommandSetPIDFilter.PIDFlags.TokenSof);

				return (ConnectionResult.Success, connector);
			}
			catch
			{
				connector?.Dispose();
				return (ConnectionResult.UnknownError, null);
			}
		}

		private async Task ReadAsyncTask()
		{
			while (Stream.CanRead)
			{
				USBPacket[]? receivedMessages = await ReadAsync();
				if (receivedMessages != null)
				{
					foreach (USBPacket receivedMessage in receivedMessages)
					{
						USBPackedReceived?.Invoke(Port, new(receivedMessage));
					}
				}
			}

			Disconnected?.Invoke(Port, new());
		}

		private async Task<USBPacket[]?> ReadAsync()
		{
			int bytesRead = await Stream.ReadAsync(_readBuffer.SerialBuffer);
			
			return _readBuffer.Deserialize(bytesRead);
		}

		private ValueTask SendPacket(IPacket packet)
		{
			byte[] data = PacketSerializer.Serialize(packet);
			return _serialPortStream.WriteAsync(data);
		}

		public ValueTask SendStart()
		{
			return SendPacket(new PacketCommandStart());
		}

		public ValueTask SendStop()
		{
			return SendPacket(new PacketCommandStop());
		}

		public ValueTask SendSetPIDFilter(PacketCommandSetPIDFilter.PIDFlags flags)
		{
			return SendPacket(new PacketCommandSetPIDFilter(flags));
		}


		#region Boilerplate dispose code

		private bool _disposedValue;

		private void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					if (Stream.CanWrite)
					{
						Stream.Write(PacketSerializer.Serialize(new PacketCommandStop()));
					}

					Stream.Dispose();
				}

				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
