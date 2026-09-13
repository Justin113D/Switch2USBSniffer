using System;
using System.Text;

namespace Switch2USBSniffer.Data.USB
{
	internal class SetupRequest
	{
		public enum RequestDirection
		{
			HostToDevice = 0,
			DeviceToHost = 1
		}

		public enum RequestType
		{
			Standard = 0,
			Class = 1,
			Vendor = 2,
			Reserved = 3
		}

		public enum RequestRecipient
		{
			Device = 0,
			Interface = 1,
			Endpoint = 2,
			Other = 3
		}

		public enum DescriptorType
		{
			Device = 1,
			Configuration = 2,
			String = 3,
		}

		public RequestDirection Direction { get; private set; }
		public RequestType Type { get; private set; }
		public RequestRecipient Recipient { get; private set; }
		public byte Request { get; private set; }
		public ushort Value { get; private set; }
		public ushort Index { get; private set; }
		public ushort Length { get; private set; }

		public void FromData(byte[] data)
		{
			byte requestType = data[0];

			Direction = (RequestDirection)(requestType >> 7);
			Type = (RequestType)((requestType >> 5) & 0x3);
			Recipient = (RequestRecipient)(requestType & 0x1F);
			Request = data[1];
			Value = BitConverter.ToUInt16(data, 2);
			Index = BitConverter.ToUInt16(data, 4);
			Length = BitConverter.ToUInt16(data, 6);
		}

		public string ProcessResponse(byte[] data)
		{
			switch (Request)
			{
				case 0x06:
					DescriptorType descriptorType = (DescriptorType)(Value >> 8);
					if (descriptorType == DescriptorType.String && data.Length > 2)
					{
						return '"' + Encoding.Unicode.GetString(data, 2, data.Length - 2) + '"';
					}
					goto default;
				default:
					return $"[{data.Length}]";
			}
		}

		public override string ToString()
		{
			string info = string.Empty;

			if(Direction == RequestDirection.DeviceToHost && Type == RequestType.Standard && Recipient == RequestRecipient.Device)
			{
				switch (Request)
				{
					case 0x06: // get descriptor
						DescriptorType descriptorType = (DescriptorType)(Value >> 8);
						byte descriptorIndex = (byte)(Value & 0xFF);
						info = $"Get Descriptor {descriptorType}[{descriptorIndex}]";

						if(descriptorType == DescriptorType.String)
						{
							info += $" ({Index:X2})";
						}

						info += $" [{Length}]";
						break;
				}
			}

			if(string.IsNullOrEmpty(info))
			{
				info = $"{Request}: {Value}, {Index}[{Length}]";
			}

			return $"({Direction}, {Type}, {Recipient}) {info}";
		}
	}
}
