using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Channels;
using DMT.Core.Models;

namespace TAI.Device.Fluke.D7526
{
    public class GetValueCommand : FlukeCommand
    {
        private readonly string[] Commands = new string[2] { "ISO_MEAS DCI\rVAL?", "ISO_MEAS DC100V\rVAL?" };
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
