﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;

namespace TAI.Device.Fluke.D7526
{

    public class SetValueCommand:FlukeCommand
    {
        private readonly string[] Units = new string[3] { "mA", "mV", "CEL" };
        private ChannelType Type { get; set; }
        private float Value { get; set; }
        public SetValueCommand(BaseDevice device, ChannelType type, float value):base(device)
        {
            this.Device = device;
            this.Type = type;
            this.Value = value;
        }

        public override string Pack()
        {

            string value = string.Format("OUT {0} {1}", this.Value ,Units[(int)this.Type]);
            return value;
        }

        
    }
}
