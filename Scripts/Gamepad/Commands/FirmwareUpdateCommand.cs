using Amicitia.IO.Binary;

namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class FirmwareUpdateCommand : SubCommand
	{
		public override byte ID => 13;

		public class InitializeUpdate : FirmwareUpdateCommand
		{
			public override byte SubID => 1;

			public override string SubName => "Initialize update?";

		}

		public class SetFailsafeAddress : FirmwareUpdateCommand
		{
			public override byte SubID => 2;

			public override string SubName => "Set failsafe address?";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(1);
				uint address = reader.ReadUInt32();
				return $"{unknown} ?/ 0x{address:X8}";
			}
		}

		public class SetImageSize : FirmwareUpdateCommand
		{
			public override byte SubID => 3;

			public override string SubName => "Set image size?";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(5);
				uint size = reader.ReadUInt32();
				return $"{unknown} ?/ {size}";
			}
		}

		public class TransferUpdateData : FirmwareUpdateCommand
		{
			public override byte SubID => 4;

			public override string SubName => "Transfer update data";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				int length = reader.ReadInt32();
				string data = reader.ReadHexBytes(length);
				return data;
			}
		}

		public class EndDataTransfer : FirmwareUpdateCommand
		{
			public override byte SubID => 5;

			public override string SubName => "End data transfer?";
		}

		public class VerifyUpdate : FirmwareUpdateCommand
		{
			public override byte SubID => 6;

			public override string SubName => "Verify update?";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(5);
				uint size = reader.ReadUInt32();
				uint checksum = reader.ReadUInt32();
				return $"{unknown} ?/ {size}, {checksum}";
			}
		}

		public class FinalizeUpdate : FirmwareUpdateCommand
		{
			public override byte SubID => 7;

			public override string SubName => "FinalizeUpdate";
		}
	}
}
