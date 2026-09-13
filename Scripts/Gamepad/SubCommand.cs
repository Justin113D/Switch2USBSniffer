using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Switch2USBSniffer.Data.Gamepad
{
	internal abstract class SubCommand
	{
		public abstract byte ID { get; }

		public string Name => ID >= Command.CommandNames.Length ? $"Unknown{ID}" : Command.CommandNames[ID];

		public abstract byte SubID { get; }

		public abstract string SubName { get; }

		public ushort GlobalID => (ushort)(ID | (SubID << 8));

		public static ReadOnlyDictionary<ushort, SubCommand> SubCommands { get; }


		static SubCommand()
		{
			Dictionary<ushort, SubCommand> subCommands = [];

			foreach(Type type in typeof(SubCommand).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(SubCommand)) && !x.IsAbstract))
			{
				SubCommand subCommand = (SubCommand)Activator.CreateInstance(type)!;
				subCommands.Add(subCommand.GlobalID, subCommand);
			}

			SubCommands = new(subCommands);
		}


		public virtual string? DecodeRequestInfo(BinaryObjectReader reader)
		{
			return null;
		}

		public virtual string? DecodeResponseInfo(BinaryObjectReader reader)
		{
			return null;
		}
	}
}
