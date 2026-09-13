namespace Switch2USBSniffer.Data.Serial.Packets
{
	internal readonly struct PacketCommandStop : IPacket
	{
		public byte[] GetBytes()
		{
			return [ 1 ];
		}
	}
}
