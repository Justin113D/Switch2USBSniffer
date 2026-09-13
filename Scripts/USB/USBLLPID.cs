namespace Switch2USBSniffer.Data.USB
{
	internal enum USBLLPID : byte
	{
		TokenOut = 0b1110_0001,
		TokenIn = 0b0110_1001,
		TokenSof = 0b1010_0101,
		TokenSetup = 0b0010_1101,

		Data0 = 0b1100_0011,
		Data1 = 0b0100_1011,
		Data2 = 0b1000_0111,
		DataM = 0b0000_1111,

		HandshakeAck = 0b1101_0010,
		HandshakeNak = 0b0101_1010,
		HandshakeStall = 0b0001_1110,
		HandshakeNyet = 0b1001_0110,
	}
}
