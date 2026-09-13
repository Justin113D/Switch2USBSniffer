using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using Switch2USBSniffer.Data.Gamepad;
using Switch2USBSniffer.Scripts.Gamepad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Switch2USBSniffer.Scripts
{
	internal partial class GamepadOutputController : Control
	{
		private struct ReportQueue
		{
			public uint timestamp;
			public byte[] data;

			public ReportQueue(uint timestamp, byte[] data)
			{
				this.timestamp = timestamp;
				this.data = data;
			}
		}

		private readonly Queue<ReportQueue> _queue = [];

		

		private bool _refreshRequired = true;

		[Export]
		private TextEdit? DataText { get; set; }

		[Export]
		private TextEdit? MotionDataText { get; set; }

		[Export]
		private Array<Control>? ButtonControls { get; set; }

		[Export]
		private Control? LeftStickSpot { get; set; }

		[Export]
		private Label? LeftStickText { get; set; }

		[Export]
		private Control? RightStickSpot { get; set; }

		[Export]
		private Label? RightStickText { get; set; }

		[Export]
		private Node? GyroPlot { get; set; }

		[Export]
		private Node3D? GamepadModel { get; set; }

		private static StringName GyroPlotUpdateMethod = "update_plots";

		private uint _queueTimestamp;
		private bool _updated;

		public void ResetQueue()
		{
			_queue.Clear();
			_queueTimestamp = 0;
		}

		public void UpdateReport(byte[] data, uint timestamp)
		{
			if (data[0] != 9)
			{
				return;
			}

			if(_updated && _queue.Count > 0)
			{
				_queueTimestamp = _queue.ToArray()[^1].timestamp;
			}

			_queue.Enqueue(new(timestamp, data));
			_updated = false;
		}

		public override void _Process(double delta)
		{
			base._Process(delta);

			if(_queue.Count == 0)
			{
				return;
			}

			_updated = true;

			if (_queueTimestamp == 0)
			{
				ReportQueue item = _queue.ToArray()[^1];
				_queue.Clear();

				_queueTimestamp = item.timestamp;
				_queue.Enqueue(item);
			}
			else
			{
				_queueTimestamp += (uint)(delta * 1000000);
			}

			ReportQueue lastItem = default;
			HIDReport9 lastReport = new();
			HIDReport9Gyro lastGyroReport = new();
			bool hasLast = false;

			List<double>[] values = [[], [], [], [], [], [], [], [], [], []];

			while (_queue.TryPeek(out ReportQueue currentItem) && currentItem.timestamp <= _queueTimestamp)
			{
				_queue.Dequeue();
				hasLast = true;
				lastItem = currentItem;
				lastReport.FromBytes(lastItem.data);
				lastGyroReport.FromBytes(lastReport.MotionData);

				if(lastReport.MotionData.Length == 0)
				{
					foreach (List<double> item in values)
					{
						item.Add(double.NaN);
					}
				}
				else
				{
					values[0].Add(lastGyroReport.Quaternion.W);
					values[1].Add(lastGyroReport.Quaternion.X);
					values[2].Add(lastGyroReport.Quaternion.Y);
					values[3].Add(lastGyroReport.Quaternion.Z);


					values[4].Add(lastGyroReport.Accel.X);
					values[5].Add(lastGyroReport.Accel.Y);
					values[6].Add(lastGyroReport.Accel.Z);

					values[7].Add(lastGyroReport.Gyro.X);
					values[8].Add(lastGyroReport.Gyro.Y);
					values[9].Add(lastGyroReport.Gyro.Z);

				}
			}

			if(!hasLast)
			{
				return;
			}

			foreach (List<double> item in values)
			{
				item.Add(double.NaN);
			}
			
			GyroPlot!.Call(GyroPlotUpdateMethod, new Array<double[]>(values.Select(x => x.ToArray())));

			DataText!.Text = string.Empty;

			for (int i = 0; i < lastItem.data!.Length; i++)
			{
				DataText!.Text += lastItem.data[i].ToString("X2");

				if (i < lastItem.data.Length - 1)
				{
					if (i % 8 == 7)
					{
						DataText!.Text += '\n';
					}
					else if (i % 4 == 3)
					{
						DataText!.Text += "  ";
					}
					else
					{
						DataText!.Text += ' ';
					}
				}
			}

			for (int i = 0; i < ButtonControls!.Count; i++)
			{
				bool active = lastReport.Buttons.HasFlag((HIDReport9.ButtonFlags)(1 << i));

				ButtonControls[i].SelfModulate = active ? Colors.Green : Colors.Gray;
			}

			LeftStickSpot!.OffsetTransformPositionRatio = lastReport.LeftStick * new Vector2(3, -3);
			LeftStickText!.Text = $"X: {lastReport.LeftStick.X}\nY: {lastReport.LeftStick.Y}";

			RightStickSpot!.OffsetTransformPositionRatio = lastReport.RightStick * new Vector2(3, -3);
			RightStickText!.Text = $"X: {lastReport.RightStick.X}\nY: {lastReport.RightStick.Y}";

			if (lastGyroReport.Quaternion.IsFinite() && lastGyroReport.Quaternion.LengthSquared() > 0)
			{
				GamepadModel!.Quaternion = lastGyroReport.Quaternion * new Quaternion(Vector3.Right, float.Pi * 0.5f);
			}
			else
			{
				GamepadModel!.Quaternion = Quaternion.Identity;
			}

			MotionDataText!.Text = $"Sample number: {lastGyroReport.SampleNumber}\n" +
				$"Delta:{lastGyroReport.SampleNumberDelta}\n" +
				$"State: {lastGyroReport.ImuState}\n" +
				$"Omitted Index: {lastGyroReport.OmittedIndex}\n" +
				$"Quaternion 1: {lastGyroReport.RawQuaternion.X}\n" +
				$"Quaternion 2: {lastGyroReport.RawQuaternion.Y}\n" +
				$"Quaternion 3: {lastGyroReport.RawQuaternion.Z}\n" +
				$"=> ({lastGyroReport.Quaternion.W:F3}, {lastGyroReport.Quaternion.X:F3}, {lastGyroReport.Quaternion.Y:F3}, {lastGyroReport.Quaternion.Z:F3})\n" +
				$"Accel X: {lastGyroReport.Accel.X}\n" +
				$"Accel Y: {lastGyroReport.Accel.Y}\n" +
				$"Accel Z: {lastGyroReport.Accel.Z}\n" +
				$"Gyro X: {lastGyroReport.Gyro.X}\n" +
				$"Gyro Y: {lastGyroReport.Gyro.Y}\n" +
				$"Gyro Z: {lastGyroReport.Gyro.Z}\n" +
				$"\n";

			byte[] motion = lastReport.MotionData;

			for (int i = 4; i < motion.Length; i++)
			{
				MotionDataText!.Text += motion[i].ToString("B8");

				if (i % 4 == 3)
				{
					MotionDataText!.Text += '\n';
				}
				else
				{
					MotionDataText!.Text += ' ';
				}
			}
		}
	}
}
