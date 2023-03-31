using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMTTestAdapter
{

    public enum CommandName
    {

        Initialize = 100,

        /// <summary>
        /// 获取当前系统状态
        /// </summary>

        GetSystemStatus = 101,

        /// <summary>
        /// 启动试验进程
        /// </summary>
        StartTest = 102,

        /// <summary>
        /// 停止试验进程，并复位
        /// </summary>
        StopTest = 103,

        /// <summary>
        /// 设置模拟量通道数据
        /// </summary>

        SetAnalogueChannelValue = 104,

        /// <summary>
        /// 获取模拟量通道数据
        /// </summary>
        GetAnalogueChannelValue = 105,

        /// <summary>
        /// 设置数字量通道数据
        /// </summary>

        SetDigitalChannelValue = 106,
        /// <summary>
        /// 获取数字量通道数据
        /// </summary>

        GetDigitalChannelValue = 107,

        /// <summary>
        /// 获取最后的错误码
        /// </summary>

        GetLastErrorCode = 108,

        /// <summary>
        /// 获取视觉 模块类型
        /// </summary>

        GetVISModuleType = 109,

        /// <summary>
        /// 获取视觉OCR 识别 模块码
        /// </summary>

        GetVISModuleCode = 110,

        /// <summary>
        /// 获取视觉灯测结果
        /// </summary>
		GetVISLightingResult = 111,

        /// <summary>
        /// 启动工位测试
        /// </summary>
		StartStationTest = 112,

        /// <summary>
        /// 设置测试工位测试结果
        /// </summary>

        SetTestResult = 113,

        RequestVISLighting =114,

        ReleaseVISLighting =115,

        /// <summary>
        /// 事件上报
        /// </summary>
        NotifyEvent = 120,
        /// <summary>
        ///状态上报
        /// </summary>
        NotifyStatus = 121,

    }




    public enum SystemCode
    {
        Ok = 0,
        Fault = 1,

    }


    public enum OperateCommand
    { 
        None =0,
        Initialize =1,
        StartTest =2,
        StopTest =3,
        StartStationTest =4,
        StopStationTest =5,
        EnableStationTest =6,
    }





}
