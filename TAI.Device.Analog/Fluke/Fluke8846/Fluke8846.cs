using System;
using System.Collections.Generic;
using System.Text;
using DMT.Core.Channels;
using DMT.Core.Models;
using TAI.Device.Fluke.D8846;

namespace TAI.Device
{
    class Fluke8846 : DeviceMaster, IAnalogDevice
    {
        public Fluke8846() : base()
        {
            this.Caption = "Fluke8846";
            this.Channel = new SerialChannel(this.Caption);
            this.Channel.AttachObserver(this.subjectObserver.Update);
        }

        public  bool Active()
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


        public override void LoadFromFile(string fileName)
        {
            this.Channel.LoadFromFile(fileName);

        }

        public bool Open()
        {
            this.Channel.Open();
            return this.Channel.Active();
        }

        public bool SetValue(ChannelType channelType, float value)
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
                bool result = command.ParseResponse(content, ref value);
                this.Identify = value;
                return result;
            }
            return false;
        }

        public bool Initialize()
        {
            InitializeCommand command = new InitializeCommand(this);
            this.SendCommand(command.PackageString());
            return this.GetIdentify();
        }

        public bool GetValue(ChannelType channelType, ref float value)
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
