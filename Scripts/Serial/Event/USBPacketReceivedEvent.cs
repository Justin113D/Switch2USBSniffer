using Switch2USBSniffer.Data.Serial.Packets;
using System;

namespace Switch2USBSniffer.Data.Serial.Event
{
	internal class USBPacketReceivedEventArgs : EventArgs
	{
		public uint Timestamp { get; }
		public byte[] Data {get; }

		internal USBPacketReceivedEventArgs(USBPacket packet)
		{
			Timestamp = packet.Timestamp;
			Data = packet.Data;
		}

	}

	internal delegate void USBPacketReceivedEventHandler(string port, USBPacketReceivedEventArgs args);
}
