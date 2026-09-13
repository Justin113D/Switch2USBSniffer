using Amicitia.IO.Binary;
using Godot;
using System;
using System.IO;

namespace Switch2USBSniffer.Data.Gamepad
{
	internal struct HIDReport9
	{
		[Flags]
		public enum PowerInfoFlags : byte
		{
			ExternalPower = 0x01,
			Charging = 0x02,
			BatteryLevel0 = 0x04,
			BatteryLevel1 = 0x08,
			BatteryLevel2 = 0x10,
			BatteryLevel3 = 0x20
		}

		[Flags]
		public enum ButtonFlags : uint
		{
			B = 0x01,
			A = 0x02,
			Y = 0x04,
			X = 0x08,
			R = 0x10,
			ZR = 0x20,
			Plus = 0x40,
			RightStick = 0x80,

			Down = 0x0100,
			Right = 0x0200,
			Left = 0x0400,
			Up = 0x0800,
			L = 0x1000,
			ZL = 0x2000,
			Minus = 0x4000,
			LeftStick = 0x8000,

			Home = 0x010000,
			Capture = 0x020000,
			GR = 0x040000,
			GL = 0x080000,
			C = 0x100000,

		}

		public byte Counter { get; private set; }

		public PowerInfoFlags PowerInfo { get; private set; }

		public ButtonFlags Buttons { get; private set; }

		public Vector2 LeftStick { get; private set; }

		public Vector2 RightStick { get; private set; }

		public byte Unknown1 { get; private set; }

		public byte LeftGCTrigger { get; private set; }

		public byte RightGCTrigger { get; private set; }

		public byte[] MotionData { get; private set; }

		private static Vector2 ReadStick(BinaryValueReader reader)
		{
			uint value = reader.ReadUInt32();
			reader.Seek(-1, SeekOrigin.Current);

			const float normalizeFactor = 1 / (0xFFF * 0.5f);

			float x = (value & 0xFFF) * normalizeFactor - 1;
			float y = ((value >> 12) & 0xFFF) * normalizeFactor - 1;

			return new(x, y);
		}

		public void FromBytes(byte[] data)
		{
			using MemoryStream stream = new(data);
			BinaryValueReader reader = new(stream, Amicitia.IO.Streams.StreamOwnership.Retain, Endianness.Little);

			reader.ReadByte(); // report id
			Counter = reader.ReadByte();
			PowerInfo = (PowerInfoFlags)reader.ReadByte();
			Buttons = (ButtonFlags)reader.ReadUInt32();
			reader.Seek(-1, SeekOrigin.Current);

			LeftStick = ReadStick(reader);
			RightStick = ReadStick(reader);

			Unknown1 = reader.ReadByte();

			LeftGCTrigger = reader.ReadByte();
			RightGCTrigger = reader.ReadByte();

			byte motionDataLength = reader.ReadByte();
			MotionData = reader.ReadArray<byte>(motionDataLength);
		}
	}
}
