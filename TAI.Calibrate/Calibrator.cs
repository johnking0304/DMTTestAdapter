using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Channels;
using DMT.Core.Models;
using TAI.Test.Scheme;
using DMT.Core.Utils;
using TAI.Device;

namespace TAI.Calibrator
{

    public class CalibrateValueItem
    { 
        public string Caption { get; set; }
        public float AnalogDeviceValue { get; set; } 
        public float CalibrateValue { get; set; }

        public string SetFormat { get; set; }

        public CalibrateValueItem(string caption)
        {
            this.Caption = caption;
        }

    }


    public class CalibrateConfig
    {
        public const int ITEM_COUNT = 2;
        public const int ITEM_MIN_INDEX = 0;
        public const int ITEM_MAX_INDEX = 1;
        public const string Caption = "CalibrateConfig";

        public static string[] ItemCaptions = new string[ITEM_COUNT] { "Min", "Max" };
        public int BaseIndex { get; set; }

        public ChannelType ChannelDataType { get; set; }
        public string SaveFormat { get; set; }
        public CalibrateValueItem[] Items { get; set; }

        public CalibrateConfig()
        {
            this.Items = new CalibrateValueItem[ITEM_COUNT];
            for (int i = 0; i < ITEM_COUNT; i++)
            {
                this.Items[i] = new CalibrateValueItem(ItemCaptions[i]);
            }
        }


        public void LoadFormFile(string fileName)
        {

            this.BaseIndex = IniFiles.GetIntValue(fileName, Caption, "BaseIndex", 0);
            this.SaveFormat = IniFiles.GetStringValue(fileName, Caption, "SaveFormat", "io/cr -w{0}");
            this.ChannelDataType =  (ChannelType)IniFiles.GetIntValue(fileName, "TestSchemeConfig", "ChannelDataType", (int)ChannelType.None);

            foreach (CalibrateValueItem item in this.Items)
            {
                string ItemSection = string.Format("{0}{1}", Caption, item.Caption);
                item.AnalogDeviceValue = (float)IniFiles.GetFloatValue(fileName, ItemSection, "AnalogDeviceValue",0.0f);
                item.CalibrateValue = (float)IniFiles.GetFloatValue(fileName, ItemSection, "CalibrateValue", 0.0f);
                item.SetFormat = IniFiles.GetStringValue(fileName, ItemSection, "SetFormat", "io/cr -T-s{0}-a-min {1}");
            }


        }
    }



    public class BaseCalibrator:BaseController
    {

        public UDPClientChannel UDPClient { get; set; }
        public UDPService UDPService { get; set; }

        public CardModule ActiveCardModule { get; set; }

        public CalibrateConfig CalibrateConfig { get; set; }

        public SetChannelValueDelegate SetChannelValue { get; set; }
        public GetChannelValueDelegate GetChannelValue { get; set; }

        public ushort SequenceNumber { get; set; }


        public int ActiveChannelIndex { get; set; }


        public BaseCalibrator()
        {
            this.UDPService = new UDPService();
            this.UDPClient = new UDPClientChannel();
            this.CalibrateConfig = new CalibrateConfig();
            this.SequenceNumber = 0;
        }



        public void ConnectModule()
        {
            this.NotifyMessage("发送CONNECT 连接命令");
            CalibrateCommand command = new ConnectCommand(this);
            byte[] buffer =command.Package();
            this.UDPClient.SendCommand(buffer);
        }


        public void SetCalibrateChannelValue(float value ,string setFormat)
        {
            this.NotifyMessage("发送标定值命令");
            string content = string.Format(setFormat,this.ChannelIndex, value);
            CalibrateCommand command = new SetCalibrateValueCommand(this, content);
            byte[] buffer = command.Package();
            this.UDPClient.SendCommand(buffer);
        }

        public void GetCalibrateChannelValue()
        {
            this.NotifyMessage("发送获取数据命令");
            CalibrateCommand command = new ReadCalibrateValueCommand(this);
            byte[] buffer = command.Package();
            this.UDPClient.SendCommand(buffer);
        }

        public void SaveCalibrateChannelValue()
        {
            this.NotifyMessage("发送保存标定值命令");
            string content = string.Format(this.CalibrateConfig.SaveFormat, this.ChannelIndex);
            CalibrateCommand command = new SaveCalibrateValueCommand(this, content);
            byte[] buffer = command.Package();
            this.UDPClient.SendCommand(buffer);

        }

        public bool CalibrateCompleted
        {
            get {
                return this.ActiveChannelIndex >= this.ActiveCardModule.ChannelCount;
            }
        }

        public  int  ChannelIndex
        {
            get { return this.ActiveChannelIndex + this.CalibrateConfig.BaseIndex; }            
        }

        public void NotifyMessage(string message)
        {

            this.Notify((int)NotifyEvents.Message, this.ActiveCardModule.Description, "", this.ActiveCardModule, message);
        }

    }




    public class AnalogInputModuleCalibrator : BaseCalibrator
    {

        public override void ProcessEvent()
        {
            if (!this.CalibrateCompleted)
            {
                foreach (CalibrateValueItem item in this.CalibrateConfig.Items)
                {
                    string message = "";
                    //输入模拟量设备，Fluke 7526 表置值  
                    this.SetChannelValue((int)ActiveCardModule.CardType, this.ActiveChannelIndex.ToString(), (int)this.CalibrateConfig.ChannelDataType, item.AnalogDeviceValue, ref message);
                    this.NotifyMessage(message);
                    this.Delay(1000);
                    this.ConnectModule();
                    this.SetCalibrateChannelValue(item.CalibrateValue,item.SetFormat);
                    this.ConnectModule();
                    this.GetCalibrateChannelValue();
                    this.SaveCalibrateChannelValue();
                    Delay(1000);
                }
            }
        }

    }


    public class AnalogOutputModuleCalibrator : BaseCalibrator
    {

        public  string GetChannelIndexLabel
        {
            get { return this.ActiveCardModule.PointNames[this.ActiveChannelIndex]; }
           
        }


        public override void ProcessEvent()
        {
            if (!this.CalibrateCompleted)
            {
                foreach (CalibrateValueItem item in this.CalibrateConfig.Items)
                {
                    string message = "";
                    //输出模拟量设备，NUCON 置值
                    this.SetChannelValue((int)ActiveCardModule.CardType, this.GetChannelIndexLabel, (int)this.CalibrateConfig.ChannelDataType, item.CalibrateValue, ref message);
                    this.NotifyMessage(message);
                    this.Delay(1000);
                    float value = 0.0f;
                    //获取表 8846的采样值
                    this.GetChannelValue((int)ActiveCardModule.CardType, this.ActiveChannelIndex.ToString(), (int)this.CalibrateConfig.ChannelDataType, ref value, ref message);
                    this.ConnectModule();

                    value = value /1; //FIXME :计算公式

                    this.SetCalibrateChannelValue( value,item.SetFormat);
                    this.ConnectModule();
                    this.GetCalibrateChannelValue();
                    this.SaveCalibrateChannelValue();
                    Delay(1000);
                }
            }
            
        }


    }

}
