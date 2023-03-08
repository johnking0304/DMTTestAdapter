using DMT.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAI.Device
{
    public class BeamexMC6 : DeviceMaster,IAnalogDevice
    {
        public bool Active()
        {
            throw new NotImplementedException();
        }

        public bool Close()
        {
            throw new NotImplementedException();
        }

        public bool GetValue( ChannelType channelType, ref float value)
        {
            throw new NotImplementedException();
        }



        public bool Initialize()
        {
            return true;
        }

        public bool Open()
        {
            throw new NotImplementedException();
        }

        public bool SetValue( ChannelType channelType, float value)
        {
            throw new NotImplementedException();
        }


    }
}
