using Amicitia.IO.Binary;

namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class InitializationCommand : SubCommand
	{
		public override byte ID => 3;

		public class BluetoothWake : InitializationCommand
		{
			public override byte SubID => 1;

			public override string SubName => "Bluetooth wake";
		}

		public class BluetoothCancel : InitializationCommand
		{
			public override byte SubID => 2;

			public override string SubName => "Bluetooth cancel";
		}

		public class EnableUSBHIDReports : InitializationCommand
		{
			public override byte SubID => 3;

			public override string SubName => "Enable USB HID reports";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				bool enable = reader.ReadByte() != 0;
				string unknown = reader.ReadHexBytes(3);

				return $"{enable} /? {unknown}";
			}
		}

		public class SendPairingInfo : InitializationCommand
		{
			public override byte SubID => 7;

			public override string SubName => "Send pairing info";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string address = BluetoothPairingCommand.ReadAddress(reader);
				string ltk = BluetoothPairingCommand.ReadLTK(reader);
				return $"{address}, {ltk}";
			}
		}

		public class ClearPairingInfo : InitializationCommand
		{
			public override byte SubID => 8;

			public override string SubName => "Clear pairing info";
		}

		public class StorePairingInfo : InitializationCommand
		{
			public override byte SubID => 9;

			public override string SubName => "Store pairing info";
		}

		public class SelectInputReport : InitializationCommand
		{
			public override byte SubID => 10;

			public override string SubName => "Select input report";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				byte reportID = reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);
				return $"{reportID} /? {unknown}";
			}
		}

		public class InitializeUSB : InitializationCommand
		{
			public override byte SubID => 13;

			public override string SubName => "Initialize USB";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(2);
				string address = BluetoothPairingCommand.ReadAddress(reader);

				return $"{unknown} ?/ {address}";
			}
		}
	}
}
