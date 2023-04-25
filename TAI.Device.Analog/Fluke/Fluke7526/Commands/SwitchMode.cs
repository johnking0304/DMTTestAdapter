using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;

namespace TAI.Device.Fluke.D7526
{

    public class SwitchModeCommand : FlukeCommand
    {
        public RTDType RTDType { get; set; }
        /*        public SwitchModeCommand(BaseDevice device):base(device)
                {
                    this.Device = device;
                }*/

        public SwitchModeCommand(BaseDevice device, RTDType RtdType) : base(device)
        {
            this.RTDType = RtdType;
        }

        public override string Pack()
        {
            string value = string.Format("RTD_TYPE  {0}",RTDType.ToString());
            return value;
        }

        
    }
}
