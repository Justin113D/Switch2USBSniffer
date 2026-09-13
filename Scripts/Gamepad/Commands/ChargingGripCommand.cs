namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class ChargingGripCommand : SubCommand
	{
		public override byte ID => 8;

		public class GetInfo1 : ChargingGripCommand
		{
			public override byte SubID => 1;

			public override string SubName => "Get Info 1";

		}

		public class EnableButtons : ChargingGripCommand
		{
			public override byte SubID => 2;

			public override string SubName => "Enable Buttons";

		}

		public class GetInfo2 : ChargingGripCommand
		{
			public override byte SubID => 3;

			public override string SubName => "Get Info 2";

		}
	}
}
