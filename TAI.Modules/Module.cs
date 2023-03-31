using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DMT.Core.Models;
using System.ComponentModel;

namespace TAI.Modules
{



    public enum ModuleType
    {
        None = -1,
        DI = 1,     //数字量输入模块（DI）	        pDI240，24通道
        DO = 2,     //数字量输出模块（DO）	        pDO160，16通道
        PI = 3,     //脉冲量输入模块（PI）	        pPI080，8通道，24V/48V 、5V两种类型
        AI = 4,     //模拟量输入模块（AI）	        nAI160，16通道
        AO = 5,     //模拟量输出模块（AO）	        nAO080，8通道       
        RTD_3L =6,  //热电阻温度采集模块（RTD）	nTD160_3Wire，16通道三线制
        RTD_4L = 7,  //热电阻温度采集模块（RTD）	nTD120_4Wire，12通道四线制
        TC = 8,    //热电偶温度采集模块（TC）	                    8路通道 
      
    }


    public enum Position
    {
        Origin =    0,
        FeedBase =  0,

        StationBase =99,
        DI =        100,     //数字量输入模块（DI）	        pDI240，24通道
        DO =        101,     //数字量输出模块（DO）	        pDO160，16通道
        PI =        102,     //脉冲量输入模块（PI）	        pPI080，8通道，24V/48V 、5V两种类型

        AI =        103,     //模拟量输入模块（AI）	        nAI160，16通道
        AO =        104,     //模拟量输出模块（AO）	        nAO080，8通道       
        RTD_3L =    105,  //热电阻温度采集模块（RTD）	nTD160_3Wire，16通道三线制
        RTD_4L =    106,  //热电阻温度采集模块（RTD）	nTD120_4Wire，12通道四线制
        TC =        107,   //热电偶温度采集模块（TC）	                    8路通道 
        Prepare =   108,  // 预热


        StationQRBase =19,
        DI_QR =     20,     
        DO_QR =     21,     
        PI_QR =     22,     
        AI_QR =     23,    
        AO_QR =     24,    
        RTD_3L_QR = 25,
        RTD_4L_QR = 26,
        TC_QR =     27,
        Prepare_QR= 28,

        Out_OK =   30,
        Out_NG =   31,

    }

    public enum TestStep
    {
        [Description("空闲")]
        Idle = 1,
        [Description("上料")]
        Feeding =2,
        [Description("准备完成")]
        Ready =3,
        [Description("测试中")]
        Testing = 4,
        [Description("预热")]
        Prepare = 5,
        [Description("下料")]
        Blanking = 6,
        [Description("测试完成")]
        Finish = 7,
    }


    public enum Status
    { 
        Busy =1,
        Idle =0,
        Completed =1,
        Valid =1,
        Invalid =0,
        Enable =1,
    }

    public class SystemMessage : StatusMessage
    {
        [JsonProperty(propertyName: "devices")]
        public List<StatusMessage> Devices { get; set; }

        [JsonProperty(propertyName: "stations")]
        public List<StationStatus> Stations { get; set; }

        public SystemMessage(string name) : base(name)
        {
            this.Devices = new List<StatusMessage>();
            this.Stations = new List<StationStatus>();
        }
    }



    public class Module
    {

        public const int PREPARE_TIME = 1;
        public int Id { get; set; }
        public string SerialCode { get; set; }
        public ModuleType ModuleType { get; set; }
        public int ChannelCount { get; set; }
        private TestStep testStep { get; set; }
        public TestStep TestStep { 
            get
            { 
                return testStep;
            }
            set 
            {
                if (this.LinkStation != null)
                {
                    this.LinkStation.TestStep = value;
                }
                this.testStep = value;
            } 
        }
        public bool Enable { get; set; }
        public bool Conclusion {  set
            {
                this.CurrentPosition = this.TargetPosition;
                this.TargetPosition = value ? Position.Out_OK : Position.Out_NG;
            } 
        }

        public int QRPositionValue { get
                => (int)this.LinkStation.QRPosition;
                }
        public Position CurrentPosition { get; set; }
        public ushort PositionIndex { get; set; }

        public Station LinkStation { get; set; }
        public int PrepareMinutes { get; set; }
       

        public DateTime StartDateTime { get; set; }


        public int StationId { get
            {
               return  (int)this.ModuleType;
            } }

        public Module(int index)
        {
            this.Id = index;
            this.SerialCode = "";
            this.ModuleType = ModuleType.None;
            this.ChannelCount = 0;
            this.TestStep = TestStep.Idle;
            this.PositionIndex =(ushort) index;
            this.Enable = false;
            this.Conclusion = true;
            this.PrepareMinutes = PREPARE_TIME;
        }


        public Module()
        {
            this.Id = 0;
            this.SerialCode = "";
            this.ModuleType = ModuleType.None;
            this.ChannelCount = 0;
            this.TestStep = TestStep.Idle;
            this.Enable = false;
            this.PrepareMinutes = PREPARE_TIME;
        }


        public string Description
        {
            get => string.Format("[{0}:{1}]",this.SerialCode,this.ModuleType.ToString());
        }

        public ushort CurrentPositionValue
        {
            get
            {
                if (this.CurrentPosition == Position.FeedBase)
                {
                    return (ushort)((ushort)Position.FeedBase + this.PositionIndex);
                }
                 
                return (ushort)this.CurrentPosition;
            }
        
        }

        //目标位置
        public Position TargetPosition { get; set; }

        public ushort TargetPositionValue { 
            get {
                return (ushort)this.TargetPosition;
            } }

        public Module Clone()
        {
            Module module = new Module()
            {
                SerialCode = this.SerialCode,
                ModuleType = this.ModuleType,
                ChannelCount = this.ChannelCount,
                TestStep = this.TestStep,
                CurrentPosition = this.CurrentPosition,
                PositionIndex = this.PositionIndex,
                LinkStation = this.LinkStation,
                TargetPosition = this.LinkStation.TestPosition,
            };
            return module;
        }



        public bool PrepareReady
        {
            get {
                TimeSpan span = DateTime.Now - this.StartDateTime;
                return span.TotalMinutes >= this.PrepareMinutes;                        
            }
        }


    }


}





