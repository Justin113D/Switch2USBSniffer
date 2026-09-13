using System;
using System.Collections.Generic;
using System.Linq;
using Switch2USBSniffer.Data.Serial.Packets;


namespace Switch2USBSniffer.Data.Serial
{
	internal class PacketSerializer
	{
		private const ushort _maxPacketDataSize = 1028;

		private const ushort _packetHeaderSize = sizeof(byte) + sizeof(uint);
		private const ushort _packetSize = _packetHeaderSize + _maxPacketDataSize;

		private const byte _slip_end = 0xC0;
		private const byte _slip_escape = 0xDB;
		private const byte _slip_escape_end = 0xDC;
		private const byte _slip_escape_escape = 0xDD;

		public class DeserializeBuffer
		{
			public readonly byte[] SerialBuffer = new byte[_packetSize * 2 + 1];
			private readonly byte[] _readBuffer = new byte[_packetSize];
			private ushort _readBufferPosition = 0;
			private bool _readBufferEscaped = false;


			private void AddReadByte(byte value)
			{
				_readBuffer[_readBufferPosition] = value;
				_readBufferPosition = (ushort)((_readBufferPosition + 1) % _readBuffer.Length);
			}

			private bool ProcessMessage(out USBPacket packet)
			{
				if (_readBufferPosition <= _packetHeaderSize
					|| _readBuffer[0] != 0)// if not USB message
				{
					packet = default;
					return false;
				}

				packet = new(
					BitConverter.ToUInt32(_readBuffer.AsSpan(1)),
					_readBuffer[_packetHeaderSize.._readBufferPosition]
				);

				return true;
			}

			public USBPacket[]? Deserialize(int length)
			{
				List<USBPacket> messages = [];

				for (int i = 0; i < length; i++)
				{
					byte value = SerialBuffer[i];

					if (_readBufferEscaped)
					{
						_readBufferEscaped = false;

						switch (value)
						{
							case _slip_escape_end:
								value = _slip_end;
								break;
							case _slip_escape_escape:
								value = _slip_escape;
								break;
						}
						AddReadByte(value);
						continue;
					}

					switch (value)
					{
						case _slip_end:
							if (ProcessMessage(out USBPacket message))
							{
								messages.Add(message);
							}

							_readBufferPosition = 0;
							break;
						case _slip_escape:
							_readBufferEscaped = true;
							break;
						default:
							AddReadByte(value);
							break;
					}

				}

				if (messages.Count == 0)
				{
					return null;
				}

				return [.. messages];
			}
		}

		public static byte[] Serialize(IPacket packet)
		{
			byte[] raw = packet.GetBytes();

			if (raw.Length > _packetSize)
			{
				throw new ArgumentException("Data is too large");
			}

			int toEscapeCount = raw.Count(x => x == _slip_end || x == _slip_escape);
			byte[] result = new byte[raw.Length + toEscapeCount + 1];
			for (int i = 0, j = 0; i < raw.Length; i++, j++)
			{
				byte value = raw[i];

				if (value == _slip_end)
				{
					result[j] = _slip_escape;
					j++;
					value = _slip_escape_end;
				}
				else if (value == _slip_escape)
				{
					result[j] = _slip_escape;
					j++;
					value = _slip_escape_escape;
				}

				result[j] = value;
			}
			result[^1] = _slip_end;

			return result;
		}
	}
}
