namespace Switch2USBSniffer.Data.Serial.Packets
{
	internal readonly struct USBPacket
	{
		public readonly uint Timestamp;
		public readonly byte[] Data;

		public USBPacket(uint timestamp, byte[] data)
		{
			Timestamp = timestamp;
			Data = data;
		}
	}
}
