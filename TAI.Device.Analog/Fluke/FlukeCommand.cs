using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;
using DMT.Core.Channels;


namespace TAI.Device
{
    public class FlukeCommand:BaseCommand
    {
        public FlukeCommand(BaseDevice device):base(device)
        { 
            
        }

        public override string PackageString()
        {
            return string.Format("{0}\r", this.Pack());
        }



        public override bool ParseResponse(string content ,ref float value)
        {
            return true;
        }


        public override bool ParseResponse(string content, ref string value)
        {
            return true;
        }

        public override bool ParseResponse(string content)
        {
            return true;
        }

    }
}
