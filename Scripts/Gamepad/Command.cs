using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Switch2USBSniffer.Data.Gamepad
{
	internal static class Command
	{
		public static readonly string[] CommandNames = [
			"Not used",
			"NFC",
			"Flash Memory",
			"Initialization",
			"Unknown4",
			"Unknown5",
			"Unknown6",
			"Unknown7",
			"Charging Grip",
			"Player LEDs",
			"Vibration",
			"Battery",
			"Feature Select",
			"Firmware Update",
			"Unknown14",
			"Unknown15",
			"Firmware Info",
			"Unknown17",
			"Unknown18",
			"Unknown19",
			"Unknown20",
			"Bluetooth Pairing",
			"Unknown22",
			"Unknown23",
			"Unknown24",
		];

		public static bool DecodeCommand(byte[] data, [NotNullWhen(true)] out CommandInfo? info)
		{
			if(data.Length < 8)
			{
				info = null;
				return false;
			}

			using MemoryStream stream = new(data);
			BinaryObjectReader reader = new(stream, StreamOwnership.Retain, Endianness.Little);

			byte commandID = reader.ReadByte();
			CommandDirection direction = (CommandDirection)reader.ReadByte();
			CommandTransport transport = (CommandTransport)reader.ReadByte();
			byte subCommandID = reader.ReadByte();
			reader.Skip(sizeof(uint));

			if(!Enum.IsDefined(direction) || !Enum.IsDefined(transport))
			{
				info = null;
				return false;
			}

			ushort globalCommandID = (ushort)(commandID | (subCommandID << 8));
			string commandName = commandID >= CommandNames.Length ? $"Unknown{commandID:D2}" : CommandNames[commandID];
			string subCommandName = $"Unknown{subCommandID:D2}";
			string? commandData = null;

			

			if (SubCommand.SubCommands.TryGetValue(globalCommandID, out SubCommand? subcommand))
			{
				subCommandName = subcommand.SubName;

				try
				{
					switch (direction)
					{
						case CommandDirection.HostToDevice:
							commandData = subcommand.DecodeRequestInfo(reader);
							break;
						case CommandDirection.DeviceToHost:
							commandData = subcommand.DecodeResponseInfo(reader);
							break;
					}
				}
				catch(Exception exception)
				{
					commandData = exception.GetType().Name + " ";
					reader.Seek(8, SeekOrigin.Begin);
				}
			}

			if(reader.Position < reader.Length)
			{
				if(commandData != null)
				{
					commandData += " //? ";
				}
				else
				{
					commandData = "? ";
				}

				commandData += Util.ReadHexBytes(reader, (int)(reader.Length - reader.Position));
			}
			else if(commandData == null)
			{
				commandData = string.Empty;
			}

			info = new()
			{
				CommandID = commandID,
				CommandName = commandName,
				Direction = direction,
				Transport = transport,
				SubCommandID = subCommandID,
				SubCommandName = subCommandName,
				CommandData = commandData,
			};

			return true;
		}
	}
}
