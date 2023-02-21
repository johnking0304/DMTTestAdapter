using System;
using System.Collections.Generic;
using System.Text;
using DMTTestAapter;

namespace AnalogDevice.Fluke8846
{
    class Fluke8846 : DeviceMaster, IAnalogDevice
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



        public bool Initialize(string filename)
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
