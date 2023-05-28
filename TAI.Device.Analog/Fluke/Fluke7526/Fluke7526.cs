using System;
using System.Collections.Generic;
using System.Text;
using TAI.Device.Fluke.D7526;
using DMT.Core.Channels;
using DMT.Core.Models;

namespace TAI.Device
{

    public enum RTDType
    { 
        None =1,
        PT385_100 =2,
    }

    class Fluke7526 : DeviceMaster, IAnalogDevice
    {

        public ChannelType ChannelType { get; set; }
        public RTDType RTDType { get; set; }
        public Fluke7526() : base()
        {
            this.Caption = "Fluke7526";
            //this.Channel = new SerialChannel(this.Caption);
            this.Channel = new TCPClientChannel(this.Caption);
            this.Channel.AttachObserver(this.subjectObserver.Update);
            this.StatusMessage.Name = this.Caption;
            this.RTDType = RTDType.None;
            this.ChannelType = ChannelType.None;
        }

        public override bool Active()
        {
            if (this.Channel != null)
            {
                return this.Channel.Active();
            }
            return false;
        }

        public override bool Close()
        {
            return this.Channel.Close();
        }


        public override void LoadFromFile(string fileName)
        {
            
            this.Channel.LoadFromFile(fileName);
        }

        public override bool Open()
        {
            this.Channel.OpenSync();
            return this.Channel.Active();
        }

        public override bool SetValue(ChannelType channelType, float value)
        {
            if (channelType == ChannelType.Resistance)
            {
                if (this.RTDType != RTDType.PT385_100)
                {
                    FlukeCommand switchMode = new SwitchModeCommand(this, this.RTDType);
                    this.SendCommand(switchMode.PackageString());
                    this.RTDType = RTDType.PT385_100;
                }
            }
            else
            {
                this.RTDType = RTDType.None;
            }


            FlukeCommand command = new SetValueCommand(this, channelType, value);
            this.SendCommand(command.PackageString());

            if (channelType == ChannelType.Resistance || channelType == ChannelType.Voltage)
            {
                if (this.ChannelType != channelType)
                {
                    command = new StandByCommand(this);
                    this.SendCommand(command.PackageString());

                    this.Delay(1000);
                    this.ChannelType = channelType;
                }
            }
            else
            {
                command = new StandByCommand(this);
                this.SendCommand(command.PackageString());

                this.Delay(1000);
                this.ChannelType = channelType;
            }



            command = new OperateCommand(this);
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

        public override bool Initialize()
        {
            InitializeCommand command = new InitializeCommand(this);
            this.SendCommand(command.PackageString());

            return  this.GetIdentify();
        }

        public override bool GetValue(ChannelType channelType, ref float value)
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
