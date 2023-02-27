using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Channels;
using DMT.Core.Models;

namespace TAI.Device.Fluke.D8846
{
    public class GetValueCommand : FlukeCommand
    {
        private readonly string[] Commands = new string[2] { "MEAS:curr:dc? 0.1", "MEAS:volt:dc?  100" };
        private ChannelType Type { get; set; }
        public GetValueCommand(BaseDevice device, ChannelType type) : base(device)
        {
            this.Device = device;
        }

        public override string Pack()
        {
 

            string value = string.Format("{0}", (int)this.Type, Commands[(int)this.Type]);
            return value;
        }


        public override bool ParseResponse(string content, ref double value)
        {
            return true;
        }


    }



}
