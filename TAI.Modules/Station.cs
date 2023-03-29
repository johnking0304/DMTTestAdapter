using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DMT.Core.Utils;

namespace TAI.Modules
{

    public enum StationType
    {
        [Description("DI测试工位")]
        DI = 1,     //数字量输入模块（DI）	        pDI240，24通道
        [Description("DO测试工位")]
        DO = 2,     //数字量输出模块（DO）	        pDO160，16通道
        [Description("PI测试工位")]
        PI = 3,     //脉冲量输入模块（PI）	        pPI080，8通道，24V/48V 、5V两种类型
        [Description("AI测试工位")]
        AI = 4,     //模拟量输入模块（AI）	        nAI160，16通道
        [Description("AO测试工位")]
        AO = 5,     //模拟量输出模块（AO）	        nAO080，8通道       
        [Description("RTD三线测试工位")]
        RTD_3L = 6,  //热电阻温度采集模块（RTD）	nTD160_3Wire，16通道三线制
        [Description("RTD四线测试工位")]
        RTD_4L = 7,  //热电阻温度采集模块（RTD）	nTD120_4Wire，12通道四线制
        [Description("TC测试工位")]
        TC = 8,    //热电偶温度采集模块（TC）	                    8路通道 
        [Description("预热工位")]
        Prepare = 9,  // 预热
    }


    public class StationStatus
    {
        [JsonProperty(propertyName: "id")]
        public StationType StationType { get; set; }

        [JsonProperty(propertyName: "station")]
        public string station { get => this.StationType.ToString(); }

        [JsonProperty(propertyName: "status")]
        public TestStep TestStep { get; set; }

        [JsonProperty(propertyName: "statusText")]
        public string StatusText
        {
            get {
                return this.TestStep.Description();
            }
        }

        public StationStatus(StationType type)
        {
            this.StationType = type;
            this.TestStep = TestStep.Idle;
            
        }
    }




    public class Station
    {
        public Position TestPosition { get; set; }

        public Position QRPosition { get; set; }      
        public StationType StationType { get; set; }
        public Module LinkedModule { get; set; }
        public TestStep TestStep {  set
            {
                this.StationStatus.TestStep = value;
            } }


        public StationStatus StationStatus { get; set; }

        public Station(StationType stationType)
        {
            this.StationType = stationType;
            this.StationStatus = new StationStatus(this.StationType);
            this.TestStep = TestStep.Idle;
            this.TestPosition = (Position)((int)Position.StationBase + (int)stationType);
            this.QRPosition = (Position)((int)Position.StationQRBase + (int)stationType);
            this.LinkedModule = null;
            
        }


        public void Clear()
        {
            this.LinkedModule = null;
            this.TestStep = TestStep.Idle;
        }


    }
}
