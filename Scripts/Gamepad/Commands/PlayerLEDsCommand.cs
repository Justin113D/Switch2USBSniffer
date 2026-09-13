using Amicitia.IO.Binary;

namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class PlayerLEDsCommand : SubCommand
	{
		public override byte ID => 9;

		public class SetPlayer1 : PlayerLEDsCommand
		{
			public override byte SubID => 1;

			public override string SubName => "Set player 1";
		}

		public class SetPlayer2 : PlayerLEDsCommand
		{
			public override byte SubID => 2;

			public override string SubName => "Set player 2";
		}

		public class SetPlayer3 : PlayerLEDsCommand
		{
			public override byte SubID => 3;

			public override string SubName => "Set player 3";
		}

		public class SetPlayer4 : PlayerLEDsCommand
		{
			public override byte SubID => 4;

			public override string SubName => "Set player 4";
		}

		public class SetAll : PlayerLEDsCommand
		{
			public override byte SubID => 5;

			public override string SubName => "Set all";
		}

		public class SetNone : PlayerLEDsCommand
		{
			public override byte SubID => 6;

			public override string SubName => "Set none";
		}

		public class SetPattern : PlayerLEDsCommand
		{
			public override byte SubID => 7;

			public override string SubName => "Set pattern";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string bitmask = reader.ReadHexBytes(1);
				string unknown = reader.ReadHexBytes(3);

				return $"{bitmask} /? {unknown}";
			}
		}

		public class Flash : PlayerLEDsCommand
		{
			public override byte SubID => 8;

			public override string SubName => "Flash";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				bool enable = reader.ReadByte() != 0;
				string unknown = reader.ReadHexBytes(7);

				return $"{enable} /? {unknown}";
			}
		}
	}
}
