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
using DMT.Core.Channels;

namespace DMTTestAdapter
{

    public class ChannelIdLink
    {
        static Dictionary<int, int> IdLinks = new Dictionary<int, int>() {
        {1 , 1 },
        {3 , 2 },
        {5 , 3 },
        {7 , 4 },
        {9 , 5 },
        {11, 6 },
        {13, 7 },
        {15, 8 },
        {2 , 9 },
        {4 , 10 },
        {6 , 11 },
        {8 , 12 },
        {10, 13 },
        {12, 14 },
        {14, 15 },
        {16, 16 }
    };

        public static int GetOutput(int input)
        {
            if (IdLinks.ContainsKey(input))
            {
                return IdLinks[input];
            }
            else
            {
                return input;
            }
        }
    }


    /// <summary>
    /// 测试接口
    /// </summary>
    [Guid("2BD2243F-816B-4FB9-ABB6-E9E24AD433E2")]
    [ClassInterface(ClassInterfaceType.None)]
    public class TestAdapter : OperatorController, ITestAdapter
    {

        public static readonly int FeedCountMax = 6;
        
        private DeviceModel MeasureDeviceModel { get; set; }
        private DeviceModel GeneratorDeviceModel { get; set; }

        public DigitalDevice DigitalDevice { get; set; }

        public EnvironmentDevice EnvironmentDevice { get; set; }
        public DeviceMaster MeasureDevice { get; set; }
        public DeviceMaster GeneratorDevice { get; set; }
        public ProcessController ProcessController { get; set; }
        public List<SwitchController> SwitchControllers { get; set; }

        

        public VISController VISController { get; set; }

        public TestState testState;
        public TestState TestState {
            get
            {
                return testState;
            }
            set
            {
                this.testState = value;
                this.SystemMessage.Status = value.TestingState.ToString();
            } 
        }

        public SystemMessage SystemMessage { get; set; }

        public OperateCommand Command { get; set; }      
        public int ActiveStation { get; set; }
        public OperateCommand RequestCommand { get; set; }

        public OperateCommand ReleaseCommand { get; set; }


        //测试模块
        public List<Module> TestingModules { get; set; }

        public List<Station> Stations { get; set; }

        public TCPService Service { get; set; }

        public DeviceType DeviceType { get; set; }

        public  Dictionary<string, int> PrepareMinutes { get; set; }

        /// <summary>
        /// 初始化完成
        /// </summary>
        public bool InitializeCompleted { get; set; }

        public bool IsFeedingBoxEmpty { get
            {
                bool result = true;
                foreach (Module module in this.TestingModules)
                {
                    result = result && module.TestStep != TestStep.Idle;               
                }
                return result;            
            } }

        public TestAdapter()
        {
            this.Caption = "TestAdapter";
            
            this.Command = OperateCommand.None;
            this.ReleaseCommand = OperateCommand.None;
            
            this.TestingModules = new List<Module>();
            this.Stations = new List<Station>();
            this.SystemMessage = new SystemMessage(this.Caption);
            this.ActiveStation = 0;

            this.PrepareMinutes = new Dictionary<string,int>();

            this.WaitMilliseconds = 5000;

            this.Service = new TCPService();

            this.Service.AttachObserver(this.subjectObserver.Update);


            foreach (StationType type in Enum.GetValues(typeof(StationType)))
            {
                Station station = new Station(type);
                this.Stations.Add(station);
                this.SystemMessage.Stations.Add(station.StationStatus);
                station.LoadCompensates(Contant.STATIONS);
            }
            this.LoadConfig();

            this.TestState = new InitializeTestState(this);

        }

        public void RemoveModule(Module module)
        {
            this.TestingModules.Remove(module);
        }

        public string SystemMessageText { 
        get => JsonConvert.SerializeObject(this.SystemMessage);
        }


        public Station PrepareStation {
            get => this.Stations[(int)StationType.Prepare - 1];
         }

        private void  LoadConfig()
        {
            this.InitializeCompleted = false;

            this.Service.LoadFromFile(Contant.CONFIG);
            this.Service.Open();

            this.LoadModuleConfig(Contant.MODULE_CONFIG);
                      
            this.LoadFromFile(Contant.CONFIG);
            this.DigitalDevice = new DigitalDevice();
            this.DigitalDevice.LoadFromFile(Contant.DIGITAL_CONFIG);
            this.DigitalDevice.Open();
            this.DigitalDevice.Start();
            this.SystemMessage.Devices.Add(this.DigitalDevice.StatusMessage);


            this.EnvironmentDevice = new EnvironmentDevice();
            this.EnvironmentDevice.LoadFromFile(Contant.ENVIRONMENT_CONFIG);
            this.EnvironmentDevice.Open();
            this.EnvironmentDevice.Start();
            //this.SystemMessage.Devices.Add(this.EnvironmentDevice.StatusMessage);

            this.MeasureDevice = AnalogDeviceFactory.CreateDevice(this.MeasureDeviceModel);
            this.MeasureDevice.LoadFromFile(Contant.ANALOG_CONFIG);
            this.MeasureDevice.Open();
            StatusMessage message = this.MeasureDevice.GetStatusMessage();
            message.Name = "MeasureDevice";
            this.SystemMessage.Devices.Add(this.MeasureDevice.GetStatusMessage());

            this.GeneratorDevice = AnalogDeviceFactory.CreateDevice(this.GeneratorDeviceModel);
            this.GeneratorDevice.LoadFromFile(Contant.ANALOG_CONFIG);
            this.GeneratorDevice.Open();
            message = this.GeneratorDevice.GetStatusMessage();
            message.Name = "GeneratorDevice";           
            this.SystemMessage.Devices.Add(this.GeneratorDevice.GetStatusMessage());

            this.ProcessController = new ProcessController();
            this.ProcessController.LoadFromFile(Contant.PROCESS_CONFIG);
            this.ProcessController.Open();
            this.ProcessController.Start();
            this.SystemMessage.Devices.Add(this.ProcessController.StatusMessage);

            this.VISController = new VISController();
            this.VISController.LoadFromFile(Contant.VIS_CONFIG);
            this.VISController.Open();
            this.VISController.Start();
            this.SystemMessage.Devices.Add(this.VISController.StatusMessage);


            this.SwitchControllers = new List<SwitchController>();

            for (int i = (int)StationType.AI; i <= (int)StationType.TC; i++)
            {
                SwitchController switchController = new SwitchController((StationType)i);
                switchController.LoadFromFile(Contant.SWITCH_CONFIG);
                switchController.Open();
                switchController.Start();
                this.SwitchControllers.Add(switchController);
                this.SystemMessage.Devices.Add(switchController.StatusMessage);
            }

            foreach (SwitchController switchController in this.SwitchControllers)
            {
                 switchController.Initialize();
            }


            this.StartThread();
            
        }


        public void Dispose()
        {
            this.StopThread();

            this.Service.Close();

            this.DigitalDevice.Close();

            this.EnvironmentDevice.Close();

            this.MeasureDevice.Close();

            this.GeneratorDevice.Close();

            this.ProcessController.Close();

            this.VISController.Close();

            foreach (SwitchController switchController in this.SwitchControllers)
            {
                switchController.Close();
            }

        }
        public void SaveModuleConfig(string fileName)
        {
            foreach(string key in this.PrepareMinutes.Keys)
            {               
                IniFiles.WriteIntValue(fileName, "PrepareMinutes", key, this.PrepareMinutes[key]);
            }
        }

        public void LoadModuleConfig(string fileName)
        {
            this.PrepareMinutes.Clear();
            string caption = "PrepareMinutes";
            for (int i = (int)ModuleType.AI; i <= (int)ModuleType.TC; i++)
            {
                string key = ((ModuleType)i).ToString();
                int value = IniFiles.GetIntValue(fileName, caption, key , 15);
                this.PrepareMinutes.Add(key, value);
            }

            string[] list = IniFiles.GetAllSectionNames(fileName);

            if (!((System.Collections.IList)list).Contains(caption))
            {
                this.SaveModuleConfig(fileName);
            }
        }

        public override void LoadFromFile(string fileName)
        {
            string value = IniFiles.GetStringValue(fileName, this.Caption, "MeasureDeviceModel", "Fluke8846");
            this.MeasureDeviceModel = (DeviceModel)Enum.Parse(typeof(DeviceModel), value);
            value =  IniFiles.GetStringValue(fileName, this.Caption, "GeneratorDeviceModel", "Fluke7526");
            this.GeneratorDeviceModel = (DeviceModel)Enum.Parse(typeof(DeviceModel), value);         
            base.LoadFromFile(fileName);
            
        }

        public int GetPrepareMinutes(ModuleType moduleType)
        {
            if (this.PrepareMinutes.ContainsKey(moduleType.ToString()))
            {
                return this.PrepareMinutes[moduleType.ToString()];
            }
            return 0;
        }

        public override void SaveToFile(string fileName)
        {
            IniFiles.WriteStringValue(fileName, this.Caption, "MeasureDeviceModel", this.MeasureDeviceModel.ToString());
            IniFiles.WriteStringValue(fileName, this.Caption, "GeneratorDeviceModel", this.GeneratorDeviceModel.ToString());
        }
        public override void ProcessEvent()
        {
            //任意时刻，如果系统复位，就把状态切换到 初始化状态
            if (this.ProcessController.SystemTerminated)
            {
                this.TestState = new InitializeTestState(this,false);
            }

            if (this.TestState != null)
            {
                this.TestState.Execute();
            }

            if (this.WaitTimeOut())
            {
                this.NotifyFeedingBoxMapValue();
                this.UpdateEnvironmentData();
            }
           

        }


        public void UpdateDeviceType()
        {
            this.VISController.ProgramDeviceType = (int)this.ProcessController.TestDeviceType;
            this.DeviceType = this.ProcessController.TestDeviceType; 
        }

        private bool SendCommandReply(string reply)
        {
            reply = string.Format("{0}\n", reply);
            return this.Service.SendCommand(reply);
        }



        private SwitchMode GetSwitchChannelMode(int stationId, int type)
        {
            SwitchMode mode = SwitchMode.Off;
            switch ((ChannelType)type)
            {
                case ChannelType.Current:
                    {
                        switch ((StationType)stationId)
                        {
                            case StationType.AI:
                            case StationType.AO:
                                {
                                    mode = SwitchMode.Current;
                                    break;
                                }
                        }
                        break;
                    }
                case ChannelType.Voltage:
                    {
                        switch ((StationType)stationId)
                        {
                            case StationType.AI:
                            case StationType.AO:
                            case StationType.TC:
                                {
                                    mode = SwitchMode.Voltage;
                                    break;
                                }
                        }
                        break;
                    }
                case ChannelType.Resistance:
                    {
                        switch ((StationType)stationId)
                        {
                            case StationType.RTD_3L:
                            case StationType.RTD_4L:
                                {
                                    mode = SwitchMode.RTD;
                                    break;
                                }
                        }

                        break;
                    }

            }
            return mode;
        }

/*        Off = 0,    //0-关闭全部使能通道
        Voltage =1, //1-A 进 A 出 （TC 和 AI、AO 电压型）
        RTD =2,     //2-A 进 A 出，B 进 B 出（RTD3，4 线）
        Current=3,  //3-B 进 A 出 （AI、AO 电流型）*/
        public bool SwitchChannelOperate(int stationId, ushort channelId,int type)
        {
            SwitchMode mode = this.GetSwitchChannelMode(stationId, type);
            int index = stationId - (int)StationType.AI;
            bool result = this.SwitchControllers[index].SwitchModeOperate(mode);
            return this.SwitchControllers[index].SwitchChannelOperate(channelId);       
        }

        public bool SwitchChannelOperate(int stationId, ushort channelId, SwitchMode mode)
        {
            int index = stationId - (int)StationType.AI;
            bool result = this.SwitchControllers[index].SwitchModeOperate(mode);
            return this.SwitchControllers[index].SwitchChannelOperate(channelId);
        }

        public bool SwitchModeOperate(int stationId, SwitchMode mode)
        {
            int index = stationId - (int)StationType.AI;
            return this.SwitchControllers[index].SwitchModeOperate(mode);
        }


        #region Interface

        public string Initialize()
        {

            bool result = this.DigitalDevice.Initialize();

            result &= this.MeasureDevice.Initialize();

            result &= this.GeneratorDevice.Initialize();

            result &= this.ProcessController.Initialize();

            result &= this.VISController.Initialize();

            result &= this.EnvironmentDevice.Initialize();

            foreach (SwitchController switchController in this.SwitchControllers)
            {
                result &= switchController.Initialize();
            }
           
            this.TestingModules.Clear();
            //工位状态复位
            foreach (Station station in this.Stations)
            {
                station.Reset();
            }
            //状态复位


            return  this.SystemMessageText;
        }

        public int ConvertChannelId(int stationId, int sourceId)
        {
            switch ((StationType)stationId)
            {
                case StationType.TC:
                case StationType.AI:
                case StationType.AO:
                    {
                        return ChannelIdLink.GetOutput(sourceId);
                    }
                case StationType.RTD_3L:
                case StationType.RTD_4L:
               
                    {
                        return sourceId;
                    }
            }
            return sourceId;
        }

        public string GetAnalogueChannelValue(int stationId,int channelId, int type)
        {
            LogHelper.LogInfoMsg(string.Format("接收命令:获取模拟量通道数据[工位={0},通道={1},类型={2}]", ((StationType)stationId).ToString(), channelId, ((ChannelType)type).ToString()));
            if (stationId >= (int)StationType.AI && stationId <= (int)StationType.TC)
            {
                int linkChannelId = ConvertChannelId(stationId, channelId);
                LogHelper.LogInfoMsg(string.Format("实际切换通道:{0}->{1}", channelId, linkChannelId));

                this.SwitchChannelOperate(stationId, (ushort)linkChannelId, type);
                float value = 0;
                bool result = this.MeasureDevice.GetValue((ChannelType)type, ref value);
                return string.Format("{0},{1}", result ? "Ok" : "Fail", value);
            }
            else
            {
                return string.Format("Fail");
            }            
        }

        public string GetDigitalChannelValue(int stationId, int channelId)
        {

            bool value = false;
            LogHelper.LogInfoMsg(string.Format("接收命令:获取数字量通道数据[通道={0}]", channelId));
            bool result = this.DigitalDevice.GetValue(channelId, ref value);
            return string.Format("{0},{1}", result ? "Ok" : "Fail", value ? "1" : "0");
        }

        public string GetLastErrorCode()
        {
            return "Ok";
        }

        public string GetSystemStatus()
        {
            //LogHelper.LogInfoMsg(string.Format("接收命令:获取系统状态"));
            return  this.SystemMessageText;
        }

        public void NotifyTestingStateChanged(TestingState state)
        {
            string command = string.Format("Notify,{0},{1}", (int)state,state.Description());
            this.Service.SendCommand(command);
        }



        public string SetAnalogueChannelValue(int stationId,int channelId, int type, float value)
        {
            if (stationId >= (int)StationType.PI && stationId <= (int)StationType.TC)
            {
                LogHelper.LogInfoMsg(string.Format("接收命令:设置模拟量通道数据[工位={0},通道={1},类型={2},值={3}]", ((StationType)stationId).ToString(), channelId,((ChannelType)type).ToString(),value));
                bool result = true;
                if (stationId == (int)StationType.PI)
                {
                    result &= this.DigitalDevice.SetPluseOutputMode(PluseMode.PL10KHZ);
                    result &= this.DigitalDevice.SelectPIChannel(channelId);
                }
                else
                {
                    int linkChannelId = ConvertChannelId(stationId,channelId);

                    LogHelper.LogInfoMsg(string.Format("实际切换通道:{0}->{1}", channelId, linkChannelId));

                    result = this.SwitchChannelOperate(stationId, (ushort)linkChannelId, type);

                    Station station = this.Stations[stationId - 1];

                    //if ((ChannelType)type == ChannelType.Resistance)
                    {
                        value = station.CompensateValue(linkChannelId, value);
                    }

                    /// 如果温度设置值 大于800 则按800 设置
                    if ((ChannelType)type == ChannelType.Resistance)
                    {
                        if (value > 800)
                        {
                            LogHelper.LogInfoMsg(string.Format("[{0}]设置值[{1}]大于800，按800设置", type.ToString(),value));
                            value = 800;
                        }
                    }

                    result &= this.GeneratorDevice.SetValue((ChannelType)type, value);
                }
                return result?"Ok":"Fail";
            }
            else
            {
                return string.Format("Fail");
            }
        }

        public string SetDigitalChannelValue(int stationId, int channelId, bool value)
        {
            LogHelper.LogInfoMsg(string.Format("接收命令:设置数量通道数据[通道={0},值={1}]", channelId, value));
            bool result=  this.DigitalDevice.SetValue(channelId,value);
            return result? "Ok" : "Fail";
        }


        public string StartTest()
        {
            LogHelper.LogInfoMsg(string.Format("接收命令:启动系统测试"));
            this.Command = OperateCommand.StartTest;
            return "Ok";
        }

            public string StopTest()
        {
            LogHelper.LogInfoMsg(string.Format("接收命令:停止系统测试"));
            this.Command = OperateCommand.StopTest;
            return "Ok";
        }

        public string StartStationTest(int StationId)
        {
            this.Command = OperateCommand.StartStationTest;
            LogHelper.LogInfoMsg(string.Format("接收命令:启动工位[{0}]测试", StationId));
            return "Ok";
        }


      


        public string GetVISModuleType(int StationId)
        {
            LogHelper.LogInfoMsg(string.Format("接收命令:获取工位[{0}]模块类型视觉识别结果", ((StationType)StationId).ToString()));
            Station station =this.GetModuleStation((ModuleType)StationId);
            if (station.LinkedModule != null)
            {
                string content = station.LinkedModule.ModuleType.ToString();
                return string.Format("Ok,{0}",content);
            }
            return "Fail";
        }

        public string GetVISModuleCode(int StationId)
        {
            LogHelper.LogInfoMsg(string.Format("接收命令:获取工位[{0}]模块二维码视觉识别结果", ((StationType)StationId).ToString()));
            Station station = this.GetModuleStation((ModuleType)StationId);
            if (station.LinkedModule != null)
            {
                return string.Format("Ok,{0}",station.LinkedModule.SerialCode);
            }
            return "Fail";
        }

        public string GetVISLightingResult(int StationId)
        {
            LogHelper.LogInfoMsg(string.Format("获取工位[{0}]模块灯测视觉识别结果[{0}]", ((StationType)StationId).ToString()));
            string value = "";
            int channelCount = 0;
            Station station =this.Stations[StationId - 1];
            if (station.LinkedModule != null)
            {
                channelCount = station.LinkedModule.ChannelCount;
            }

            if (this.VISController.TryOCRChannelLighting((ModuleType)StationId, channelCount, ref value))
            {
                return string.Format("Ok,{0}", value);
            }
            return "Fail";
        }

        public string SetTestResult(int StationId, bool result)
        {
            LogHelper.LogInfoMsg(string.Format("接收命令:通知工位[{0}]测试结果[{1}]", ((StationType)StationId).ToString(), result));
            this.Command = OperateCommand.StopStationTest;
            this.ProcessController.SetModuleTestResult(result);
            if ((StationId >=(int)StationType.DI) && (StationId <= (int)StationType.Prepare))
            {
                Station station = this.Stations[StationId - 1];
                if (station.LinkedModule != null)
                {
                    station.LinkedModule.Conclusion = result;
                    station.WaitToBlanking = true;
                }
                return "Ok";
            }
            return "Fail";
            
        }

    
        public string RequestVISLighting(int StationId)
        {
            LogHelper.LogInfoMsg(string.Format("接收命令:工位[{0}]请求灯测服务", ((StationType)StationId).ToString()));
            if (this.ProcessController.RobotIdle && this.TestState.TestingState == TestingState.PreFeeding)
            {
                this.ActiveStation = StationId;
                this.RequestCommand = OperateCommand.RequestVISLighting;
                return string.Format("Fail,{0}", this.ActiveStation);//SWait
            }
            else if (this.TestState.TestingState == TestingState.Testing && this.TestState.PrepareCompleted)
            {
                //当系统状态在测试状态 并且 准备完成（满足VIS到位，机械手到位条件） ,且请求的StationId 和灯测准备id一致  则 返回ok
                if (StationId == this.ActiveStation)
                {
                    return string.Format("Ok,{0}", this.ActiveStation);
                }
            }            
            return string.Format("Fail,{0}", StationId); ;
        }

        public string ReleaseVISLighting(int StationId)
        {
            LogHelper.LogInfoMsg(string.Format("接收命令:工位[{0}]释放灯测服务", ((StationType)StationId).ToString()));
            if (this.TestState.TestingState == TestingState.Testing)
            {
                //释放灯测 关闭
                //this.ReleaseCommand = OperateCommand.ReleaseVISLighting;
                return string.Format("Ok,{0}", StationId);
            }
            else
            {
                return string.Format("Fail,{0}", this.ActiveStation);
            }
           

        }


        #endregion


        public Station StationWaitToBlanking 
        {
            get {
                foreach (Station station in this.Stations)
                {
                    if (station.WaitToBlanking)
                    {
                        return station;
                    }
                
                }
                return null;
            }
        
        }



        #region Module 
        public void StartModulePrepare(Module module)
        {
            module.TestStep = TestStep.Prepare;
            this.PrepareStation.LinkedModule = module;
            this.PrepareStation.TestStep = TestStep.Prepare;
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


        public int ModuleCount { get {
                return TestingModules.Count;

            } }


        public bool FeedBoxEmpty
        {
            get
            {
                bool empty = true;
                foreach (Module module in this.TestingModules)
                {

                    empty &= module.TestStep > TestStep.Ready;
                }

                return empty;

            }

        }


        public ModuleType ParseModuleType(string content,ref int channelCount)
        {

            if (content.Contains("RTD"))
            {
                return ModuleType.RTD_3L;
            }
            else if (content.Contains("AI"))
            {
                if (content.Equals("8CHAI"))
                {
                    channelCount = 8;
                }
                else if (content.Equals("16CHAI"))
                {
                    channelCount = 16;
                }
                else
                {
                    channelCount = 8;
                }

                return ModuleType.AI;
            }
            else if (content.Contains("TC"))
            {
                if (content.Equals("8CHTC"))
                {
                    channelCount = 8;
                }
                else if (content.Equals("16CHTC"))
                {
                    channelCount = 16;
                }
                else
                {
                    channelCount = 16;
                }

                return ModuleType.TC;

            }
            else
            {
                try
                {
                    ModuleType type = (ModuleType)System.Enum.Parse(typeof(ModuleType), content);

                    switch (type)
                    {
                        case ModuleType.DI:
                            {
                                channelCount = 24;
                                break;
                            }
                        case ModuleType.DO:
                            {
                                channelCount = 16;
                                break;
                            }
                        case ModuleType.AO:
                        case ModuleType.RTD_3L:
                        case ModuleType.RTD_4L:
                        case ModuleType.PI:
                            {
                                channelCount = 8;
                                break;
                            }
                    }

                    return type;
                }
                catch
                {
                    return ModuleType.None;
                }
            }
            
        }

        public Station GetModuleStation(ModuleType moduleType)
        {
            if (moduleType != ModuleType.None)
            {
                return this.Stations[(int)moduleType-1];
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

        public Module ModuleNeedFeed
        {
            get
            {

                foreach (Module module in this.TestingModules)
                {
                    if (module.ModuleType >= ModuleType.DI && module.ModuleType <= ModuleType.PI)
                    {
                        // 数字量模块上料条件，模块空闲 && 数字量模块工位空闲
                        if (module.TestStep == TestStep.Idle && !this.ProcessController.StationIsBusy((StationType)module.StationId))
                        {
                            return module;
                        }
                    }
                }
                return null;
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

        private string Pack(bool result,string reply)
        {
            string resultText = result ? "Ok" : "Fail";
            if (string.IsNullOrEmpty(reply))
            {
                return string.Format("{0}", resultText);
            }
            else
            {
                return string.Format("{0},{1}", resultText, reply);
            }
        
        }

        private void  ProcessCommand(string  content)
        {
            string[] lines = content.Split(new char[1] { (char)0x0A});

            foreach (string line in lines)
            {

                string commandText  = line.Trim(new char[3] { (char)0x0A, (char)0x0D, (char)0x0 });

                if (!string.IsNullOrEmpty(commandText))
                {
                   
                    string[] commands = commandText.Split(new char[2] { ',', ' ' });
                    string reply = "Ok";
                    if (commands.Length >= 1)
                    {
                        try
                        {
                            CommandName command = (CommandName)Enum.Parse(typeof(CommandName), commands[0]);                          
                            switch (command)
                            {
                                case CommandName.Initialize:
                                    {
                                        reply = this.Initialize();
                                        reply = this.Pack(true, reply);
                                        break;
                                    }

                                /// <summary>
                                /// 获取当前系统状态
                                /// </summary>

                                case CommandName.GetSystemStatus:
                                    {
                                        reply = this.GetSystemStatus();
                                        reply = this.Pack(true, reply);
                                        break;
                                    }

                                /// <summary>
                                /// 启动试验进程
                                /// </summary>
                                case CommandName.StartTest:
                                    {
                                        reply = this.StartTest();

                                        break;
                                    }

                                /// <summary>
                                /// 停止试验进程，并复位
                                /// </summary>
                                case CommandName.StopTest:
                                    {
                                        reply = this.StopTest();
                                        break;
                                    }

                                /// <summary>
                                /// 设置模拟量通道数据
                                /// </summary>

                                case CommandName.SetAnalogueChannelValue:
                                    {
                                        if (commands.Length >= 5)
                                        {
                                            int stationlId = int.Parse(commands[1]);
                                            int channelId = int.Parse(commands[2]);
                                            int channelType = int.Parse(commands[3]);
                                            float value = float.Parse(commands[4]);
                                            reply = this.SetAnalogueChannelValue(stationlId, channelId, channelType, value);

                                        }
                                        else
                                        {
                                            reply = this.Pack(false,"");
                                        }

                                        break;
                                    }

                                /// <summary>
                                /// 获取模拟量通道数据
                                /// </summary>
                                case CommandName.GetAnalogueChannelValue:
                                    {
                                        if (commands.Length >= 4)
                                        {
                                            int staionId = int.Parse(commands[1]);
                                            int channelId = int.Parse(commands[2]);
                                            int channelType = int.Parse(commands[3]);
                                            reply = this.GetAnalogueChannelValue(staionId, channelId, channelType);

                                        }
                                        else
                                        {
                                            reply = this.Pack(false, "");
                                        }

                                        break;
                                    }

                                /// <summary>
                                /// 设置数字量通道数据
                                /// </summary>

                                case CommandName.SetDigitalChannelValue:
                                    {
                                        if (commands.Length >= 4)
                                        {
                                            int stationId = int.Parse(commands[1]);
                                            int channelId = int.Parse(commands[2]);
                                            bool value = int.Parse(commands[3]) ==1;

                                            reply = this.SetDigitalChannelValue(stationId, channelId, value);

                                        }
                                        else
                                        {
                                            reply = this.Pack(false, "");
                                        }

                                        break;
                                    }
                                /// <summary>
                                /// 获取数字量通道数据
                                /// </summary>

                                case CommandName.GetDigitalChannelValue:
                                    {
                                        if (commands.Length >= 3)
                                        {
                                            int stationId = int.Parse(commands[1]);
                                            int channelId = int.Parse(commands[2]);
                                            reply = this.GetDigitalChannelValue(stationId,channelId);
                                        }
                                        else
                                        {
                                            reply = this.Pack(false, "");
                                        }
                                        break;
                                    }

                                /// <summary>
                                /// 获取最后的错误码
                                /// </summary>

                                case CommandName.GetLastErrorCode:
                                    {
                                        reply = this.GetLastErrorCode();
                                        break;
                                    }

                                /// <summary>
                                /// 获取视觉 模块类型
                                /// </summary>

                                case CommandName.GetVISModuleType:
                                    {
                                        if (commands.Length >= 2)
                                        {
                                            int stationlId = int.Parse(commands[1]);
                                            reply = this.GetVISModuleType(stationlId);
                                        }
                                        else
                                        {
                                            reply = this.Pack(false, "");
                                        }

                                        break;
                                    }


                                /// <summary>
                                /// 获取视觉OCR 识别 模块码
                                /// </summary>

                                case CommandName.GetVISModuleCode:
                                    {
                                        if (commands.Length >= 2)
                                        {
                                            int stationlId = int.Parse(commands[1]);
                                            reply = this.GetVISModuleCode(stationlId);
                                        }
                                        else
                                        {
                                            reply = this.Pack(false, "");
                                        }
                                        break;
                                    }

                                /// <summary>
                                /// 获取视觉灯测结果
                                /// </summary>
                                case CommandName.GetVISLightingResult:
                                    {
                                        if (commands.Length >= 2)
                                        {
                                            int stationlId = int.Parse(commands[1]);
                                            reply = this.GetVISLightingResult(stationlId);
                                        }
                                        else
                                        {
                                            reply = this.Pack(false, "");
                                        }
                                        break;
                                    }


                                /// <summary>
                                /// 启动工位测试
                                /// </summary>
                                case CommandName.StartStationTest:
                                    {
                                        if (commands.Length >= 2)
                                        {
                                            int stationlId = int.Parse(commands[1]);
                                            reply = this.StartStationTest(stationlId);
                                        }
                                        else
                                        {
                                            reply = this.Pack(false, "");
                                        }
                                        break;
                                    }


                                /// <summary>
                                /// 设置测试工位测试结果
                                /// </summary>

                                case CommandName.SetTestResult:
                                    {
                                        if (commands.Length >= 3)
                                        {
                                            int stationlId = int.Parse(commands[1]);
                                            bool value = int.Parse(commands[2])== 1;
                                            reply = this.SetTestResult(stationlId, value);
                                        }
                                        else
                                        {
                                            reply = this.Pack(false, "");
                                        }
                                        break;
                                    }

                                case CommandName.ReleaseVISLighting:
                                    {
                                        if (commands.Length >= 2)
                                        {
                                            int stationlId = int.Parse(commands[1]);
                                            reply = this.ReleaseVISLighting(stationlId);
                                        }
                                        else
                                        {
                                            reply = this.Pack(false, "");
                                        }
                                        break;
                                    }
                                case CommandName.RequestVISLighting:
                                    {
                                        if (commands.Length >= 2)
                                        {
                                            int stationlId = int.Parse(commands[1]);
                                            reply = this.RequestVISLighting(stationlId);
                                        }
                                        else
                                        {
                                            reply = this.Pack(false, "");
                                        }
                                        break;
                                    }

                            }
                            reply = this.PackageReply(command.ToString(), reply);
                        }
                        catch
                        {
                            LogHelper.LogErrMsg(string.Format("无效的命令[{0}]", commandText));
                            reply = this.PackageReply(commands[0],"Fail");
                        }                       
                    }

                    this.SendCommandReply(reply);
                    if (!reply.Contains("GetSystemStatus"))
                    {
                        LogHelper.LogInfoMsg(string.Format("命令返回数据[{0}]", reply));
                    }
                }
            }
        }

        private string PackageReply(string command ,string reply)
        {
            return string.Format("{0},{1}", command, reply);
        }

        public void UpdateEnvironmentData()
        {
            float temperature = 0;
            float humidity = 0;
            if (this.EnvironmentDevice.GetEnvironmentData(ref temperature, ref humidity))
            {
                this.SystemMessage.Temperature = temperature;
                this.SystemMessage.Humidity = humidity;
            }
        }
        public void NotifyFeedingBoxMapValue()
        {
            if (this.testState != null && this.testState.testingState != TestingState.Recognize)
            {
                byte value = 0;
                for (int i = 0; i < this.TestingModules.Count; i++)
                {
                    Module module = this.TestingModules[i];
                    if (module != null)
                    {
                        bool result = !((int)module.TestStep >= (int)TestStep.Ready);
                        value = ByteUtils.SetBitValue(value, (byte)(module.PositionIndex - 1), result);
                    }
                }
                this.ProcessController.NotifyFeedingBoxMapValue((ushort)value);
            }
        }
        public override void ProcessResponse(int notifyEvent, string flag, string content, object result, string message, object sender)
        {           
            switch(notifyEvent)
            {
                case TCPService.EVENT_TYPE:
                    {
                        try
                        {
                            ChannelControl control = (ChannelControl)Enum.Parse(typeof(ChannelControl), flag);
                            if (control == ChannelControl.Report)
                            {
                                this.ProcessCommand(content);
                            }
                            else
                            {
                                LogHelper.LogErrMsg(string.Format("{0}", message));
                            }
                        }
                        catch
                        {
                            LogHelper.LogErrMsg(string.Format("ChannelControl参数解析失败[{0}]!", flag));
                            
                        }
                                      
                        break;
                    }
            }
        }


    }
}
