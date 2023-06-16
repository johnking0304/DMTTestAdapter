using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;
using TAI.Test.Scheme;
using DMT.Core.Utils;
using TAI.Device;
using DMT.Core.Channels;
using System.IO;

namespace TAI.TestDispatcher
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
        public int WaitSteadyMilliSeconds { get; set; }

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
            this.WaitSteadyMilliSeconds = IniFiles.GetIntValue(fileName, Caption, "WaitSteadyMilliSeconds", 5000);
            this.ChannelDataType =  (ChannelType)IniFiles.GetIntValue(fileName, "TestSchemeConfig", "ChannelDataType", (int)ChannelType.None);

            foreach (CalibrateValueItem item in this.Items)
            {
                string ItemSection = string.Format("{0}{1}", Caption, item.Caption);
                item.AnalogDeviceValue = (float)IniFiles.GetFloatValue(fileName, ItemSection, "AnalogDeviceValue",0.0f);
                item.CalibrateValue = (float)IniFiles.GetFloatValue(fileName, ItemSection, "CalibrateValue", 0.0f);
                item.SetFormat = IniFiles.GetStringValue(fileName, ItemSection, "SetFormat", "io/cr -T-s{0}-a-min {1}");
            }
            this.SaveToFile(fileName);

        }

        public void SaveToFile(string fileName)
        {
             IniFiles.WriteIntValue(fileName, Caption, "BaseIndex", this.BaseIndex);
             IniFiles.WriteStringValue(fileName, Caption, "SaveFormat", this.SaveFormat);
             IniFiles.WriteIntValue(fileName, Caption, "WaitSteadyMilliSeconds", this.WaitSteadyMilliSeconds);

            foreach (CalibrateValueItem item in this.Items)
            {
                string ItemSection = string.Format("{0}{1}", Caption, item.Caption);
                IniFiles.WriteFloatValue(fileName, ItemSection, "AnalogDeviceValue", item.AnalogDeviceValue);
                IniFiles.WriteFloatValue(fileName, ItemSection, "CalibrateValue", item.CalibrateValue);
                IniFiles.WriteStringValue(fileName, ItemSection, "SetFormat", item.SetFormat);
            }

        }
    }



    public class BaseCalibrator:BaseController
    {

        public CalibrateController Controller { get; set; }

        public CardModule ActiveCardModule { get; set; }

        public CalibrateConfig CalibrateConfig { get; set; }

        public SetChannelValueDelegate SetChannelValue { get; set; }
        public GetChannelValueDelegate GetChannelValue { get; set; }

        public ushort SequenceNumber { get; set; }


        public int ActiveChannelIndex { get; set; }


        public BaseCalibrator(CalibrateController controller)
        {
            this.Controller = controller;
            this.CalibrateConfig = new CalibrateConfig();
            this.SequenceNumber = 0;
            
        }

        public void Dispose()
        { 
            
        
        }


        public int ProgressValue
        {
            get
            {
                if (this.ActiveCardModule.ChannelCount > 0)
                {
                    double percent = ((this.ActiveChannelIndex + 1.0) / this.ActiveCardModule.ChannelCount) * 100.0f;
                    int value = (int)Math.Round(percent);
                    if (value > 100)
                    {
                        value = 100;
                    }
                    return value;
                }
                return 100;
            }
        }


        public string ProgressValueContent
        {
            get
            {
                int channelIndex = this.ActiveChannelIndex + 1;
                if (channelIndex > this.ActiveCardModule.ChannelCount)
                {
                    channelIndex = this.ActiveCardModule.ChannelCount;
                }
                return string.Format("{0}/{1}", channelIndex, this.ActiveCardModule.ChannelCount);
            }
        }



        public override void LoadFromFile(string fileName)
        {
            string file = string.Format("{0}.ini", this.ActiveCardModule.CardType.ToString());       
            this.CalibrateConfig.LoadFormFile(Path.Combine(Constants.Contant.CARD_MODULES, file));

            base.LoadFromFile(file);
        }


        public void ConnectModule()
        {
            this.NotifyMessage("发送CONNECT 连接命令");
            CalibrateCommand command = new ConnectCommand(this);
            byte[] buffer =command.Package();
            this.Controller.UDPClient.SendCommand(buffer);
        }


        public void SetCalibrateChannelValue(float value ,string setFormat)
        {           
            string content = string.Format(setFormat,this.ChannelIndex, value);
            CalibrateCommand command = new SetCalibrateValueCommand(this, content);
            byte[] buffer = command.Package();
            this.Controller.UDPClient.SendCommand(buffer);
            this.NotifyMessage(string.Format("通道[{0}]发送标定值[{1}]命令[{2}]", this.ActiveChannelIndex+1, value, content));
        }

        public void GetCalibrateChannelValue()
        {
            this.NotifyMessage("发送获取数据命令");
            CalibrateCommand command = new ReadCalibrateValueCommand(this);
            byte[] buffer = command.Package();
            this.Controller.UDPClient.SendCommand(buffer);
        }

        public void SaveCalibrateChannelValue()
        {
            
            string content = string.Format(this.CalibrateConfig.SaveFormat, this.ChannelIndex);
            CalibrateCommand command = new SaveCalibrateValueCommand(this, content);
            byte[] buffer = command.Package();
            this.Controller.UDPClient.SendCommand(buffer);
            this.NotifyMessage(string.Format("通道[{0}]发送保存标定值命令[{1}]", this.ActiveChannelIndex+1, content));

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

        public void NotifyMessage(string message, MessageType messageType = MessageType.Normal)
        { 
            this.Notify((int)NotifyEvents.Message, this.ActiveCardModule.Description, messageType.ToString(), this, message);
        }

    }




    public class AnalogInputModuleCalibrator : BaseCalibrator
    {
        public AnalogInputModuleCalibrator(CalibrateController controller) : base(controller)
        { 
        
        }
        public override void ProcessEvent()
        {
            if (!this.CalibrateCompleted)
            {
                this.NotifyMessage(string.Format("----通道{0}------------------------------", this.ActiveChannelIndex + 1));
                foreach (CalibrateValueItem item in this.CalibrateConfig.Items)
                {
                    string message = "";
                    //输入模拟量设备，Fluke 7526 表置值  
                    this.SetChannelValue((int)ActiveCardModule.CardType, (this.ActiveChannelIndex+1).ToString(), (int)this.CalibrateConfig.ChannelDataType, item.AnalogDeviceValue, ref message);
                    this.NotifyMessage(message);
                    this.Delay(this.CalibrateConfig.WaitSteadyMilliSeconds);
                    this.ConnectModule();
                    this.Delay(1000);
                    this.SetCalibrateChannelValue(item.CalibrateValue, item.SetFormat);
                    this.Delay(1000);
                    this.ConnectModule();
                    this.Delay(1000);
                    this.GetCalibrateChannelValue();
                    Delay(1000);
                }
                this.ConnectModule();
                Delay(500);
                this.SaveCalibrateChannelValue();
                this.Delay(2000);

                this.ActiveChannelIndex += 1;
            }
            else
            {               
                this.NotifyMessage("校准完成");
                this.StopThread();
            }

            
        }

    }


    public class AnalogOutputModuleCalibrator : BaseCalibrator
    {
        public AnalogOutputModuleCalibrator(CalibrateController controller) : base(controller)
        {

        }

        public  string GetChannelIndexLabel
        {
            get { return this.ActiveCardModule.PointNames[this.ActiveChannelIndex]; }
           
        }


        public override void ProcessEvent()
        {
            if (!this.CalibrateCompleted)
            {
                this.NotifyMessage(string.Format("----通道{0}------------------------------", this.ActiveChannelIndex + 1));

                foreach (CalibrateValueItem item in this.CalibrateConfig.Items)
                {
                    string message = "";
                    //输出模拟量设备，NUCON 置值
                    this.SetChannelValue((int)ActiveCardModule.CardType, this.GetChannelIndexLabel, (int)this.CalibrateConfig.ChannelDataType, item.CalibrateValue, ref message);
                    this.NotifyMessage(message);

                    this.Delay(this.CalibrateConfig.WaitSteadyMilliSeconds);
                    float value = 0.0f;
                    //获取表 8846的采样值
                    this.GetChannelValue((int)ActiveCardModule.CardType, (this.ActiveChannelIndex+1).ToString(), (int)this.CalibrateConfig.ChannelDataType, ref value, ref message);
                    this.NotifyMessage(message);
                    
                    this.ConnectModule();
                    this.Delay(1000);
                    this.SetCalibrateChannelValue( value,item.SetFormat);
                    this.Delay(1000);
                    this.ConnectModule();
                    this.Delay(1000);
                    this.GetCalibrateChannelValue();
                    
                    Delay(1000);
                }
                this.ConnectModule();
                Delay(500);
                this.SaveCalibrateChannelValue();
                Delay(3000);
                this.ActiveChannelIndex += 1;
            }
            else
            {
                this.NotifyMessage("校准完成");
                this.StopThread();
            }
        }
       


    }



    public class CalibrateController : BaseController
    {
        public UDPClientChannel UDPClient { get; set; }
        public int Port { get; set; }
        public UDPService UDPService { get; set; }
        public BaseCalibrator Calibrator { get; set; }

        public CalibrateController()
        {
            this.UDPClient = new UDPClientChannel();
            this.UDPService = new UDPService();


        }
        public override void LoadFromFile(string fileName)
        {
            this.UDPClient.LoadFromFile(fileName);
            this.Port = this.UDPClient.Port;
            this.UDPService.LoadFromFile(fileName);
        }

        public void Open()
        {
            this.UDPService.Open();
            this.UDPService.StartAsyncReceiveData();
        }

        public void StartCalibrateModule(CardModule card)
        {
            this.UDPClient.Close();
            this.UDPClient.Open(card.IPAddressText,this.Port);
            this.Calibrator = CalibratorFactory.CreateCalibrator(this, card.CardType);
            if (this.Calibrator != null)
            {               
                this.Calibrator.ActiveCardModule = card;
                this.Calibrator.AttachObserver(this.subjectObserver.Update);
                this.Calibrator.LoadFromFile(Constants.Contant.CONFIG);
                this.Calibrator.StartThread();              
            }
        }

        public void StopCalibrateModule()
        {
            this.UDPClient.Close();
            if (this.Calibrator != null)
            {
                this.Calibrator.DetachObserver(this.subjectObserver.Update);
                this.Calibrator.StopThread();
                this.Calibrator.Dispose();
            }
        }

        public override void ProcessResponse(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            base.ProcessResponse(notifyEvent, flag, content, result, message, sender);
            switch (notifyEvent)
            {
                case UDPService.CHANNEL_EVENT:
                    {
                        ChannelControl control = (ChannelControl)Enum.Parse(typeof(ChannelControl), flag);
                        if (control == ChannelControl.Report)
                        {
                            byte[] value = System.Text.Encoding.Default.GetBytes(content); 

                        }
                        break;
                    }
                
            }
        }


    }

}
