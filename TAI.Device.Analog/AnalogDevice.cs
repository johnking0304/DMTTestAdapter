using System;
using DMT.Core.Channels;
using DMT.Core.Models;


namespace TAI.Device
{


    public enum DeviceCommand
    {
        Initialize = 100,
        Identify   =101,
        GetValue  =201,
        SetValue  =202, 

    }



    public class DeviceMaster : BaseDevice
    {
        public string Identify { get; set; }

        public TCPClientChannel TCPChannel {
            get {
                return (TCPClientChannel)this.Channel;
            }
        }

        public DeviceMaster() : base()
        {
            this.Identify = "";

        }

        public StatusMessage GetStatusMessage()
        {
            return this.StatusMessage;
        }


        public virtual bool Initialize()
        {
            return true;
        }
        public virtual bool Open()
        {
            return true;
        }
        public virtual bool Close()
        {
            return true;
        }
        public virtual bool Active()
        {
            return true;
        }
        public virtual bool GetValue(ChannelType channelType, ref float value)
        {
            return true;
        }

        public virtual bool SetValue(ChannelType channelType, float value)
        {
            {
                return true;
            }
        }


        public override bool SendCommand(string command)
        {
            return this.Channel.SendCommandNoReply(command);
        }

        public void Start()
        {
            this.StartThread();
        }

        public override void ProcessEvent()
        {
            if (!this.Active())
            {
                this.TCPChannel.ReConnect();
            }
        }

    }


}
