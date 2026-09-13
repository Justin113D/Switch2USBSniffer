using Amicitia.IO.Binary;

namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class FlashMemoryCommand : SubCommand
	{
		public override byte ID => 2;

		public class ReadBlock : FlashMemoryCommand
		{
			public override byte SubID => 1;

			public override string SubName => "Read block";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(4);
				uint address = reader.ReadUInt32();
				return $"{unknown} ?/ 0x{address:X8}";
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				byte readLength = reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);
				uint address = reader.ReadUInt32();
				string data = reader.ReadHexBytes(readLength);

				return $"{unknown} ?/ 0x{address:X8}: {data}";
			}
		}

		public class WriteBlock : FlashMemoryCommand
		{
			public override byte SubID => 2;

			public override string SubName => "Write block";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(4);
				uint address = reader.ReadUInt32();
				string data = reader.ReadHexBytes(64);

				return $"{unknown} ?/ 0x{address:X8}: {data}";
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(4);
				uint address = reader.ReadUInt32();
				return $"{unknown} ?/ 0x{address:X8}";
			}
		}

		public class EraseSector : FlashMemoryCommand
		{
			public override byte SubID => 3;

			public override string SubName => "Erase sector";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(4);
				uint address = reader.ReadUInt32();

				return $"{unknown} ?/ 0x{address:X8}";
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				return reader.ReadHexBytes(4);
			}
		}

		public class Read : FlashMemoryCommand
		{
			public override byte SubID => 4;

			public override string SubName => "Read";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				byte readLength = reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);
				uint address = reader.ReadUInt32();
				return $"[{readLength}] /? {unknown} ?/ 0x{address:X8}";
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				byte readLength = reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);
				uint address = reader.ReadUInt32();
				string data = reader.ReadHexBytes(readLength);

				return $"{unknown} ?/ 0x{address:X8}: {data}";
			}
		}

		public class Write : FlashMemoryCommand
		{
			public override byte SubID => 5;

			public override string SubName => "Write";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				byte writeLength = reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);
				uint address = reader.ReadUInt32();
				string data = reader.ReadHexBytes(writeLength);

				return $"{unknown} ?/ 0x{address:X8}: {data}";
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(4);
				uint address = reader.ReadUInt32();
				return $"{unknown} ?/ 0x{address:X8}";
			}
		}
	}
}
