namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class NFCCommand : SubCommand
	{
		public override byte ID => 1;

		public class GetStatus : NFCCommand
		{
			public override byte SubID => 5;

			public override string SubName => "Get status";
		}

		public class ReadDevice : NFCCommand
		{
			public override byte SubID => 6;

			public override string SubName => "Read device";
		}

		public class WriteDevice : NFCCommand
		{
			public override byte SubID => 8;

			public override string SubName => "Write device";
		}

		public class WriteBuffer : NFCCommand
		{
			public override byte SubID => 20;

			public override string SubName => "Write buffer";
		}

		public class ReadBuffer : NFCCommand
		{
			public override byte SubID => 21;

			public override string SubName => "ReadBuffer";
		}
	}
}
