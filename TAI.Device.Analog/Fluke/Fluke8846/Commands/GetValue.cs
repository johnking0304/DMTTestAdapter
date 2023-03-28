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

            string value = string.Format("{0}", Commands[(int)this.Type]);
            return value;
        }

        /// <summary>
        /// 
        /// 
        /// -1.000000E-07,A,0.000000E+00,NONE
        /// -1.000000E-03,V,0.000000E+00,NONE
        /// </summary>
        /// <param name="content"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool ParseResponse(string content, ref float value)
        {
            string[] values = content.Split(new char[1] { ','});
            if (values.Length > 0)
            {
                bool result =  float.TryParse(values[0], out value);
                if (this.Type == ChannelType.Current)
                {
                    value *= 1000;
                }
                return result;
            }
            value = 0;
            return false;
        }


    }



}
