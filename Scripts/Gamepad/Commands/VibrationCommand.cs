using Amicitia.IO.Binary;

namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class VibrationCommand : SubCommand
	{
		public override byte ID => 10;

		public class PlaySample : VibrationCommand
		{
			public override byte SubID => 2;

			public override string SubName => "Play sample";


			public enum Sample : byte
			{
				Silence = 0,
				LowFrequencyBuzz = 1,
				HighFrequencyBuzz = 2,
				SoftClick = 3,
				HigherFrequencyBeep = 4,
				HardClick = 5,
				ShortBeep = 6,
				ShortHighBeep = 7,
			}

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				Sample sample = (Sample)reader.ReadByte();
				string unknown = reader.ReadHexBytes(3);
				return $"{sample} /? {unknown}";
			}
		}

		public class SendData : VibrationCommand
		{
			public override byte SubID => 8;

			public override string SubName => "Send data";
		}
	}
}
