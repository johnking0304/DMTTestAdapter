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
        public DeviceMaster MeasureDevice { get; set; }
        public DeviceMaster GeneratorDevice { get; set; }
        public ProcessController ProcessController { get; set; }
        //public SwitchController SwitchController { get; set; }
        public List<SwitchController> SwitchControllers { get; set; }

        

        public VISController VISController { get; set; }

        public TestState TestState { get; set; }

        public SystemMessage SystemMessage { get; set; }

        public OperateCommand Command { get; set; }

        //测试模块
        public List<Module> TestingModules { get; set; }

        public List<Station> Stations { get; set; }

        public TCPService Service { get; set; }

        /// <summary>
        /// 初始化完成
        /// </summary>
        public bool InitializeCompleted { get; set; }



        public TestAdapter()
        {
            this.Caption = "TestAdapter";
            
            this.Command = OperateCommand.None;
            this.TestingModules = new List<Module>();
            this.Stations = new List<Station>();
            this.SystemMessage = new SystemMessage(this.Caption);

            this.Service = new TCPService();

            this.Service.AttachObserver(this.subjectObserver.Update);


            foreach (StationType type in Enum.GetValues(typeof(StationType)))
            {
                Station station = new Station(type);
                this.Stations.Add(station);
                this.SystemMessage.Stations.Add(station.StationStatus);
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
                      
            this.LoadFromFile(Contant.CONFIG);
            this.DigitalDevice = new DigitalDevice();
            this.DigitalDevice.LoadFromFile(Contant.DIGITAL_CONFIG);
            this.DigitalDevice.Open();
            this.DigitalDevice.Start();
            this.SystemMessage.Devices.Add(this.DigitalDevice.StatusMessage);

            
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


            this.StartThread();
            
        }


        public void Dispose()
        {
            this.StopThread();

            this.Service.Close();

            this.DigitalDevice.Close();

            this.MeasureDevice.Close();

            this.GeneratorDevice.Close();

            this.ProcessController.Close();

            this.VISController.Close();

            foreach (SwitchController switchController in this.SwitchControllers)
            {
                switchController.Close();
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

            foreach (SwitchController switchController in this.SwitchControllers)
            {
                result &= switchController.Initialize();
            }

            this.InitializeCompleted = true;
            this.TestingModules.Clear();

            return  this.SystemMessageText;
        }



        public string GetAnalogueChannelValue(int stationId,int channelId, int type)
        {
            LogHelper.LogInfoMsg(string.Format("接收命令:获取模拟量通道数据[工位={0},通道={1},类型={2}]", ((StationType)stationId).ToString(), channelId, ((ChannelType)type).ToString()));
            if (stationId >= (int)StationType.AI && stationId <= (int)StationType.TC)
            {
                this.SwitchChannelOperate(stationId, (ushort)channelId, type);
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
            LogHelper.LogInfoMsg(string.Format("接收命令:获取系统状态"));
            return  this.SystemMessageText;
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
                    result = this.SwitchChannelOperate(stationId, (ushort)channelId, type);
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

        public string EnableStationTest(int StationId)
        {
            this.Command = OperateCommand.EnableStationTest;
            LogHelper.LogInfoMsg(string.Format("接收命令:使能工位[{0}]测试", StationId));
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
            if (this.VISController.OCRChannelLighting((ModuleType)StationId, ref value))
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
                }
                return "Ok";
            }
            return "Fail";
            
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
            if (content.Contains("24/48"))
            {
                return ModuleType.PI;
            }
            else if (content.Contains("24CH"))
            {
                return ModuleType.DI;
            }
            else if (content.Contains("16CH") && content.Contains("O") && content.Contains("D"))
            {
                return ModuleType.DO;
            }

            else if (content.Contains("8CH") && content.Contains("A") && content.Contains("O"))
            {
                return ModuleType.AO;
            }
            else if (content.Contains("16CH") && content.Contains("A") && content.Contains("I"))
            {
                return ModuleType.AI;
            }
            else if (content.Contains("TC"))
            {
                return ModuleType.TC;
            }
            else if (content.Contains("RTD"))
            {
                return ModuleType.RTD_3L;
            }

                return ModuleType.None;
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
                        if (module.TestStep == TestStep.Idle)
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
