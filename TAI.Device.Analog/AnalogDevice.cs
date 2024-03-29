﻿using System;
using DMT.Core.Models;


namespace TAI.Device
{


    public enum DeviceCommand
    {
        Initialize = 100,
        Identify   =101,
        GetValue  =201,
        SetValue  =201, 

    }



    public class DeviceMaster : BaseDevice
    {
        public string Identify { get; set; }

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


        public void Delay(int milliseconds)
        {
            DateTime now = DateTime.Now;
            Boolean inTime = true;
            while (inTime)
            {
                TimeSpan value = DateTime.Now - now;
                inTime = value.TotalMilliseconds < milliseconds;
            }
            return;
        }


    }


}
