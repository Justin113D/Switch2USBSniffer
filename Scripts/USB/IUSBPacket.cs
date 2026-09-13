namespace Switch2USBSniffer.Data.USB
{
	internal interface IUSBPacket
	{
		public USBLLPID PID { get; }

		public bool FromData(byte[] data);
	}
}
