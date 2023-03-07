using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DMT.Core.Models;

namespace TAI.Modules
{
    public enum ModuleType
    {
        None =-1,
        AI = 0,     //模拟量输入模块（AI）	        nAI160，16通道
        AO = 1,     //模拟量输出模块（AO）	        nAO080，8通道
        DI = 2,     //数字量输入模块（DI）	        pDI240，24通道
        DO = 3,     //数字量输出模块（DO）	        pDO160，16通道
        PI = 4,     //脉冲量输入模块（PI）	        pPI080，8通道，24V/48V 、5V两种类型
        RTD_3L = 5,  //热电阻温度采集模块（RTD）	nTD160_3Wire，16通道三线制
        RTD_4L = 6,  //热电阻温度采集模块（RTD）	nTD120_4Wire，12通道四线制
        TC = 7 ,      //热电偶温度采集模块（TC）	                    8路通道       
    }



    public enum StationType 
    {
        AI = 0,     //模拟量输入模块（AI）	        nAI160，16通道
        AO = 1,     //模拟量输出模块（AO）	        nAO080，8通道
        DI = 2,     //数字量输入模块（DI）	        pDI240，24通道
        DO = 3,     //数字量输出模块（DO）	        pDO160，16通道
        PI = 4,     //脉冲量输入模块（PI）	        pPI080，8通道，24V/48V 、5V两种类型
        RTD_3L = 5,  //热电阻温度采集模块（RTD）	nTD160_3Wire，16通道三线制
        RTD_4L = 6,  //热电阻温度采集模块（RTD）	nTD120_4Wire，12通道四线制
        TC = 7,      //热电偶温度采集模块（TC）	                    8路通道 
        PREPARE = 8,  // 预热
    }


    /*
        Idle = 0, //空闲状态

        Feeding = 1,//上料状态
        
        Testing = 2,//测试状态

        Blanking =3,//下料状态

        Pause = 4,  //暂停状态
       
        Error = 5, //故障状态
    */




    public enum Position
    {
        Origin = 100,
        FeedBase =0,
        DI = 7,     //数字量输入模块（DI）	        pDI240，24通道
        DO = 8,     //数字量输出模块（DO）	        pDO160，16通道
        PI = 9,     //脉冲量输入模块（PI）	        pPI080，8通道，24V/48V 、5V两种类型
        AI = 10,     //模拟量输入模块（AI）	        nAI160，16通道
        AO = 11,     //模拟量输出模块（AO）	        nAO080，8通道       
        RTD_3L = 12,  //热电阻温度采集模块（RTD）	nTD160_3Wire，16通道三线制
        RTD_4L = 13,  //热电阻温度采集模块（RTD）	nTD120_4Wire，12通道四线制
        TC = 14,      //热电偶温度采集模块（TC）	                    8路通道 
        Prepare = 15,  // 预热
        DI_QR = 20,     
        DO_QR = 21,     
        PI_QR = 22,     
        AI_QR = 23,    
        AO_QR = 24,    
        RTD_3L_QR = 25,
        RTD_4L_QR = 26,
        TC_QR = 27,
        Prepare_QR =28,
        Feed_OK =30,
        Feed_NG =31,

    }

    public enum TestStep
    {
        Idle = 0,
        Wait = 1,
        Testing = 2,
        Prepare = 3,
        Finish = 4,
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





    public class Module
    {
        public string SerialCode { get; set; }
        public ModuleType ModuleType { get; set; }
        public int ChnanelCount { get; set; }
        public TestStep TestStep { get; set; }
        
        //起始位置
        public Position StartPosition { get; set; }
        public ushort PositionIndex { get; set; }

        public Module(int index)
        {
            this.SerialCode = "";
            this.ModuleType = ModuleType.None;
            this.ChnanelCount = 0;
            this.TestStep = TestStep.Idle;
            this.PositionIndex =(ushort) index;     
        }

        public Module()
        {
            this.SerialCode = "";
            this.ModuleType = ModuleType.None;
            this.ChnanelCount = 0;
            this.TestStep = TestStep.Idle;
        }


        public ushort StartPositionValue
        {
            get {
                if (this.StartPosition == Position.FeedBase)
                {
                    return (ushort)((ushort)Position.FeedBase + this.PositionIndex);
                }
                return (ushort)this.StartPosition;
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
                ChnanelCount = this.ChnanelCount,
                TestStep = this.TestStep,
                StartPosition = this.StartPosition,
                PositionIndex = this.PositionIndex,
            };
            return module;
        }
    }





    public class SystemMessage : StatusMessage
    {
        [JsonProperty(propertyName: "results")]
        public List<StatusMessage> Results { get; set; }

        public SystemMessage():base()
        {
            this.Results = new List<StatusMessage>();
        }
    }

}
