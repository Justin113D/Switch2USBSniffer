namespace Switch2USBSniffer.Data.Serial.Packets
{
	internal readonly struct PacketCommandSetPIDFilter : IPacket
	{
		public enum PIDFlags : uint
		{
			TokenOut = 1 << 0b0001,
			TokenIn = 1 << 0b1001,
			TokenSof = 1 << 0b0101, 
			TokenSetup = 1 << 0b1101, 

			Data0 = 1 << 0b0011,
			Data1 = 1 << 0b1011,
			Data2 = 1 << 0b0111,
			DataM = 1 << 0b1111,

			HandshakeAck = 1 << 0b0010,
			HandshakeNak = 1 << 0b1010,
			HandshakeStall = 1 << 0b1110,
			HandshakeNyet = 1 << 0b0110,
		}

		public PIDFlags Flags { get; init; }

		public PacketCommandSetPIDFilter(PIDFlags flags)
		{
			Flags = flags;
		}

		public byte[] GetBytes()
		{
			return [2, (byte)(((ushort)Flags) & 0xFF), (byte)(((ushort)Flags >> 8) & 0xFF)];
		}
	}
}
