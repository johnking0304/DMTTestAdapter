using System;
using DMT.Core.Models;
using DMT.Core.Channels;
using Newtonsoft.Json;
using System.Collections.Generic;
using TAI.Manager.VIS;
using DMT.Core.Utils;
using TAI.Modules;

namespace TAI.Manager
{

    public enum VISProgram
    {
        MODULE = 0,  //模块标识
        BARCODE = 1,//条形码
        DI = 2,     //数字量输入模块（DI）	     
        DO = 3,     //数字量输出模块（DO）	     
        PI = 4,     //脉冲量输入模块（PI）
        AI = 5,     //模拟量输入模块（AI）	
        AO = 6,     //模拟量输出模块（AO）	     
        TC = 7,     //热电偶温度采集模块（TC） 
        RTD_3L = 8,   //热电阻温度采集模块（RTD)
        RTD_4L = 8,   //热电阻温度采集模块（RTD)
    }



    /// <summary>
    /// 视觉系统
    /// </summary>
    public class VISController : BaseDevice
    {
        public List<KeyValuePair<string, int>> ChannelResults { get; set; }
        public int ChannelId { get; set; }
        public string ModelType { get; set; }
        public int ChannelCount { get; set; }
        public string ModelSerialCode { get; set; }
        public string LightingContent { get; set; }

        public int ProgramId { get; set; }

        public int ProgramDeviceType { get; set; }


        public int TryCountMax { get; set; }

        public List<KeyValuePair<string,int>> ProgramIds { get; set; }

        public TCPClientChannel TCPChannel {
            get {
                return (TCPClientChannel)this.Channel;
            }
        }
        public VISController() : base()
        {
            this.Caption = "VISController";
            this.Channel = new TCPClientChannel(this.Caption);
            this.ChannelResults = new List<KeyValuePair<string, int>>();
            this.ProgramIds = new List<KeyValuePair<string, int>>();
            this.StatusMessage.Name = this.Caption;
            this.ProgramId = -1;
            this.ProgramDeviceType = 0;
            this.TryCountMax = 3;
        }

        private bool GetChannelValue(string name ,ref bool value)
        {
            foreach(KeyValuePair<string, int> pair in this.ChannelResults)
            { 
                if (pair.Key.Equals(name))
                {
                    value = pair.Value == 1;
                    return true;
                }
            }
            return false;         
        }

        public override void SaveToFile(string fileName)
        {
            string seciton = "ProgramIds";
            foreach (KeyValuePair<string, int> program in this.ProgramIds)
            {
                IniFiles.WriteIntValue(fileName, seciton, program.Key, program.Value);
            }

        }

        public override void LoadFromFile(string fileName)
        {
            this.Channel.LoadFromFile(fileName);

            string[] nameList = System.Enum.GetNames(typeof(VISProgram));            
            string seciton = "ProgramIds";
            foreach (string name in nameList)
            {
                VISProgram item = (VISProgram)Enum.Parse(typeof(VISProgram), name);
                int programId  = IniFiles.GetIntValue(fileName, seciton, name, (int)item);

                KeyValuePair<string, int> program = new KeyValuePair<string, int>(name, programId);
                this.ProgramIds.Add(program);
            }

            base.LoadFromFile(fileName);
        }

        public int GetProgramId(VISProgram program)
        {
            foreach (KeyValuePair<string, int> item in this.ProgramIds)
            {
                if (item.Key == program.ToString())
                {
                    return item.Value + this.ProgramDeviceType;
                }              
            }
            return -1;
        }


        public int ModuleProgramId
        {
            get {
                return this.GetProgramId(VISProgram.MODULE);
                }
        }

        public int ModuleSerialCodeProgramId
        {
            get
            {
                return this.GetProgramId(VISProgram.BARCODE);
            }
        }

        public int GetModuleLightingProgramId(ModuleType type,int channelCount)
        {
            VISProgram item = (VISProgram)Enum.Parse(typeof(VISProgram), type.ToString());
            int pid = this.GetProgramId(item);
            switch (type)
            {
                case ModuleType.AI:
                case ModuleType.TC:
                    {
                        pid = pid + 10;
                        break;
                    }
            }
            return pid;

        }



        public bool Active()
        {
           return this.Channel.Active();
        }

        public bool Close()
        {
            return this.Channel.Close();
        }

        public bool Initialize()
        {
            return this.Active();
        }

        public bool Open()
        {
            this.Channel.OpenSync();
            return this.Active();
        }

        public void Start()
        {
            this.StartThread();
        }


        public bool StartSwitchProgram(int ProgramId)
        {
            SwitchProgramCommand command = new SwitchProgramCommand(this, ProgramId);
            string content = this.TCPChannel.SendCommandSync(command.PackageString());            
            return command.ParseResponse(content);
        }



        private bool StartOCRecognize(OCRType ocrType)
        {
            OCRecognizeCommand command = new OCRecognizeCommand(this, ocrType);
            
            string content = this.TCPChannel.SendCommandSync(command.PackageString());
            if (content.Contains("T1"))
            {
                switch (ocrType)
                {
                    case OCRType.ChannelLighting:
                        {
                            if (!content.Contains("Channels"))
                            {
                                content = this.TCPChannel.Receive();
                            }
                            break;
                        }
                    case OCRType.ModelSerialCode:
                        {
                            if (!content.Contains("Code"))
                            {
                                content = this.TCPChannel.Receive();
                            }
                            break;
                        }
                    case OCRType.ModelType:
                        {
                            if (!content.Contains("Module"))
                            {
                                content = this.TCPChannel.Receive();
                            }
                            break;
                        }
                       
                }
                return command.ParseResponse(content);

            }
            return false;
            
        }


        public bool SwitchProgramToOCRModelType()
        {
            
            if (this.ProgramId != this.ModuleProgramId)
            {
                this.ProgramId = this.ModuleProgramId;
                return this.StartSwitchProgram(this.ModuleProgramId);
            }
            return true;            
        }


        public bool TryOCRModelType(ref string modelType)
        {
            bool result = OCRModelType(ref modelType);
            int tryCount = 1;
            while (!result)
            {
                if (tryCount <= this.TryCountMax)
                {
                    result = OCRModelType(ref modelType);
                    tryCount += 1;
                    LogHelper.LogInfoMsg(string.Format("重试[模块类型]识别，次数[{0}]", tryCount));                    
                }
                else
                {
                    LogHelper.LogInfoMsg(string.Format("[模块类型]识别次数[{0}]已经超出最大重试次数[{1}]", tryCount,this.TryCountMax));
                    break;
                }
            }
            return result;
        }


        public bool OCRModelType(ref string modelType)
        {            
            if (this.SwitchProgramToOCRModelType())
            {
                this.ModelType = "";
                if (this.StartOCRecognize(OCRType.ModelType))
                {
                    modelType = this.ModelType;
                    return true;
                }
            }
            return false;
        }



        public bool TryOCRChannelLighting(ModuleType module,int channelCount, ref string content)
        {
             
            bool result = OCRChannelLighting(module, channelCount, ref content);
            int tryCount = 1;
            while (!result)
            {
                if (tryCount <= this.TryCountMax)
                {
                    result = OCRChannelLighting(module, channelCount, ref content);
                    tryCount += 1;
                    LogHelper.LogInfoMsg(string.Format("[{0}]重试[灯测]识别，次数[{1}]", module.Description(), tryCount));
                }
                else
                {
                    LogHelper.LogInfoMsg(string.Format("[{0}][灯测]识别次数[{1}]已经超出最大重试次数[{2}]", module.Description(),tryCount, this.TryCountMax));
                    break;
                }
            }
            return result;
        }



        public bool OCRChannelLighting(ModuleType module,int channelCount,ref string content)
        {
            int program = this.GetModuleLightingProgramId(module,channelCount);
            if (this.ProgramId != program)
            {
                this.StartSwitchProgram(program);
                this.ProgramId = program;
            }
              
            this.ChannelResults.Clear();
            if (this.StartOCRecognize(OCRType.ChannelLighting))
            {
                content = this.LightingContent;
                return true;
            }
            
            return false;
        }



        public bool SwitchProgramToQRModelSerialCode()
        {
            
            if (this.ProgramId != this.ModuleSerialCodeProgramId)
            {
                this.ProgramId = this.ModuleSerialCodeProgramId;

                return this.StartSwitchProgram(this.ModuleSerialCodeProgramId);
            }
            return true;
        }

        public bool TryQRModelSerialCode(ref string serialCode)
        {
            bool result = QRModelSerialCode(ref serialCode);
            int tryCount = 1;
            while (!result)
            {
                if (tryCount <= this.TryCountMax)
                {
                    result = QRModelSerialCode(ref serialCode);
                    tryCount += 1;
                    LogHelper.LogInfoMsg(string.Format("重试[模块二维码]识别，次数[{0}]", tryCount));
                }
                else
                {
                    LogHelper.LogInfoMsg(string.Format("[模块二维码]识别次数[{0}]已经超出最大重试次数[{1}]", tryCount, this.TryCountMax));
                    break;
                }
            }
            return result;
        }


        public bool QRModelSerialCode(ref string serialCode)
        {

            if (this.SwitchProgramToQRModelSerialCode())
            {
                this.ModelSerialCode = "";
                if (this.StartOCRecognize(OCRType.ModelSerialCode))
                {
                    serialCode = this.ModelSerialCode;
                    return true;
                }
            }
            return false;
        }


        public override void ProcessEvent()
        {
            if (!this.Active())
            {
                //通道读写错误，重连判断及动作
                this.TCPChannel.ReConnect();
            }
        }

    }
}
