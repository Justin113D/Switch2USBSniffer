using Amicitia.IO.Binary;

namespace Switch2USBSniffer.Data.Gamepad
{
	internal static class Util
	{
		public static string ReadHexBytes(this BinaryObjectReader reader, int count)
		{
			string[] values = new string[count];
			for(int i = 0; i < count; i++)
			{
				values[i] = reader.ReadByte().ToString("X2");
			}
			return string.Join(' ', values);
		}
	}
}
