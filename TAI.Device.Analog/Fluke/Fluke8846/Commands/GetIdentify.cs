using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;

namespace TAI.Device.Fluke.D8846
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

        public override bool ParseResponse(string content, ref string value)
        {
            if (!string.IsNullOrEmpty(content))
            {
                string[] list = content.Split(',');
                if (list.Length >= 3)
                {
                    value = string.Format("{0}-{1}",list[1],list[2]);
                    return true;
                }
            }
            return false;
        }

    }
}
