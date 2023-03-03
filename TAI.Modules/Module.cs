using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAI.Modules
{
    public enum ModuleType
    {
        NONE = 0,
        AI = 1,     //模拟量输入模块（AI）	        nAI160，16通道
        AO = 2,     //模拟量输出模块（AO）	        nAO080，8通道
        DI = 3,     //数字量输入模块（DI）	        pDI240，24通道
        DO = 4,     //数字量输出模块（DO）	        pDO160，16通道
        PI = 5,     //脉冲量输入模块（PI）	        pPI080，8通道，24V/48V 、5V两种类型
        RTD_3L = 6,  //热电阻温度采集模块（RTD）	nTD160_3Wire，16通道三线制
        RTD_4L = 7,  //热电阻温度采集模块（RTD）	nTD120_4Wire，12通道四线制
        TC = 8       //热电偶温度采集模块（TC）	                    8路通道
    }


    public enum TestStep
    {
        Idle = 0,
        Wait = 1,
        Testing = 2,
        Finish = 3,
    }




    public class Module
    {
        public string SerialCode { get; set; }
        public ModuleType ModuleType { get; set; }
        public TestStep TestStep { get; set; }
        public int ChnanelCount { get; set; }

        



    }
}
