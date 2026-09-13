namespace Switch2USBSniffer.Data.Gamepad
{
	internal readonly struct CommandInfo
	{
		public int CommandID { get; init; }

		public string CommandName { get; init; }

		public CommandDirection Direction { get; init; }

		public CommandTransport Transport { get; init; }

		public int SubCommandID { get; init; }

		public string SubCommandName { get; init; }

		public string CommandData { get; init; }

		public override string ToString()
		{
			return $"[{Direction.ToString()[0]}{Transport.ToString()[0]}] {CommandID:D2}.{SubCommandID:D2} \"{CommandName}\"-\"{SubCommandName}\": {CommandData}";
		}
	}
}
