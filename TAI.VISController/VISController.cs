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
        public int ChannelCount { get; set; }
        public string ModelType { get; set; }
        public string ModelSerialCode { get; set; }

        

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
                    return item.Value;
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

        public int GetModuleLightingProgramId(ModuleType type)
        {
            VISProgram item = (VISProgram)Enum.Parse(typeof(VISProgram), type.ToString());
            return this.GetProgramId(item);
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


        private bool StartSwitchProgram(int ProgramId)
        {
            SwitchProgramCommand command = new SwitchProgramCommand(this, ProgramId);
            string content = this.TCPChannel.SendCommandSync(command.PackageString());
            return command.ParseResponse(content);
        }



        private bool StartOCRecognize(OCRType ocrType)
        {
            OCRecognizeCommand command = new OCRecognizeCommand(this, ocrType);
            
            string content = this.TCPChannel.SendCommandSync(command.PackageString());
            return command.ParseResponse(content);
        }



        public bool OCRModelType(ref string modelType)
        {
            if (this.StartSwitchProgram(this.ModuleProgramId))
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

        public bool OCRChannelLighting(int channelCount,ModuleType module, ref bool[] channels)
        {

            if (this.StartSwitchProgram(this.GetModuleLightingProgramId(module)))
            {
                this.ChannelCount = channelCount;
                this.ChannelResults.Clear();
                if (this.StartOCRecognize(OCRType.ChannelLighting))
                {
                    bool value = false;
                    for (int i = 0; i < channelCount; i++)
                    {
                        string channelName = string.Format("CH{0}", i);

                        if (this.GetChannelValue(channelName, ref value))
                        {
                            channels[i] = value;
                        }
                        else
                        {
                            channels[i] = false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool QRModelSerialCode(ref string serialCode)
        {

            if (this.StartSwitchProgram(this.ModuleSerialCodeProgramId))
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
