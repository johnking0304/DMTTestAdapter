using System;
using System.Runtime.InteropServices;
using DMT.Core.Models;
using DMT.Core.Utils;
using TAI.Constants;
using TAI.Device;
using TAI.Manager;
using TAI.Modules;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DMTTestAdapter
{



    /// <summary>
    /// 测试接口
    /// </summary>
    [Guid("7BAFA04D-0619-4D1C-B21B-5BF7AE1B0AF4")]
    [ClassInterface(ClassInterfaceType.None)]
    public class TestAdapter : OperatorController, ITestAdapter
    {

        public static readonly int FeedCountMax  = 6;
             
        private DeviceModel MeasureDeviceModel { get; set; }
        private DeviceModel GeneratorDeviceModel { get; set; }

        public DigitalDevice DigitalDevice { get; set; }
        public IAnalogDevice MeasureDevice { get; set; }
        public IAnalogDevice GeneratorDevice { get; set; }
        public ProcessController ProcessController { get; set; }
        public SwitchController SwitchController { get; set; }
        public VISController VISController { get; set; }

        public TestState TestState { get; set; }

        public SystemMessage SystemMessage { get; set; }

        public OperateCommand Command { get; set; }

        //测试模块
        public List<Module> TestingModules { get; set; }
        /// <summary>
        /// 初始化完成
        /// </summary>
        public bool  InitializeCompleted {get;set;}



        public TestAdapter()
        {
            this.Caption = "TestAdapter";
            this.LoadConfig();
            this.Command = OperateCommand.None;
            this.TestingModules = new List<Module>();
        }


        private void  LoadConfig()
        {
            this.SystemMessage = new SystemMessage();

            this.InitializeCompleted = false;
            this.TestState = new InitializeTestState(this);

            this.LoadFromFile(Contant.CONFIG);
            this.DigitalDevice = new DigitalDevice();
            this.DigitalDevice.LoadFromFile(Contant.DIGITAL_CONFIG);
            this.DigitalDevice.Open();
            this.DigitalDevice.Start();
            this.SystemMessage.Results.Add(this.DigitalDevice.StatusMessage);

            this.MeasureDevice = AnalogDeviceFactory.CreateDevice(this.MeasureDeviceModel);
            this.MeasureDevice.LoadFromFile(Contant.ANALOG_CONFIG);
            this.MeasureDevice.Open();
            this.SystemMessage.Results.Add(this.MeasureDevice.GetStatusMessage());

            this.GeneratorDevice = AnalogDeviceFactory.CreateDevice(this.GeneratorDeviceModel);
            this.GeneratorDevice.LoadFromFile(Contant.ANALOG_CONFIG);
            this.GeneratorDevice.Open();
            this.SystemMessage.Results.Add(this.GeneratorDevice.GetStatusMessage());

            this.ProcessController = new ProcessController();
            this.ProcessController.LoadFromFile(Contant.PROCESS_CONFIG);
            this.ProcessController.Open();
            this.ProcessController.Start();
            this.SystemMessage.Results.Add(this.ProcessController.StatusMessage);

            this.VISController = new VISController();
            this.VISController.LoadFromFile(Contant.VIS_CONFIG);
            this.VISController.Open();
            this.VISController.Start();
            this.SystemMessage.Results.Add(this.VISController.StatusMessage);

            this.SwitchController = new SwitchController();
            this.SwitchController.LoadFromFile(Contant.SWITCH_CONFIG);
            this.SwitchController.Open();
            this.SwitchController.Start();
            this.SystemMessage.Results.Add(this.SwitchController.StatusMessage);

            this.StartThread();

        }

        

        public override void LoadFromFile(string fileName)
        {
            string value = IniFiles.GetStringValue(fileName, this.Caption, "MeasureDeviceModel", "Fluke7526");
            this.MeasureDeviceModel = (DeviceModel)Enum.Parse(typeof(DeviceModel), value);
            value =  IniFiles.GetStringValue(fileName, this.Caption, "GeneratorDeviceModel", "Fluke8846");
            this.GeneratorDeviceModel = (DeviceModel)Enum.Parse(typeof(DeviceModel), value);

            base.LoadFromFile(fileName);
        }

        public override void SaveToFile(string fileName)
        {
            IniFiles.WriteStringValue(fileName, this.Caption, "MeasureDeviceModel", this.MeasureDeviceModel.ToString());
            IniFiles.WriteStringValue(fileName, this.Caption, "GeneratorDeviceModel", this.GeneratorDeviceModel.ToString());
        }
        public override void ProcessEvent()
        {
            if (this.TestState != null)
            {
                this.TestState.Execute();
            }
        }




        #region Interface

        public bool Initialize()
        {

            bool result = this.DigitalDevice.Initialize();

            result &= this.MeasureDevice.Initialize();

            result &= this.GeneratorDevice.Initialize();

            result &= this.ProcessController.Initialize();

            result &= this.VISController.Initialize();

            result &= this.SwitchController.Initialize();

            this.InitializeCompleted = true;
            this.TestingModules.Clear();

            return result;
        }



        public double GetAnalogueChannelValue(int channelId, int type)
        {
            this.SwitchController.SwitchChannelOperate((ushort)channelId);
            double value = 0.0;
            this.MeasureDevice.GetValue( (ChannelType)type, ref value);        
            return value;        
        }

        public bool GetDigitalChannelValue(int channelId)
        {
            return this.DigitalDevice.GetValue(channelId);
        }

        public int GetLastErrorCode()
        {
            return 0;
        }

        public string GetSystemStatus()
        {           
            return JsonConvert.SerializeObject(this.StatusMessage);
        }

        public string GetVISResult()
        {
            return "";
        }



        public bool Initialize(string content)
        {
            ///根据约定的content初始化设备信息
            return true;
        }

        public bool SetAnalogueChannelValue(int channelId, int type, double value)
        {

            bool result = this.SwitchController.SwitchChannelOperate((ushort)channelId);
            result &= this.GeneratorDevice.SetValue((ChannelType)type, value);
            return result;
        }

        public bool SetDigitalChannelValue(int channelId, bool value)
        {
            return this.DigitalDevice.SetValue(channelId,value);
        }


        public void StartTest()
        {

            this.Command = OperateCommand.StartTest;           
        }

        public void StopTest()
        {
            this.Command = OperateCommand.StopTest;
        }
        #endregion


    }
}
