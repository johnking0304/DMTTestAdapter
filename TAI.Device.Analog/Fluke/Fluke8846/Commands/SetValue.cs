using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;

namespace TAI.Device.Fluke.D8846
{


    public class SetValueCommand:FlukeCommand
    {
        private readonly string[] Units = new string[3] { "mA", "V", "Ohm" };
        private ChannelType Type { get; set; }
        private double Value { get; set; }
        public SetValueCommand(BaseDevice device, ChannelType type, double value):base(device)
        {
            this.Device = device;
        }

        public override string Pack()
        {
            string value = string.Format("OUT {0} {1}",(int)this.Type,Units[(int)this.Type]);
            return value;
        }

        
    }
}
