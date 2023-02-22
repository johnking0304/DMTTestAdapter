using AnalogDevice;
using System;
using DMTTestAdapter;
using DMT.Core.Models;
using DMT.Core.Utils;
using TAI.Constants;
using TAI.Device;
using TAI.Manager;



namespace DMTTestAdapter
{

    public enum SystemStatus
    {
        Idle = 0,
        Initialize =1,
        Testing =2,
        Error=3,
    }


    /// <summary>
    /// 测试接口
    /// </summary>
    public class TestAdapter : Controller, ITestAdapter
    {

        private DeviceModel MeasureDeviceModel { get; set; }

        private DeviceModel GeneratorDeviceModel { get; set; }

        public DigitalDevice DigitalDevice { get; set;}
        public IAnalogDevice MeasureDevice { get; set; }
        public IAnalogDevice GeneratorDevice { get; set; }
        public ProcessController ProcessController { get; set; }
        public SwitchController SwitchController { get; set; }
        public VISController VISController { get; set; }

        public event NotifyHandler NotifyCallback;

        public SystemStatus SystemStatus { get; set; }

        public int LastErrorCode { get
            {
                return 0;
            
            } }


        public TestAdapter()
        {
            this.Caption = "TestAdapter";
        }


        private void  LoadConfig()
        {
            this.LoadFromFile(Contant.CONFIG);
            this.DigitalDevice = new DigitalDevice();
            this.DigitalDevice.LoadFromFile(Contant.CONFIG);

            this.MeasureDevice = AnalogDeviceFactory.CreateDevice(this.MeasureDeviceModel);
            this.MeasureDevice.LoadFromFile(Contant.CONFIG);

            this.GeneratorDevice = AnalogDeviceFactory.CreateDevice(this.GeneratorDeviceModel);
            this.GeneratorDevice.LoadFromFile(Contant.CONFIG);

            this.ProcessController = new ProcessController();
            this.ProcessController.LoadFromFile(Contant.CONFIG);

            this.VISController = new VISController();
            this.VISController.LoadFromFile(Contant.CONFIG);

            this.SwitchController = new SwitchController();
            this.SwitchController.LoadFromFile(Contant.CONFIG);

        }

        public bool Initialize()
        {
            this.LoadConfig();

            this.DigitalDevice.Initialize();

            this.MeasureDevice.Initialize();

            this.GeneratorDevice.Initialize();

            this.ProcessController.Initialize();

            this.VISController.Initialize();

            this.SwitchController.Initialize();


            return true;
        }

        public override void LoadFromFile(string fileName)
        {
            string value = IniFiles.GetStringValue(fileName, this.Caption, "MeasureDeviceModel", "BeamexMC6");
            this.MeasureDeviceModel = (DeviceModel)Enum.Parse(typeof(DeviceModel), value);
            value =  IniFiles.GetStringValue(fileName, this.Caption, "GeneratorDeviceModel", "Fluke8846");
            this.GeneratorDeviceModel = (DeviceModel)Enum.Parse(typeof(DeviceModel), value);

            base.LoadFromFile(fileName);
        }

        public override void SaveToFile(string fileName)
        {
            IniFiles.WriteStringValue(fileName, this.Caption, "MeasureDeviceModel", this.MeasureDeviceModel.ToString());
            IniFiles.GetStringValue(fileName, this.Caption, "GeneratorDeviceModel", this.GeneratorDeviceModel.ToString());
        }


        public double GetAnalogueChannelValue(int channelId, ChannelType type)
        {
            this.SwitchController.SwitchChannel(channelId);
            double value = 0.0;
            this.MeasureDevice.GetValue(channelId, type, ref value);        
            return value;        
        }

        public bool GetDigitalChannelValue(int channelId)
        {
            return this.DigitalDevice.GetValue(channelId);
        }

        public int GetLastErrorCode()
        {
            return this.LastErrorCode;
        }

        public string GetSystemStatus()
        {
            return this.SystemStatus.ToString();
        }

        public string GetVISResult()
        {
            return this.VISController.GetResult();
        }



        public bool Initialize(string content)
        {
            ///根据约定的content初始化设备信息
            return true;
        }

        public bool SetAnalogueChannelValue(int channelId, ChannelType type, double value)
        {

            bool result = this.SwitchController.SwitchChannel(channelId);
            result &= this.GeneratorDevice.SetValue(channelId, type, value);
            return result;
        }

        public bool SetDigitalChannelValue(int channelId, bool value)
        {
            return this.DigitalDevice.SetValue(channelId,value);
        }

        public void SetNotifyCallback(NotifyHandler notify)
        {
            this.NotifyCallback += notify;
        }

        public void StartTest()
        {
            this.SystemStatus = SystemStatus.Testing;           
        }

        public void StopTest()
        {
            this.SystemStatus = SystemStatus.Idle;
        }
    }
}
