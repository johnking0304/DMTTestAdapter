using System;
using System.Collections.Generic;
using System.Text;
using DMT.Core.Models;

namespace TAI.Device
{

    public enum DeviceModel
    {
        BeamexMC6=0,
        Fluke7526=1,
        Fluke8846=2,
        
    }

    //0 电流模式
    //1 电压模式
    //2 电阻模式
    public enum ChannelType
    { 
        None =-1,
        Current =0,
        Voltage =1,
        Resistance =2,
    }





    public interface IAnalogDevice
    {        
        bool Initialize();
        bool Open();
        bool Close();
        bool Active();
        bool GetValue( ChannelType channelType,ref float value);

        bool SetValue( ChannelType channelType,float value);

        void LoadFromFile(string fileName);

        void SaveToFile(string fileName);

        StatusMessage GetStatusMessage();







    }
}



