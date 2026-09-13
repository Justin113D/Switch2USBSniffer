using System;

namespace Switch2USBSniffer.Data.Serial.Event
{
	internal class DisconnectedEventArgs : EventArgs
	{
		internal DisconnectedEventArgs() { }

	}

	internal delegate void DisconnectedEventHandler(string port, DisconnectedEventArgs args);
}
