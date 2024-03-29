﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DMT.Core.Utils;
using System.IO.Ports;

namespace DMT.Core.Channels
{
    public class SerialChannel : BaseChannel
    {
        public const int READ_TIMEOUT_MAX = 3000;

        private Thread ReceiveProcessor;
        private Boolean Terminated { get; set; }

        public SerialPort SerialPort;

        public string Port
        {
            get
            {
                return this.SerialPort.PortName;
            }
            set
            {
                this.SerialPort.PortName = value;
            }
        }

        public int BaudRate
        {
            get
            {
                return this.SerialPort.BaudRate;
            }
            set
            {
                this.SerialPort.BaudRate = value;
            }
        }

        public int ReadTimeout
        {
            get
            {
                return this.SerialPort.ReadTimeout;
            }
            set
            {
                this.SerialPort.ReadTimeout = value;
            }
        }

        public void Initialize()
        {
            this.SerialPort = new SerialPort();
            
            this.SerialPort.ReadTimeout = 1000;
            // 
            

            //          
        }

        public override Boolean Open()
        {
            Boolean result = false;
            String Name = "串口[" + this.SerialPort.PortName + "]";
            try
            {

                this.SerialPort.Open();
                result = this.SerialPort.IsOpen;
                this.LastMessage = Name + "打开成功！";
                this.LastErrorCode = ChannelResult.OK;
                this.Notify(CHANNEL_EVENT, ChannelControl.Open.ToString(), "", ChannelResult.OK, this.LastMessage);
                return result;
            }
            catch (System.UnauthorizedAccessException)
            {
                this.LastMessage = Name + "访问拒绝，请确定是否被占用！";
                result = false;
            }
            catch (System.InvalidOperationException)
            {
                this.LastMessage = Name + "打开失败，请确定是否被占用！";
                result = false;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                this.LastMessage = Name + "打开失败，部分参数无效！";
                result = false;
            }

            catch (System.IO.IOException)
            {
                this.LastMessage = Name + "打开失败，端口无效！";
                result = false;
            }

            this.LastErrorCode = ChannelResult.CanNotOpen;
            this.Notify(CHANNEL_EVENT, ChannelControl.Open.ToString(), "", ChannelResult.CanNotOpen, this.LastMessage);
            return result;
        }

        public override Boolean Close()
        {
            String Name = "串口[" + this.SerialPort.PortName + "]";

            if (this.SerialPort.IsOpen)
            {
                try
                {
                    this.SerialPort.Close();
                    this.LastMessage = Name + "关闭成功！";
                    this.LastErrorCode = ChannelResult.OK;
                    this.Notify(CHANNEL_EVENT, ChannelControl.Close.ToString(), "", ChannelResult.OK, this.LastMessage);
                }
                catch (System.IO.IOException)
                {
                    this.LastMessage = Name + "无效，关闭失败！";
                    this.LastErrorCode = ChannelResult.CanNotClose;
                    this.Notify(CHANNEL_EVENT, ChannelControl.Close.ToString(), "", ChannelResult.CanNotClose, this.LastMessage);
                    return false;
                }

            }
            return !this.SerialPort.IsOpen;
        }

        public override Boolean Active()
        {
            return this.SerialPort.IsOpen;
        }


        public override bool SendCommand(byte[] command)
        {
            if (!this.SerialPort.IsOpen)
            {
                this.Open();
            }
            if ((this.SerialPort.IsOpen) && (command.Length > 0))
            {
                this.SerialPort.Write(command,0,command.Length);
                this.LastMessage = "发送数据成功";
                this.LastErrorCode = ChannelResult.OK;
                this.Notify(CHANNEL_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.OK, this.LastMessage);
                return true;
            }
            else
            {
                this.LastMessage = "发送数据失败";
                this.LastErrorCode = ChannelResult.SendError;
                this.Notify(CHANNEL_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.SendError, this.LastMessage);
                return false;
            }
        }



        public override Boolean SendCommand(String command)
        {
            if (!this.SerialPort.IsOpen)
            {
                this.Open();
            }
            if ((this.SerialPort.IsOpen) && (command.Length > 0))
            {
                this.SerialPort.Write(command);
                this.LastMessage = "发送数据成功";
                this.LastErrorCode = ChannelResult.OK;
                this.Notify(CHANNEL_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.OK, this.LastMessage);
                return true;
            }
            else
            {
                this.LastMessage = "发送数据失败";
                this.LastErrorCode = ChannelResult.SendError;
                this.Notify(CHANNEL_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.SendError, this.LastMessage);
                return false;
            }
        }


        public override string Receive()
        {
            ChannelResult resResult = ChannelResult.OK;
            string ReceiveString = "";
            if (!this.SerialPort.IsOpen)
            {
                this.Open();
            }

            try
            {
                ReceiveString = this.SerialPort.ReadLine();
                resResult = ChannelResult.OK;
                this.LastMessage = "读取数据";
                this.LastErrorCode = ChannelResult.OK;
                this.Notify(CHANNEL_EVENT, ChannelControl.Receive.ToString(), ReceiveString, resResult, this.LastMessage);
                return ReceiveString;
            }
            catch (System.TimeoutException)
            {
                resResult = ChannelResult.ReceiveError;
                this.LastMessage = "接收数据超时";
                this.LastErrorCode = ChannelResult.ReceiveTimeOut;
                this.Notify(CHANNEL_EVENT, ChannelControl.Receive.ToString(), ReceiveString, resResult, this.LastMessage);
                return "";
            }
            catch (System.Exception)
            {
                resResult = ChannelResult.ReceiveError;
                this.LastMessage = "接收数据错误";
                this.LastErrorCode = ChannelResult.ReceiveError;
                this.Notify(CHANNEL_EVENT, ChannelControl.Receive.ToString(), ReceiveString, resResult, this.LastMessage);
                return "";
            }
           
        }
        




        public SerialChannel(String Caption)
        {
            this.Caption = string.Format( "{0}SerialPort",Caption);
            this.Initialize();
        }

        public override void LoadFromFile(string fileName)
        {
            try
            {
                this.SerialPort.PortName = IniFiles.GetStringValue(fileName, this.Caption, "PortName", "COM4");
                this.SerialPort.BaudRate = IniFiles.GetIntValue(fileName, this.Caption, "BaudRate", 4800);
                this.SerialPort.Parity = (Parity)IniFiles.GetIntValue(fileName, this.Caption, "Parity", (int)Parity.None);
                this.SerialPort.DataBits = IniFiles.GetIntValue(fileName, this.Caption, "DataBits", 8);
                this.SerialPort.StopBits = (StopBits)IniFiles.GetIntValue(fileName, this.Caption, "StopBits", (int)StopBits.One);
                this.SerialPort.NewLine = IniFiles.GetStringValue(fileName, this.Caption, "NewlineText", "\r");
                this.SerialPort.ReadTimeout = IniFiles.GetIntValue(fileName, this.Caption, "ReadTimeout", 1000);

                if (!System.IO.File.Exists(fileName))
                {
                    this.SaveToFile(fileName);
                }
                return;
            }
            catch (System.IO.IOException)
            {
                this.LastMessage = "初始化访问无效！";
                this.LastErrorCode = ChannelResult.InvalidParam;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                this.LastMessage = "初始化串口参数出错！";
                this.LastErrorCode = ChannelResult.InvalidParam;
            }
            this.Notify(CHANNEL_EVENT, ChannelControl.Init.ToString(), "",ChannelResult.InvalidParam, this.LastMessage);

        }

        public override void SaveToFile(string fileName)
        {
            IniFiles.WriteStringValue(fileName, this.Caption, "PortName", this.SerialPort.PortName);
            IniFiles.WriteIntValue(fileName, this.Caption, "BaudRate", this.SerialPort.BaudRate);
            IniFiles.WriteIntValue(fileName, this.Caption, "Parity", (int)this.SerialPort.Parity);
            IniFiles.WriteIntValue(fileName, this.Caption, "DataBits", this.SerialPort.DataBits);
            IniFiles.WriteIntValue(fileName, this.Caption, "StopBits", (int)this.SerialPort.StopBits);
            IniFiles.WriteStringValue(fileName, this.Caption, "NewlineText", this.SerialPort.NewLine);
            IniFiles.WriteIntValue(fileName, this.Caption, "ReadTimeout", this.SerialPort.ReadTimeout);
        }



        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
          try
            {               
                string value = this.SerialPort.ReadExisting();
                this.LastMessage = "";
                this.LastErrorCode = ChannelResult.OK;
                this.Notify(CHANNEL_EVENT, ChannelControl.Receive.ToString(), value, ChannelResult.OK, this.LastMessage);
           }
            catch
            {

            }

        }



        private void PorcessReceiveData()
        {
            while (!this.Terminated)
            {
                try
                {
                    Thread.Sleep(1);
                    ChannelResult resResult = ChannelResult.OK;
                    string ReceiveString = "";
                    if (this.Active())
                    {
                        try
                        {
                            ReceiveString = this.SerialPort.ReadExisting();          
                            resResult = ChannelResult.OK;
                            this.LastMessage = "读取数据";
                            this.LastErrorCode = ChannelResult.OK;
                            this.Notify(CHANNEL_EVENT, ChannelControl.Receive.ToString(), ReceiveString, resResult, this.LastMessage);
                        }
                        catch (System.Exception)
                        {
                            resResult = ChannelResult.Fail;
                            this.LastMessage = "接收数据错误";
                            this.LastErrorCode = ChannelResult.ReceiveError;
                            this.Notify(CHANNEL_EVENT, ChannelControl.Receive.ToString(), ReceiveString, resResult, this.LastMessage);
                        }
                    }
                }
                catch (System.Threading.ThreadInterruptedException)
                {
                }
            }
        }

        public override void StartReceiveData()
        {
            this.Terminated = false;
            this.SerialPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialPortDataReceived);
            this.ReceiveProcessor = new Thread(new ThreadStart(this.PorcessReceiveData));
            this.ReceiveProcessor.IsBackground = true;
            this.ReceiveProcessor.Start();
        }

        public override void StopReceiveData()
        {
            this.Terminated = true;
            this.SerialPort.DataReceived -= new SerialDataReceivedEventHandler(this.SerialPortDataReceived);
            this.ReceiveProcessor.Interrupt();
        }

    }
}
