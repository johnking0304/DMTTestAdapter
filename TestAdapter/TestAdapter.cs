using AnalogDevice;
using System;

namespace DMTTestAdapter
{

    /// <summary>
    /// 测试接口
    /// </summary>
    public class TestAdapter : ITestAdapter
    {
        public double GetAnalogueChannelValue(int channelId, ChannelType type)
        {
            throw new NotImplementedException();
        }

        public bool GetDigitalChannelValue(int channelId)
        {
            throw new NotImplementedException();
        }

        public int GetLastErrorCode()
        {
            throw new NotImplementedException();
        }

        public string GetSystemStatus()
        {
            throw new NotImplementedException();
        }

        public string GetVISResult()
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public bool Initialize(string content)
        {
            throw new NotImplementedException();
        }

        public bool SetAnalogueChannelValue(int channelId, ChannelType type, double value)
        {
            throw new NotImplementedException();
        }

        public bool SetDigitalChannelValue(int channelId, bool value)
        {
            throw new NotImplementedException();
        }

        public void SetNotifyCallback(Notify notify)
        {
            throw new NotImplementedException();
        }

        public bool StartTest()
        {
            throw new NotImplementedException();
        }

        public bool StopTest()
        {
            throw new NotImplementedException();
        }
    }
}
