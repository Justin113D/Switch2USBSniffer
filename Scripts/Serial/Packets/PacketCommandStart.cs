namespace Switch2USBSniffer.Data.Serial.Packets
{
	internal readonly struct PacketCommandStart : IPacket
	{
		public byte[] GetBytes()
		{
			return [ 0 ];
		}
	}
}
