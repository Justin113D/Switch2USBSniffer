using Godot;
using Switch2USBSniffer.Data.Serial;
using Switch2USBSniffer.Data.Serial.Event;
using Switch2USBSniffer.Data.USB;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace Switch2USBSniffer.Scripts
{
	public partial class MainController : Node
	{
		private (string port, string description)[] _ports = [];
		private SnifferConnector? _connector;
		private USBLinkLayer? _usbLinkLayer;

		[Export]
		private PopupMenu? ConnectionMenu { get; set; }

		[Export]
		private GamepadOutputController? GamepadOutputController { get; set; }

		[Export]
		private RichTextLabel? CommandOutput { get; set; }

		[Export]
		private RichTextLabel? SetupOutput { get; set; }

		private bool _connecting;
		private string _commandOutputQueue = string.Empty;
		private string _setupOutputQueue = string.Empty;

		public override void _Ready()
		{
			base._Ready();
			RefreshPortList();
		}

		public override void _Process(double delta)
		{
			base._Process(delta);

			if(_commandOutputQueue.Length > 0)
			{
				CommandOutput!.Text += _commandOutputQueue;
				_commandOutputQueue = string.Empty;
			}

			if (_setupOutputQueue.Length > 0)
			{
				SetupOutput!.Text += _setupOutputQueue;
				_setupOutputQueue = string.Empty;
			}
		}

		public async void ConnectAsync(string port)
		{
			_connecting = true;
			(SnifferConnector.ConnectionResult result, SnifferConnector? connector) = await SnifferConnector.TryEstablishConnection(port);
			_connecting = false;
			if (result == SnifferConnector.ConnectionResult.Success)
			{
				GamepadOutputController!.ResetQueue();

				_usbLinkLayer = new();
				_usbLinkLayer.ReceivedHIDReport += GamepadOutputController!.UpdateReport;
				_usbLinkLayer.ReceivedCommand += OnReceivedCommand;
				_usbLinkLayer.ReceivedSetup += OnReceivedSetup;

				_connector = connector!;
				_connector.USBPackedReceived += OnUSBPacketReceived;

				for(int i = 0; i < _ports.Length; i++)
				{
					ConnectionMenu!.SetItemDisabled(i, true);
				}

				ConnectionMenu!.SetItemDisabled(_ports.Length + 1, true);
				ConnectionMenu!.SetItemDisabled(_ports.Length + 2, false);
			}
		}

		private void OnReceivedSetup(string info)
		{
			_setupOutputQueue += info + '\n';
		}

		private void OnReceivedCommand(string info)
		{
			_commandOutputQueue += info + '\n';
		}

		private void OnUSBPacketReceived(string port, USBPacketReceivedEventArgs args)
		{
			try
			{
				_usbLinkLayer?.ProcessPacket(args.Data, args.Timestamp);
			}
			catch
			{
				//Console.WriteLine(exception.Message);
			}

		}


		public void OnConnectionMenuItemPressed(int id)
		{
			switch (id)
			{
				case 1:
					RefreshPortList();
					break;
				case 2:
					Disconnect();
					break;
				default:
					ConnectAsync(_ports[id - 100].port);
					break;
			}
		}

		private void RefreshPortList()
		{
			for(int i = 0; i < ConnectionMenu!.ItemCount; i++)
			{
				if(ConnectionMenu.GetItemId(i) >= 100)
				{
					ConnectionMenu.RemoveItem(i);
					i--;
				}
			}

			_ports = SnifferConnector.ListAvailablePorts();
			for (int i = 0, index = ConnectionMenu.ItemCount; i < _ports.Length; i++, index++)
			{
				ConnectionMenu.AddItem("Connect to " + _ports[i].port, 100 + i);
				ConnectionMenu.SetItemIndex(index, i);
			}
		}

		private void Disconnect()
		{
			for (int i = 0; i < _ports.Length; i++)
			{
				ConnectionMenu!.SetItemDisabled(i, false);
			}

			ConnectionMenu!.SetItemDisabled(_ports.Length + 1, false);
			ConnectionMenu!.SetItemDisabled(_ports.Length + 2, true);

			_connector!.Dispose();
			_connector = null;

			_usbLinkLayer = null;
		}

	}
}
