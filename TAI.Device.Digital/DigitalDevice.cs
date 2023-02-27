﻿using System;
using DMT.Core.Models;
using DMT.Core.Protocols;

namespace TAI.Device
{

    public class DigitalDevice : Controller, IController
    {
        public ModbusTCPClient Channel { get; set; }


        public DigitalDevice() : base()
        {
            this.Caption = "DigitalDevice";
            this.Channel = new ModbusTCPClient(this.Caption);

        }

        public bool Active()
        {
            return this.Channel.hasError;
        }

        public bool Close()
        {
            return true;
        }

        public bool Initialize()
        {
            this.Channel.Initialize(Constants.Contant.DIGITAL_CONFIG);
            return true;
        }

        public bool Open()
        {
            return true;
        }

        public void Start()
        {
            return ;
        }

        public bool SetValue(int channelId, bool value)
        {

            //this.Channel.WriteMultipleRegisters();
            return true;
        }

        public bool GetValue(int channelId)
        {
            //this.Channel.ReadHoldingRegisters();
            return true;
        }

    }
}
