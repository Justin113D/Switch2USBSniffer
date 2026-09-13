using System;

namespace Switch2USBSniffer.Data.USB
{
	internal class CRCException : Exception
	{
		public uint Received { get; }
		public uint Calculated { get; }

		public CRCException(uint received, uint calculated) : base($"Received CRC5 {received}, when it should have been {calculated}")
		{
			Received = received;
			Calculated = calculated;
		}
	}
}
