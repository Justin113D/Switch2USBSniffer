using Amicitia.IO.Binary;

namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class BatteryCommand : SubCommand
	{
		public override byte ID => 11;


		public class GetVoltage : BatteryCommand
		{
			public override byte SubID => 3;

			public override string SubName => "Get Voltage";

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				ushort charge = reader.ReadUInt16();
				string unknown = reader.ReadHexBytes(2);

				return $"{charge} Mv /? {unknown}";
			}
		}

		public class GetCharge : BatteryCommand
		{
			public override byte SubID => 4;

			public override string SubName => "Get Charge";
		}
	}
}
