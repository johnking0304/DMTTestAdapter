using System;
using System.Collections.Generic;
using System.Text;
using TAI.Device.Fluke.D7526;
using DMT.Core.Channels;

namespace TAI.Device
{
    class Fluke7526 : DeviceMaster, IAnalogDevice
    {
        public Fluke7526() : base()
        {
            this.Caption = "Fluke7526";
            this.Channel = new SerialChannel(this.Caption);
            this.Channel.AttachObserver(this.subjectObserver.Update);
        }

        public bool Active()
        {
            if (this.Channel != null)
            {
                return this.Channel.Active();
            }
            return false;
        }

        public bool Close()
        {
            return this.Channel.Close();
        }





        public bool Initialize(string fileName)
        {
            this.LoadFromFile(fileName);
            this.Channel.LoadFromFile(fileName);
            return true;

        }

        public bool Open()
        {
            this.Channel.Open();
            return this.Channel.Active();
        }

        public bool SetValue(ChannelType channelType, double value)
        {
            SetValueCommand command = new SetValueCommand(this, channelType, value);
            this.SendCommand(command.PackageString());
            return true;
        }

        public bool GetIdentify()
        {
            GetIdentifyCommand command = new GetIdentifyCommand(this);
            this.SendCommand(command.PackageString());
            string content = this.Channel.Receive();
            if (this.Channel.LastErrorCode == ChannelResult.OK)
            {
                string value = "";
                command.ParseResponse(content, ref value);
                this.Identify = value;
                return true;
            }
            return false;
        }

        public bool Initialize()
        {
            InitializeCommand command = new InitializeCommand(this);
            this.SendCommand(command.PackageString());
            return true;
        }

        public bool GetValue(ChannelType channelType, ref double value)
        {
            GetValueCommand command = new GetValueCommand(this, channelType);
            this.SendCommand(command.PackageString());
            string content = this.Channel.Receive();
            if (this.Channel.LastErrorCode == ChannelResult.OK)
            {
                return command.ParseResponse(content, ref value);
            }
            return false;
        }


    }
}
