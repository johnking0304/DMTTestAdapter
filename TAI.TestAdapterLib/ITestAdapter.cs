using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using TAI.Device;


namespace DMTTestAdapter
{

    public interface ITestAdapter
    {
        /// <summary>
        /// 根据本地配置载入设备配置
        /// </summary>
        /// <returns></returns>

        string Initialize();

        /// <summary>
        /// 获取当前系统状态
        /// </summary>
        /// <returns></returns>

        string GetSystemStatus();

        /// <summary>
        /// 启动试验进程
        /// </summary>
        /// <returns></returns>

        string StartTest();

        /// <summary>
        /// 停止试验进程，并复位
        /// </summary>
        /// <returns></returns>

        string StopTest();


        /// <summary>
        /// 设置模拟量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="type"></param>  
        ////0 电流模式
        ////1 电压模式
        ////2 电阻模式
        /// <param name="value"></param>
        /// <returns></returns>

        string SetAnalogueChannelValue(int StationId,int channelId, int type, float value);

        /// <summary>
        /// 获取模拟量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [DispId(7)]
        string GetAnalogueChannelValue(int StationId,int channelId, int type);

        /// <summary>
        /// 设置数字量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="value"></param>
        /// <returns></returns>

        string SetDigitalChannelValue(int StationId,int channelId, bool value);
        /// <summary>
        /// 获取数字量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>

        string GetDigitalChannelValue(int StationId,int channelId);

        /// <summary>
        /// 获取最后的错误码
        /// </summary>
        /// <returns></returns>

        string GetLastErrorCode();

        /// <summary>
        /// 获取视觉 模块类型
        /// </summary>
        /// <returns></returns>

        string GetVISModuleType(int StationId);

        /// <summary>
        /// 获取视觉OCR 识别 模块码
        /// </summary>
        /// <returns></returns>

        string GetVISModuleCode(int StationId);

        /// <summary>
        /// 获取视觉灯测结果
        /// </summary>
        /// <returns></returns>

        string GetVISLightingResult(int StationId);

        /// <summary>
        /// 启动工位测试
        /// </summary>
        /// <param name="StationId"></param> 
        /// <returns></returns>

        string StartStationTest(int StationId);


        /// <summary>
        /// 设置测试工位测试结果
        /// </summary>
        /// <param name="StationId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        string SetTestResult(int StationId ,bool result);

        /// <summary>
        /// 请求灯测服务
        /// </summary>
        /// <param name="StationId"></param>
        /// <returns></returns>
        string RequestVISLighting(int StationId);
        /// <summary>
        /// 释放灯测服务
        /// </summary>
        /// <param name="StationId"></param>
        /// <returns></returns>
        string ReleaseVISLighting(int StationId);






    }
}
