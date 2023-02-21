using System;
using System.Collections.Generic;
using System.Text;

namespace AnalogDevice
{

    public enum ChannelType
    { 
        Current,
        Voltage,
    }


    public interface IAnalogDevice
    {
        bool Initialize(string filename);
        bool Open();
        bool Close();
        bool Active();
        bool GetValue(int channelId, ChannelType channelType,ref double value);

        bool SetValue(int channelId, ChannelType channelType,double value);
        

    }
}



