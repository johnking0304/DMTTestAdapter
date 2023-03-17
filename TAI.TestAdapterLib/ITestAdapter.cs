using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using TAI.Device;


namespace DMTTestAdapter
{
    [Guid("A24ECE6B-202D-48C0-B054-A52514FEE069")]
    public interface ITestAdapter
    {
        /// <summary>
        /// 根据本地配置载入设备配置
        /// </summary>
        /// <returns></returns>
        [DispId(1)]
        string Initialize();

        /// <summary>
        /// 获取当前系统状态
        /// </summary>
        /// <returns></returns>
        [DispId(3)]
        string GetSystemStatus();

        /// <summary>
        /// 启动试验进程
        /// </summary>
        /// <returns></returns>
        [DispId(4)]
        void StartTest();

        /// <summary>
        /// 停止试验进程，并复位
        /// </summary>
        /// <returns></returns>
        [DispId(5)]
        void StopTest();


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
        [DispId(6)]
        bool SetAnalogueChannelValue(int channelId, int type, float value);

        /// <summary>
        /// 获取模拟量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [DispId(7)]
        float GetAnalogueChannelValue(int channelId, int type);

        /// <summary>
        /// 设置数字量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [DispId(8)]
        bool SetDigitalChannelValue(int channelId, bool value);
        /// <summary>
        /// 获取数字量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        [DispId(9)]
        bool GetDigitalChannelValue(int channelId);

        /// <summary>
        /// 获取最后的错误码
        /// </summary>
        /// <returns></returns>
        [DispId(10)]
        int GetLastErrorCode();

        /// <summary>
        /// 获取视觉 模块类型
        /// </summary>
        /// <returns></returns>
        [DispId(11)]
        string GetVISModuleType(int StationId);

        /// <summary>
        /// 获取视觉OCR 识别 模块码
        /// </summary>
        /// <returns></returns>
        [DispId(12)]
        string GetVISModuleCode(int StationId);

        /// <summary>
        /// 获取视觉灯测结果
        /// </summary>
        /// <returns></returns>
        [DispId(13)]
        string GetVISLightingResult(int StationId);

        /// <summary>
        /// 启动工位测试
        /// </summary>
        /// <param name="StationId"></param> 
        /// <returns></returns>
        [DispId(14)]
        bool StartStationTest(int StationId);


        /// <summary>
        /// 设置测试工位测试结果
        /// </summary>
        /// <param name="StationId"></param>
        /// <param name="result"></param>
        /// <returns></returns>

        [DispId(15)]
        bool SetTestResult(int StationId ,bool result);




    }
}
