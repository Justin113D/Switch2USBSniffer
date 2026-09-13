using Amicitia.IO.Binary;
using System;

namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class FeatureSelectCommand : SubCommand
	{
		public override byte ID => 12;

		[Flags]
		public enum Feature : byte
		{
			Buttons = 0x01,
			Joysticks = 0x02,
			IMU = 0x04,
			Unknown3 = 0x08,
			Mouse = 0x10,
			Rumble = 0x20,
			Unknown6 = 0x40,
			Magnetometer = 0x80
		}

		public class GetInfo : FeatureSelectCommand
		{
			public override byte SubID => 1;

			public override string SubName => "Get Info";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				Feature flags = (Feature)reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);

				return $"{flags} /? {unknown}";
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(4);
				string info = reader.ReadHexBytes(8);

				return $"{unknown} ?/ {info}";
			}
		}

		public class SetMask : FeatureSelectCommand
		{
			public override byte SubID => 2;

			public override string SubName => "Set Mask";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				Feature flags = (Feature)reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);

				return $"{flags} /? {unknown}";
			}
		}

		public class ClearMask : FeatureSelectCommand
		{
			public override byte SubID => 3;

			public override string SubName => "ClearMask";
		}

		public class Enable : FeatureSelectCommand
		{
			public override byte SubID => 4;

			public override string SubName => "Enable";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				Feature flags = (Feature)reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);

				return $"{flags} /? {unknown}";
			}
		}

		public class Disable : FeatureSelectCommand
		{
			public override byte SubID => 5;

			public override string SubName => "Disable";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				Feature flags = (Feature)reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);

				return $"{flags} /? {unknown}";
			}
		}

		public class Configure : FeatureSelectCommand
		{
			public override byte SubID => 6;

			public override string SubName => "Configure";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				Feature flags = (Feature)reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);

				return $"{flags} /? {unknown}";
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(4);
				int length = reader.ReadInt32();
				string data = reader.ReadHexBytes(32);

				return $"{unknown} ?/ [{length}] {data}";
			}
		}
	}
}
