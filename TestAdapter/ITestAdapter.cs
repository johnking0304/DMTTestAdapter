﻿using System;
using System.Collections.Generic;
using System.Text;
using AnalogDevice;

namespace DMTTestAdapter
{

    public delegate void Notify(int Event, int code, string message);
    public interface ITestAdapter
    {
        /// <summary>
        /// 根据本地配置载入设备配置
        /// </summary>
        /// <returns></returns>
        bool Initialize();
        /// <summary>
        /// 根据配置内容，解析后刷新设备配置
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        bool Initialize(string content);

        /// <summary>
        /// 获取当前系统状态
        /// </summary>
        /// <returns></returns>
        string GetSystemStatus();

        /// <summary>
        /// 启动试验进程
        /// </summary>
        /// <returns></returns>
        bool StartTest();

        /// <summary>
        /// 停止试验进程，并复位
        /// </summary>
        /// <returns></returns>
        bool StopTest();


        /// <summary>
        /// 设置模拟量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetAnalogueChannelValue(int channelId, ChannelType type, double value);

        /// <summary>
        /// 获取模拟量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        double GetAnalogueChannelValue(int channelId, ChannelType type);

        /// <summary>
        /// 设置数字量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetDigitalChannelValue(int channelId, bool value);
        /// <summary>
        /// 获取数字量通道数据
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        bool GetDigitalChannelValue(int channelId);

        /// <summary>
        /// 获取最后的错误码
        /// </summary>
        /// <returns></returns>
        int GetLastErrorCode();

        /// <summary>
        /// 获取视觉拍摄解析结果（OCR/QR）
        /// </summary>
        /// <returns></returns>
        string GetVISResult();


        /// <summary>
        /// 注册通知回调函数
        /// </summary>
        /// <param name="notify"></param>
        void SetNotifyCallback(Notify notify);

    }
}
