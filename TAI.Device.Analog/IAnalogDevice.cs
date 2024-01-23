using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Description("未知")]
        None =-1,
        [Description("电流型")]
        Current =0,
        [Description("电压型")]
        Voltage =1,
        [Description("温度型")]
        Resistance =2,
        [Description("脉冲型")]
        Pulse =3,
        [Description("数字量型")]
        DIO =4,
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



