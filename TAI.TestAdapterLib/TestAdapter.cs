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

        public List<Station> Stations { get; set; }


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
            this.Stations = new List<Station>();
            foreach (StationType type in Enum.GetValues(typeof(StationType)))
            {
                Station station = new Station(type);
                this.Stations.Add(station);
            }
        }

        public void RemoveModule(Module module)
        {
            this.TestingModules.Remove(module);           
        }




        public Station PrepareStation {
            get => this.Stations[(int)StationType.Prepare - 1];
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

        public string Initialize()
        {

            bool result = this.DigitalDevice.Initialize();

            result &= this.MeasureDevice.Initialize();

            result &= this.GeneratorDevice.Initialize();

            result &= this.ProcessController.Initialize();

            result &= this.VISController.Initialize();

            result &= this.SwitchController.Initialize();

            this.InitializeCompleted = true;
            this.TestingModules.Clear();

            return this.StatusMessageText;
        }



        public float GetAnalogueChannelValue(int channelId, int type)
        {
            this.SwitchController.SwitchChannelOperate((ushort)channelId);
            float value = 0;
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
            return this.StatusMessageText;
        }




        public string Initialize(string content)
        {
            ///根据约定的content初始化设备信息
            return "";
        }

        public bool SetAnalogueChannelValue(int channelId, int type, float value)
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

        public bool StartStationTest(int StationId)
        {
            return true;
        }


        public string GetVISModuleType(int StationId)
        {
            return "";
        }

        public string GetVISModuleCode(int StationId)
        {
            return "";
        }

        public string GetVISLightingResult(int StationId)
        {
            return "";
        }

        public bool SetTestResult(int StationId, bool result)
        {
            this.Command = OperateCommand.StopStationTest;
            if ((StationId > 0) && (StationId < FeedCountMax))
            {
                Station station = this.Stations[StationId - 1];
                station.LinkedModule.Conclusion = result;
                return true;
            }
            return false;
            
        }
        #endregion



        #region Module 



        public void StartModulePrepare(Module module)
        {
            module.TestStep = TestStep.Prepare;
            this.PrepareStation.LinkedModule = module;
            module.CurrentPosition = Position.Prepare;
            module.StartDateTime = DateTime.Now;
            module.TargetPosition = this.Stations[(int)module.ModuleType - 1].TestPosition;
        }


        public void StartModuleTest(Module module)
        {
            module.TestStep = TestStep.Testing;
            module.LinkStation.LinkedModule = module;
            module.CurrentPosition = module.TargetPosition;
            module.StartDateTime = DateTime.Now;
            module.TargetPosition = Position.Out_OK;
        }


        public void StartModuleBlank(Module module)
        {
            module.TestStep = TestStep.Finish;
            module.LinkStation.Clear();
            this.RemoveModule(module);
        }

        public ModuleType ParseModuleType(string content)
        {
            //FIXME





            return ModuleType.AI;
        }

        public Station GetModuleStation(ModuleType moduleType)
        {
            if (moduleType != ModuleType.None)
            {
                return this.Stations[(int)moduleType];
            }
            else
            {
                return null;
            }

        }



        public bool ModulePrepareCompleted()
        {
            if (this.PrepareStation.LinkedModule != null)
            {
                return this.PrepareStation.LinkedModule.PrepareReady;
            }
            return false;

        }


        public Module DigitalIdleModule
        {
            get
            {

                foreach (Module module in this.TestingModules)
                {
                    if (module.ModuleType >= ModuleType.DI && module.ModuleType <= ModuleType.PI)
                    {
                        if (module.TestStep == TestStep.Idle)
                        {
                            return module;
                        }
                    }
                }
                return null;
            }
        }


        public Module AnaloglIdleModule
        {
            get
            {

                foreach (Module module in this.TestingModules)
                {
                    if (module.ModuleType >= ModuleType.AI && module.ModuleType <= ModuleType.TC)
                    {
                        if (module.TestStep == TestStep.Idle)
                        {
                            return module;
                        }
                    }
                }
                return null;
            }
        }






        public bool ModuleIdle
        {
            get
            {
                foreach (Module module in this.TestingModules)
                {
                    if (module.TestStep == TestStep.Idle)
                    {
                        return true;
                    }
                }
                return false;
            }

        }

        public bool ModuleNeedFeed
        {
            get
            {

                foreach (Module module in this.TestingModules)
                {
                    if (module.ModuleType >= ModuleType.DI && module.ModuleType <= ModuleType.PI)
                    {
                        if (module.TestStep == TestStep.Idle)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public Module ModuleNeedPrepare
        {
            get
            {
                foreach (Module module in this.TestingModules)
                {
                    if (module.ModuleType >= ModuleType.AI && module.ModuleType <= ModuleType.TC)
                    {
                        if (module.TestStep == TestStep.Idle)
                        {
                            return module;
                        }
                    }
                }
                return null;
            }
        }


        #endregion


    }
}
