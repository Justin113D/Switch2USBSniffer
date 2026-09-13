using System;

namespace Switch2USBSniffer.Data.USB.Packets
{
	internal struct TokenPacket : IUSBPacket
	{
		public USBLLPID PID { get; private set; }
		public byte Address { get; private set; }
		public byte Endpoint { get; private set; }

		public bool FromData(byte[] data)
		{
			PID = (USBLLPID)data[0];

			if (data.Length < 3)
			{
				throw new ArgumentException($"{PID} Data too small ({data.Length} < 3)");
			}

			ushort info = BitConverter.ToUInt16(data, 1);
			Address = (byte)(info & 0x7F);
			Endpoint = (byte)((info >> 7) & 0x0F);

			byte crc5 = (byte)(info >> 11);
			byte check = CRC.CRC5On11Bit(info);

			return crc5 == check;
		}

		public override string ToString()
		{
			return $"{PID} Token - {Address}.{Endpoint}";
		}
	}
}
