using Amicitia.IO.Binary;
using System.Linq;

namespace Switch2USBSniffer.Data.Gamepad.Commands
{
	internal abstract class BluetoothPairingCommand : SubCommand
	{
		public override byte ID => 21;

		public static string ReadAddress(BinaryObjectReader reader)
		{
			byte[] address = new byte[6];
			for (int j = address.Length - 1; j >= 0; j--)
			{
				address[j] = reader.ReadByte();
			}
			return string.Join(':', address.Select(x => x.ToString("X2")));
		}

		public static string ReadLTK(BinaryObjectReader reader)
		{
			byte[] address = new byte[16];
			for (int j = address.Length - 1; j >= 0; j--)
			{
				address[j] = reader.ReadByte();
			}
			return string.Join(null, address.Select(x => x.ToString("X2")));
		}

		public class ExchangeAddress : BluetoothPairingCommand
		{
			public override byte SubID => 1;

			public override string SubName => "Exchange address";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(1);
				string result = $"{unknown} ?/ ";

				byte count = reader.ReadByte();
				if(count == 0)
				{
					result += "No Addresses";
				}
				else
				{
					string[] addresses = new string[count];
					for (int i = 0; i < count; i++)
					{
						addresses[i] = ReadAddress(reader);
					}
					result += string.Join(", ", addresses);
				}

				return result;
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(2);

				string result = $"{unknown} ?/ ";

				byte count = reader.ReadByte();
				if (count == 0)
				{
					result += "No Addresses";
				}
				else
				{
					string[] addresses = new string[count];
					for (int i = 0; i < count; i++)
					{
						addresses[i] = ReadAddress(reader);
					}
					result += string.Join(", ", addresses);
				}

				return result;
			}

		}

		public class ConfirmLTK : BluetoothPairingCommand
		{
			public override byte SubID => 2;

			public override string SubName => "Confirm LTK";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(1);
				string challenge = ReadLTK(reader);

				return $"{unknown} ?/ {challenge}";
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(1);
				string challenge = ReadLTK(reader);

				return $"{unknown} ?/ {challenge}";
			}
		}

		public class Finalize : BluetoothPairingCommand
		{
			public override byte SubID => 3;

			public override string SubName => "Finalize";

		}

		public class ExchangeLTK : BluetoothPairingCommand
		{
			public override byte SubID => 4;

			public override string SubName => "Exchange LTK";

			public override string? DecodeRequestInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(1);
				string challenge = ReadLTK(reader);

				return $"{unknown} ?/ {challenge}";
			}

			public override string? DecodeResponseInfo(BinaryObjectReader reader)
			{
				string unknown = reader.ReadHexBytes(1);
				string challenge = ReadLTK(reader);

				return $"{unknown} ?/ {challenge}";
			}
		}
	}
}
