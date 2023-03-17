using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;

namespace TAI.Device.Fluke.D7526
{

    public class OperateCommand:FlukeCommand
    {
        public OperateCommand(BaseDevice device):base(device)
        {
            this.Device = device;
        }

        public override string Pack()
        {
            string value = string.Format("OPER");
            return value;
        }

        
    }
}
