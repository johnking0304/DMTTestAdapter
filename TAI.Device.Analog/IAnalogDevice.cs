using System;
using System.Collections.Generic;
using System.Text;

namespace TAI.Device
{

    public enum DeviceModel
    {
        BeamexMC6,
        Fluke7526,
        Fluke8846,
        
    }


    public enum ChannelType
    { 
        Current,
        Voltage,
    }


    public interface IAnalogDevice
    {
        bool Initialize();
        bool Open();
        bool Close();
        bool Active();
        bool GetValue(int channelId, ChannelType channelType,ref double value);

        bool SetValue(int channelId, ChannelType channelType,double value);

        void LoadFromFile(string fileName);

        void SaveToFile(string fileName);
        

    }
}



