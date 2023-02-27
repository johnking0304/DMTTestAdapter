using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;

namespace TAI.Device.Fluke.D7526
{
    public class GetIdentifyCommand : FlukeCommand
    {
        public GetIdentifyCommand(BaseDevice device) : base(device)
        {
        }

        public override string Pack()
        {
            return string.Format("*IDN?");;
        }


        

    }
}
