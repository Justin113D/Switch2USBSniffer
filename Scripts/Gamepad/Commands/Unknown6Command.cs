namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class Unknown6Command : SubCommand
	{
		public override byte ID => 6;

		public class ShutdownController : Unknown6Command
		{
			public override byte SubID => 2;

			public override string SubName => "Shutdown controller?";
		}

		public class RebootController : Unknown6Command
		{
			public override byte SubID => 3;

			public override string SubName => "Reboot controller?";
		}
	}
}
