using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAI.NuCONController
{
    public class NuCONController
    {
        public bool SetChannelValue(int stationId, string channel, int dataType, float value)
        {
            return true;
        }

        public bool GetChannelValue(int stationId, string channel, int dataType, out float value)
        {
            value = 0;
            return true;
        }


    }
}
