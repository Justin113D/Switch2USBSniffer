namespace Switch2USBSniffer.Data.USB.Packets
{
	internal struct HandshakePacket : IUSBPacket
	{
		public USBLLPID PID { get; private set; }

		public bool FromData(byte[] data)
		{
			PID = (USBLLPID)data[0];
			return true;
		}

		public override string ToString()
		{
			return $"{PID} Handshake";
		}
	}
}
