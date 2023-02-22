using System;
using System.Collections.Generic;
using System.Text;

namespace TAI.Device
{
    public class Fluke7526 : DeviceMaster, IAnalogDevice
    {
        public bool Active()
        {
            throw new NotImplementedException();
        }

        public bool Close()
        {
            throw new NotImplementedException();
        }

        public bool GetValue(int channelId, ChannelType channelType, ref double value)
        {
            throw new NotImplementedException();
        }



        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public bool Open()
        {
            throw new NotImplementedException();
        }

        public bool SetValue(int channelId, ChannelType channelType, double value)
        {
            throw new NotImplementedException();
        }


    }
}
