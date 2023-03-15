using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAI.Modules
{

    public enum StationType
    {
        DI = 1,     //数字量输入模块（DI）	        pDI240，24通道
        DO = 2,     //数字量输出模块（DO）	        pDO160，16通道
        PI = 3,     //脉冲量输入模块（PI）	        pPI080，8通道，24V/48V 、5V两种类型
        AI = 4,     //模拟量输入模块（AI）	        nAI160，16通道
        AO = 5,     //模拟量输出模块（AO）	        nAO080，8通道       
        RTD_3L = 6,  //热电阻温度采集模块（RTD）	nTD160_3Wire，16通道三线制
        RTD_4L = 7,  //热电阻温度采集模块（RTD）	nTD120_4Wire，12通道四线制
        TC = 8,    //热电偶温度采集模块（TC）	                    8路通道 
        Prepare = 9,  // 预热
    }


    public class Station
    {
        public Position TestPosition { get; set; }

        public Position QRPosition { get; set; }

        
        public StationType StationType { get; set; }
        public Module LinkedModule { get; set; }
        public TestStep TestStep { get; set; }

        public Station(StationType stationType)
        {
            this.StationType = stationType;
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
