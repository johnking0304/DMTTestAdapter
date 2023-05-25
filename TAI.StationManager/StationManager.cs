using DMT.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;
using TAI.Constants;
using TAI.Device;
using TAI.Manager;
using TAI.Modules;
using DMT.Core.Channels;
using System.IO;
using Newtonsoft.Json;
using TAI.TestDispatcher;
using TAI.Test.Scheme;

namespace TAI.StationManager
{
    public class StationManager : BaseController
    {
        private DeviceModel MeasureDeviceModel { get; set; }
        private DeviceModel GeneratorDeviceModel { get; set; }
        public DigitalDevice DigitalDevice { get; set; }
        public DeviceMaster MeasureDevice { get; set; }
        public DeviceMaster GeneratorDevice { get; set; }
        public List<SwitchController> SwitchControllers { get; set; }

        public DeviceType DeviceType { get; set; }
        public List<Station> Stations { get; set; }
        public ControlUnit ControlUnit { get; set; }



        public StationManager()
        {
            this.Caption = "StationManager";
            this.Stations = new List<Station>();
            this.WaitMilliseconds = 5000;
            foreach (StationType type in Enum.GetValues(typeof(StationType)))
            {
                Station station = new Station(type);
                this.Stations.Add(station);
                station.LoadCompensates(Contant.STATIONS);
            }
            this.LoadConfig();
        }



        private void LoadConfig()
        {
            this.LoadFromFile(Contant.CONFIG);
            this.DigitalDevice = new DigitalDevice();
            this.DigitalDevice.LoadFromFile(Contant.DIGITAL_CONFIG);
            this.DigitalDevice.Open();
            this.DigitalDevice.Start();

            this.MeasureDevice = AnalogDeviceFactory.CreateDevice(this.MeasureDeviceModel);
            this.MeasureDevice.LoadFromFile(Contant.ANALOG_CONFIG);
            this.MeasureDevice.Open();

            this.GeneratorDevice = AnalogDeviceFactory.CreateDevice(this.GeneratorDeviceModel);
            this.GeneratorDevice.LoadFromFile(Contant.ANALOG_CONFIG);
            this.GeneratorDevice.Open();
            this.SwitchControllers = new List<SwitchController>();

            for (int i = (int)StationType.AI; i <= (int)StationType.TC; i++)
            {
                SwitchController switchController = new SwitchController((StationType)i);
                switchController.LoadFromFile(Contant.SWITCH_CONFIG);
                if (switchController.Enable)
                {
                    switchController.Open();
                    switchController.Start();
                }
                this.SwitchControllers.Add(switchController);
            }
            this.StartThread();

        }



        public bool Initialize()
        {
            bool result = this.DigitalDevice.Initialize();
            result &= this.MeasureDevice.Initialize();
            result &= this.GeneratorDevice.Initialize();

            foreach (SwitchController switchController in this.SwitchControllers)
            {
                if (switchController.Enable)
                {
                    result &= switchController.Initialize();
                }
            }

            return result;
        }

        public bool LoadControlUnit(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    string content = Files.LoadFromFile(fileName);
                    this.ControlUnit = JsonConvert.DeserializeObject<ControlUnit>(content);
                    return true;
                }
                catch
                {
                    LogHelper.LogErrMsg(string.Format("反序列化CU信息失败[{0}]", fileName));
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        public void Dispose()
        {
            this.StopThread();


            this.DigitalDevice.Close();

            this.MeasureDevice.Close();

            this.GeneratorDevice.Close();


            foreach (SwitchController switchController in this.SwitchControllers)
            {
                if (switchController.Enable)
                {
                    switchController.Close();
                }
            }

        }


        public void RegisterDeviceFunction(CardModule module)
        {
            foreach (TestItemNode item in module.TestDispatcher.TestScheme.TestItems)
            {
                switch (item.SignalItem.SignalTypeFrom)
                {
                    case SignalType.AnalogCom:
                        {
                            item.SetChannelValue += this.SetAnalogueChannelValue;
                            break;
                        }
                    case SignalType.DigitalCom:
                        {
                            item.SetChannelValue += this.SetDigitalChannelValue;
                            break;
                        }
                    case SignalType.NuCON:
                        {
                            item.SetChannelValue += this.SetNuCONChannelValue;
                            break;
                        }
                }

                switch (item.SignalItem.SignalTypeTo)
                {
                    case SignalType.AnalogCom:
                        {
                            item.GetChannelValue += this.GetAnalogueChannelValue;
                            break;
                        }
                    case SignalType.DigitalCom:
                        {
                            item.GetChannelValue += this.GetDigitalChannelValue;
                            break;
                        }
                    case SignalType.NuCON:
                        {
                            item.GetChannelValue += this.GetNuCONChannelValue;
                            break;
                        }
                }


            }

        }



        public void  StartModuleTest(CardModule module)
        {
            module.Initialize();
            this.RegisterDeviceFunction(module);
            module.TestDispatcher.StartWorking();
        }



        public override void LoadFromFile(string fileName)
        {
            string value = IniFiles.GetStringValue(fileName, this.Caption, "MeasureDeviceModel", "Fluke8846");
            this.MeasureDeviceModel = (DeviceModel)Enum.Parse(typeof(DeviceModel), value);
            value = IniFiles.GetStringValue(fileName, this.Caption, "GeneratorDeviceModel", "Fluke7526");
            this.GeneratorDeviceModel = (DeviceModel)Enum.Parse(typeof(DeviceModel), value);
            base.LoadFromFile(fileName);

        }



        public override void SaveToFile(string fileName)
        {
            IniFiles.WriteStringValue(fileName, this.Caption, "MeasureDeviceModel", this.MeasureDeviceModel.ToString());
            IniFiles.WriteStringValue(fileName, this.Caption, "GeneratorDeviceModel", this.GeneratorDeviceModel.ToString());
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
                            case StationType.RTD3:
                            case StationType.RTD4:
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
        private bool SwitchChannelOperate(int stationId, ushort channelId, int type)
        {
            SwitchMode mode = this.GetSwitchChannelMode(stationId, type);
            int index = stationId - (int)StationType.AI;
            bool result = this.SwitchControllers[index].SwitchModeOperate(mode);
            return this.SwitchControllers[index].SwitchChannelOperate(channelId);
        }







        private int ConvertChannelId(int stationId, int sourceId)
        {
            switch ((StationType)stationId)
            {
                case StationType.TC:
                case StationType.AI:
                case StationType.AO:
                    {
                        return ChannelIdLink.GetOutput(sourceId);
                    }
                case StationType.RTD3:
                case StationType.RTD4:

                    {
                        return sourceId;
                    }
            }
            return sourceId;
        }

        public bool GetAnalogueChannelValue(int stationId, string channel, int channelDataType, ref float dataVale)
        {
            int channelId = int.Parse(channel);

            LogHelper.LogInfoMsg(string.Format("获取模拟量通道数据[工位={0},通道={1},类型={2}]", ((StationType)stationId).ToString(), channelId, ((ChannelType)channelDataType).ToString()));
            if (stationId >= (int)StationType.AI && stationId <= (int)StationType.TC)
            {
                int linkChannelId = ConvertChannelId(stationId, channelId);
                LogHelper.LogInfoMsg(string.Format("实际切换通道:{0}->{1}", channelId, linkChannelId));

                this.SwitchChannelOperate(stationId, (ushort)linkChannelId, channelDataType);
                float value = 0;
                bool result = this.MeasureDevice.GetValue((ChannelType)channelDataType, ref value);
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool GetDigitalChannelValue(int stationId, string channel, int channelDataType, ref float dataVale)
        {
            int channelId = int.Parse(channel);
            bool value = false;
            LogHelper.LogInfoMsg(string.Format("获取数字量通道数据[通道={0}]", channelId));
            bool result = this.DigitalDevice.GetValue(channelId, ref value);
            return result;
        }


        public bool SetAnalogueChannelValue(int stationId, string channel, int channelDataType, float value)
        {
            int channelId = int.Parse(channel);
            if (stationId >= (int)StationType.PI && stationId <= (int)StationType.TC)
            {
                LogHelper.LogInfoMsg(string.Format("设置模拟量通道数据[类型={0},通道={1},类型={2},值={3}]", ((ModuleType)stationId).Description(), channelId, ((ChannelType)channelDataType).Description(), value));
                bool result = true;
                if (stationId == (int)StationType.PI)
                {
                    result &= this.DigitalDevice.SetPluseOutputMode(PluseMode.PL10KHZ);
                    result &= this.DigitalDevice.SelectPIChannel(channelId);
                }
                else
                {
                    int linkChannelId = ConvertChannelId(stationId, channelId);

                    LogHelper.LogInfoMsg(string.Format("实际切换通道:{0}->{1}", channelId, linkChannelId));

                    result = this.SwitchChannelOperate(stationId, (ushort)linkChannelId, channelDataType);

                    Station station = this.Stations[stationId - 1];


                    value = station.CompensateValue(linkChannelId, value);


                    /// 如果温度设置值 大于800 则按800 设置
                    if ((ChannelType)channelDataType == ChannelType.Resistance)
                    {
                        if (value > 800)
                        {
                            LogHelper.LogInfoMsg(string.Format("[{0}]设置值[{1}]大于800，按800设置", ((ChannelType)channelDataType).Description(), value));
                            value = 800;
                        }
                    }

                    result &= this.GeneratorDevice.SetValue((ChannelType)channelDataType, value);
                }
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool SetDigitalChannelValue(int stationId, string channel, int channelDataType, float value)
        {
            int channelId = int.Parse(channel);
            LogHelper.LogInfoMsg(string.Format("设置数量通道数据[通道={0},值={1}]", channelId, value));
            bool setValue = DoubleUtils.AreEqual(value, 1);
            bool result = this.DigitalDevice.SetValue(channelId, setValue);
            return result;
        }


        public bool SetNuCONChannelValue(int stationId, string channel, int channelDataType, float value)
        {

            return true;

        }

        public bool GetNuCONChannelValue(int stationId, string channel, int channelDataType, ref float dataVale)
        {
            return true;
        }
    }
}
