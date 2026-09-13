using System;

namespace Switch2USBSniffer.Data.USB.Packets
{
	internal struct SOFPacket : IUSBPacket
	{
		public readonly USBLLPID PID => USBLLPID.TokenSof;
		public ushort FrameNumber { get; private set; }

		public bool FromData(byte[] data)
		{
			if (data.Length < 3)
			{
				throw new ArgumentException($"{USBLLPID.TokenSof} Data too small ({data.Length} < 3)");
			}

			ushort info = BitConverter.ToUInt16(data, 1);
			FrameNumber = (ushort)(info & 0x7FF);

			byte crc5 = (byte)(info >> 11);
			byte check = CRC.CRC5On11Bit(info);

			return crc5 == check;
		}

		public override string ToString()
		{
			return $"Token SOF - {FrameNumber}";
		}
	}
}
