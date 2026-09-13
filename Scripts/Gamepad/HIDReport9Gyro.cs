using Godot;

namespace Switch2USBSniffer.Scripts.Gamepad
{
	internal struct HIDReport9Gyro
	{
		public ushort SampleNumber {get; private set;}
		public ushort SampleNumberDelta {get; private set;}
		public byte ImuState { get; private set; }

		public int OmittedIndex { get; private set; }

		public Vector3 RawQuaternion { get; private set; }

		public Quaternion Quaternion { get; private set; }

		public Vector3 Accel { get; private set; }

		public Vector3 Gyro { get; private set; }

		public void FromBytes(byte[] data)
		{

			if(data.Length == 0)
			{
				SampleNumber = default;
				SampleNumberDelta = default;
				ImuState = default;
				OmittedIndex = default;
				RawQuaternion = default;
				Quaternion = default;
				return;
			}

			SampleNumber = (ushort)(data[0] | (data[1] & 0xF) << 8);
			SampleNumberDelta = (ushort)((data[1] >> 4) | (data[2] << 4));
			ImuState = data[3];

			OmittedIndex = data[4];

			uint quaternion1 = (uint)((data[5] >> 1) | data[6] << 7 | data[7] << 15 | ((data[8] & 0x3) << 23));
			uint quaternion2 = (uint)(data[9] | data[10] << 8 | data[11] << 16 | ((data[12] & 0x1) << 24));
			uint quaternion3 = (uint)((data[12] >> 7) | data[13] << 1 | data[14] << 9 | data[15] << 17);

			const double quatFactor = (1.0 / 0x1FFFFFF) * 2;
			float quaternion1f = (float)(quaternion1 * quatFactor - 1);
			float quaternion2f = (float)(quaternion2 * quatFactor - 1);
			float quaternion3f = (float)(quaternion3 * quatFactor - 1);

			RawQuaternion = new(quaternion1f, quaternion2f, quaternion3f);

			Quaternion = OmittedIndex switch
			{
				1 => new(1, quaternion1f, quaternion2f, quaternion3f),
				2 => new(quaternion3f, 1, quaternion1f, quaternion2f),
				3 => new(quaternion2f, quaternion3f, 1, quaternion1f),
				_ => new(quaternion1f, quaternion2f, quaternion3f, 1),
			};


			if (Quaternion.W < 0)
			{
				Quaternion = -Quaternion;
			}

			Quaternion = Quaternion.Normalized();

			uint accelX = (uint)(data[16] | data[17] << 8 | data[18] << 16 | data[19] << 24);
			uint accelY = (uint)(data[20] | data[21] << 8 | data[22] << 16 | data[23] << 24);
			uint accelZ = (uint)(data[24] | data[25] << 8 | data[26] << 16 | data[27] << 24);

			float accelXf = unchecked((int)accelX) / (float)0x10000000;
			float accelYf = unchecked((int)accelY) / (float)0x10000000;
			float accelZf = unchecked((int)accelZ) / (float)0x10000000;

			Accel = new(accelXf, accelYf, accelZf);

			//short gyroX = unchecked((short)(data[16] | data[17] << 8));
			//short gyroY = unchecked((short)(data[20] | data[21] << 8));
			//short gyroZ = unchecked((short)(data[24] | data[25] << 8));

			//Gyro = new(gyroX, gyroY, gyroZ);
		}
	}
}
