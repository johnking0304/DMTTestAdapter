using DMT.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAI.Device
{
    public class BeamexMC6 : DeviceMaster,IAnalogDevice
    {
        public override  bool Active()
        {
            throw new NotImplementedException();
        }

        public override bool Close()
        {
            throw new NotImplementedException();
        }

        public override bool GetValue( ChannelType channelType, ref float value)
        {
            throw new NotImplementedException();
        }



        public override bool Initialize()
        {
            return true;
        }

        public override bool Open()
        {
            throw new NotImplementedException();
        }

        public override bool SetValue( ChannelType channelType, float value)
        {
            throw new NotImplementedException();
        }


    }
}
