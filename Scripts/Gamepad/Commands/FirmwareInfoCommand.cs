using Amicitia.IO.Binary;

namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class FirmwareInfoCommand : SubCommand
	{
		public override byte ID => 16;

		public class GetVersion : FirmwareInfoCommand
		{
			public override byte SubID => 1;

			public override string SubName => "Get Version";

			private static string ReadVersion(BinaryObjectReader reader)
			{
				byte major = reader.ReadByte();
				byte minor = reader.ReadByte();
				byte patch = reader.ReadByte();

				return $"{major}.{minor}.{patch}";
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				string firmwareVersion = ReadVersion(reader);
				byte firmwareType = reader.ReadByte();

				string bluetoothVersion = ReadVersion(reader);
				byte padding1 = reader.ReadByte();

				string dspVersion = ReadVersion(reader);
				byte padding2 = reader.ReadByte();

				return $"{firmwareVersion}, {firmwareType}; {bluetoothVersion}; {dspVersion}";
			}
		}
	}
}
