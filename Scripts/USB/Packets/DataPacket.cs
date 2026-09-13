using System;

namespace Switch2USBSniffer.Data.USB.Packets
{
	internal struct DataPacket : IUSBPacket
	{
		public USBLLPID PID { get; private set; }

		public byte[] Data { get; private set; }

		public bool FromData(byte[] data)
		{
			PID = (USBLLPID)data[0];

			if (data.Length < 3)
			{
				throw new ArgumentException($"{PID} Data too small ({data.Length} < 3)");
			}
			
			ushort crc16 = BitConverter.ToUInt16(data, data.Length - 2);
			ushort check;

			if (data.Length == 3)
			{
				Data = [];
				check = 0;
			}
			else
			{
				Data = data[1..(data.Length - 2)];
				check = CRC.CRC16(Data);
			}

			return crc16 == check;
		}

		public override string ToString()
		{
			return $"{PID} [{Data.Length}]";
		}
	}
}
