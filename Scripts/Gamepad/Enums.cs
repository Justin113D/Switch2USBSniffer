namespace Switch2USBSniffer.Data.Gamepad
{
	public enum CommandDirection : byte
	{
		HostToDevice = 0x91,
		DeviceToHost = 0x01
	}

	public enum CommandTransport : byte
	{
		USB = 0,
		Bluetooth = 1
	}
}
