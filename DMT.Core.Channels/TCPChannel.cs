using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.IO;
using DMT.Core.Utils;





namespace DMT.Core.Channels
{
    public class TCPClientChannel:BaseChannel
    {
        public const int RECEIVE_BUFFER_SIZE = 10240;

        private TcpClient TCPClient {get;set;}
        
        public int Port { get; set; }
        public string Host { get; set; }
        private int ReadTimeout { get; set; }
        private DateTime StartConnectDatetime { get;set; }
        private int TryConnectInterval { get; set; }

        private Thread ReceiveProcessor;
        private Boolean Terminated { get; set; }
        public Boolean Connecting { get; set; }
        public bool Connected { get; set; }


        private void Initialize()
        {           
            this.ReadTimeout = 2000;
            this.Connected = false;
        }
        public TCPClientChannel()
        {
            this.Caption = "TCPClient";
            this.Initialize();
        }

        public void ReConnect()
        {
            if (this.ConnectTimeout())
            {
                this.Close();
                this.Open();
            }
            
        }


        public TCPClientChannel(String Caption)
        {
            this.Caption = Caption + "TCPClient";
            this.Initialize();
                     
        }


        private void ConnectCallback(IAsyncResult result)
        {
 
            TcpClient client = (TcpClient)result.AsyncState;
            try
            {
                this.Connecting = false;
                this.Connected = false;

                if ((client != null) && (client.Connected))
                {
                    this.Connected = true;
                    client.EndConnect(result);
                    this.LastMessage = "连接成功!";
                    this.LastErrorCode = ChannelResult.OK;
                    this.Notify(CHANNEL_EVENT, ChannelControl.Open.ToString(), "", ChannelResult.OK, this.LastMessage);
                    return;
                }
                else
                {
                    client.EndConnect(result);
                    this.LastMessage = "连接失败!";
                    this.LastErrorCode = ChannelResult.CanNotOpen;
                }
            }
            catch (System.NullReferenceException)
            {
                this.LastMessage = "无效的连接！";
                this.LastErrorCode = ChannelResult.CanNotOpen;
            }
            catch (System.ObjectDisposedException)
            {
                this.LastMessage = "连接关闭！";
                this.LastErrorCode = ChannelResult.CanNotOpen;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                this.LastMessage = "无效端口 [" + this.Port.ToString() + "]！";
                this.LastErrorCode = ChannelResult.CanNotOpen;
            }
            catch (System.Net.Sockets.SocketException)
            {
                this.LastMessage = "连接错误！";
                this.LastErrorCode = ChannelResult.CanNotOpen;
            }
            this.Notify(CHANNEL_EVENT, ChannelControl.Open.ToString(),"", ChannelResult.Fail, this.LastMessage); 
        }
        public override void OpenSync()
        {
            if (!this.Connecting)
            {
                try
                {
                    this.StartConnectDatetime = DateTime.Now;
                    if(this.TCPClient != null)
                    {
                        this.Close();
                        
                    }

                    this.TCPClient = new TcpClient();
                    this.TCPClient.BeginConnect(this.Host, this.Port, new AsyncCallback(ConnectCallback), this.TCPClient);
                    this.LastMessage = "开始连接！";
                    this.Notify(CHANNEL_EVENT, ChannelControl.Open.ToString(),"",ChannelResult.Operating, this.LastMessage);
                    this.Connecting = true;
                }
                catch (System.InvalidOperationException)
                {
                    this.LastErrorCode = ChannelResult.CanNotOpen;
                }
            }   
        }

        public override Boolean Open()
        {
            try
            {
                if (this.TCPClient != null)
                {
                    this.Close();
                }
                this.TCPClient = new TcpClient();
                this.TCPClient.Connect(this.Host, this.Port);
                this.LastMessage = "连接成功！";
                this.LastErrorCode = ChannelResult.OK;
                this.Notify(CHANNEL_EVENT, ChannelControl.Open.ToString(),"", ChannelResult.OK, this.LastMessage);
                return true;
            }
            catch (System.ObjectDisposedException)
            {
                this.LastMessage = "客户端关闭！";
                this.LastErrorCode = ChannelResult.CanNotOpen;
                this.TCPClient = new TcpClient();
            }
            catch (System.ArgumentOutOfRangeException)
            {
                this.LastMessage = "无效端口[" + this.Port.ToString() + "]！";
                this.LastErrorCode = ChannelResult.CanNotOpen;
            }
            catch (System.Net.Sockets.SocketException)
            {
                this.LastMessage = "连接失败！";
                this.LastErrorCode = ChannelResult.CanNotOpen;
            }
            this.Notify(CHANNEL_EVENT, ChannelControl.Open.ToString(),"", ChannelResult.CanNotOpen, this.LastMessage);
            return false;
           
        }
        public override Boolean Close()
        {
            try
            {
                this.TCPClient.Close();
                this.Connecting = false;
                this.LastMessage = "客户端关闭";
                this.Notify(CHANNEL_EVENT, ChannelControl.Close.ToString(),"", ChannelResult.OK, this.LastMessage);
                this.LastErrorCode = ChannelResult.OK;
            }
            catch(System.Net.Sockets.SocketException)
            {
                this.LastMessage = "套接字错误！";
                this.LastErrorCode = ChannelResult.CanNotClose;
                this.Notify(CHANNEL_EVENT, ChannelControl.Close.ToString(),"", ChannelResult.CanNotClose, this.LastMessage);
                return false;
            }
            this.Connected = false;
            return true;
        }
        public override Boolean Active()
        {
            return this.Connected;

        }

        public Boolean ConnectTimeout()
        {
            TimeSpan span = DateTime.Now - this.StartConnectDatetime;
            if (span.TotalSeconds > this.TryConnectInterval)
            {
                this.StartConnectDatetime = DateTime.Now;
                return true;
            }
            return false;
        }
        //异步方法
        public  Task<string> SendAsync(string Command, int TimeOut)
        {
            return Task.Run<string>(() =>
            {
                return SendSync(Command, TimeOut);
            });
        }

        public Task<string> SendAsync(byte[] command, int TimeOut)
        {
            return Task.Run<string>(() =>
            {
                return SendSync(command, TimeOut);
            });
        }

        public override Boolean SendCommandNoReply(string command)
        {
            byte[] dataGram = Encoding.ASCII.GetBytes(command);
            return this.SendCommandNoReply(dataGram);          
        }


        public override Boolean SendCommandNoReply(byte[] command)
        {
            Boolean result = false;
            if (!this.Active())
            {
               this.OpenSync();
            }
            
            try
            {
                var dataGram = command;
                NetworkStream ClientStream = this.TCPClient.GetStream();
                ClientStream.Write(dataGram, 0, dataGram.Length);
                this.LastMessage = "发送数据成功";
                this.LastErrorCode = ChannelResult.OK;
                this.Notify(CHANNEL_EVENT, ChannelControl.Send.ToString(),"", ChannelResult.OK, this.LastMessage);
                result = true;
            }
            catch (System.Exception)
            {
                result = false;
                this.LastMessage = "发送数据失败!";
                this.LastErrorCode = ChannelResult.SendError;
            }
            return result;
        }

        public override void StartReceiveData()
        {
            this.Terminated = false;
            this.ReceiveProcessor = new Thread(new ThreadStart(this.ReceiveData));
            this.ReceiveProcessor.IsBackground = true;
            this.ReceiveProcessor.Start();
        }

        public override void StopReceiveData()
        {
            this.Terminated = true;
            this.ReceiveProcessor.Interrupt();
        }

        public void  ReceiveData()
        {
            while (!this.Terminated)
            {
                try
                {
                    Thread.Sleep(1);
                    ChannelResult resResult = ChannelResult.OK;
                    string ReceiveString = "";
                    byte[] ReceiveBytes = new byte[RECEIVE_BUFFER_SIZE];
                    if (this.Active())
                    {
                        try
                        {                          
                            NetworkStream ClientStream = this.TCPClient.GetStream();
                            int numberOfReadBytes = ClientStream.Read(ReceiveBytes, 0, RECEIVE_BUFFER_SIZE);
                            byte[] realReceiveBytes = new byte[numberOfReadBytes];
                            Array.Copy(ReceiveBytes, 0, realReceiveBytes, 0, numberOfReadBytes);
                            ReceiveString = System.Text.Encoding.Default.GetString(realReceiveBytes);
                            resResult = ChannelResult.OK;
                            this.LastMessage = "收到数据(上报)";
                            this.LastErrorCode = ChannelResult.OK;
                            this.Notify(CHANNEL_EVENT, ChannelControl.Report.ToString(), ReceiveString, resResult, this.LastMessage);

                        }
                        catch (System.Exception)
                        {
                            resResult = ChannelResult.Fail;
                            this.LastMessage = "接收数据错误";
                            this.LastErrorCode = ChannelResult.ReceiveError;
                            this.Notify(CHANNEL_EVENT, ChannelControl.Report.ToString(), ReceiveString, resResult, this.LastMessage);
                        }
                    }                  
                }
                catch (System.Threading.ThreadInterruptedException)
                { 
                }
            }
        }

        public override string Receive()
        {


            ResponseResult resResult = ResponseResult.Unknown;
            string ReceiveString = "";

            byte[] ReceiveBytes = new byte[RECEIVE_BUFFER_SIZE];

            NetworkStream ClientStream = this.TCPClient.GetStream();
            var asyncResult = ClientStream.BeginRead(ReceiveBytes, 0, RECEIVE_BUFFER_SIZE, null, null);
            asyncResult.AsyncWaitHandle.WaitOne(this.ReadTimeout);
            if (asyncResult.IsCompleted)
            {
                try
                {

                    int numberOfReadBytes = ClientStream.EndRead(asyncResult);
                    byte[] realReceiveBytes = new byte[numberOfReadBytes];
                    Array.Copy(ReceiveBytes, 0, realReceiveBytes, 0, numberOfReadBytes);
                    ReceiveString = System.Text.Encoding.Default.GetString(realReceiveBytes);
                    resResult = ResponseResult.Ok;
                    this.LastErrorCode = ChannelResult.OK;
                    this.LastMessage = "接收返回数据成功!";
                    return  ReceiveString;

                }
                catch (System.Exception e)
                {
                    resResult = ResponseResult.Error;
                    this.LastErrorCode = ChannelResult.ReceiveError;
                    this.LastMessage = string.Format("接收返回数据失败:[{0}]!", e.ToString());
                }
            }
            else
            {

                resResult = ResponseResult.TimeOut;
                this.LastMessage = "接收返回数据超时";
                this.LastErrorCode = ChannelResult.ReceiveTimeOut;

            }

            return "";

            

        }
        //同步方法
        public string SendSync(byte[] command, int TimeOut)
        {
            if (!this.Active())
            {
                this.OpenSync();
            }

            ResponseResult resResult = ResponseResult.Unknown;
            string ReceiveString = "";


            byte[] ReceiveBytes = new byte[RECEIVE_BUFFER_SIZE];
            if (this.Active())
            {
                try
                {
                    var dataGram = command;
                    
                    NetworkStream ClientStream = this.TCPClient.GetStream();
                    ClientStream.Write(dataGram, 0, dataGram.Length);                   
                    var asyncResult = ClientStream.BeginRead(ReceiveBytes, 0, RECEIVE_BUFFER_SIZE, null, null);
                    asyncResult.AsyncWaitHandle.WaitOne(this.ReadTimeout);
                    if (asyncResult.IsCompleted)
                    {
                        try
                        {
 
                            int numberOfReadBytes = ClientStream.EndRead(asyncResult);
                            byte[] realReceiveBytes = new byte[numberOfReadBytes];
                            Array.Copy(ReceiveBytes, 0, realReceiveBytes, 0, numberOfReadBytes);
                            ReceiveString = System.Text.Encoding.Default.GetString(realReceiveBytes);
                            resResult = ResponseResult.Ok;
                            this.LastErrorCode = ChannelResult.OK;
                            this.LastMessage = "接收返回数据成功!";
                            return ReceiveString;
                            
                        }
                        catch (System.Exception e)
                        {
                            resResult = ResponseResult.Error;
                            this.LastErrorCode = ChannelResult.ReceiveError;
                            this.LastMessage = string.Format("接收返回数据失败:[{0}]!",e.ToString());
                        }
                    }
                    else
                    {

                        resResult = ResponseResult.TimeOut;
                        this.LastMessage = "接收返回数据超时";
                        this.LastErrorCode = ChannelResult.ReceiveTimeOut;

                    }
                }
                catch (System.Exception)
                {
                    resResult = ResponseResult.Error;
                    this.LastErrorCode = ChannelResult.SendError;
                    this.LastMessage = "发送数据错误";
                }

            }
            else
            {
                resResult = ResponseResult.Error;
                this.LastErrorCode = ChannelResult.SendError;
            }
            this.Notify(CHANNEL_EVENT, ChannelControl.Receive.ToString(), ReceiveString, resResult, this.LastMessage);
            return "";
        }


        //同步方法
        public  string SendSync(string Command, int TimeOut)
        {

            byte[] dataGram = Encoding.ASCII.GetBytes(Command);
            return this.SendSync(dataGram, TimeOut);
        }

        public string SendCommandSync(string command)
        {
            return this.SendSync(command, this.ReadTimeout);  
            
        }

        public override Boolean SendCommand(string command)
        { 
            this.SendCommandAsync(command);
            return true;
        }

        public override Boolean SendCommand(byte[] command)
        {
            this.SendCommandAsync(command);
            return true;
        }

        public async void SendCommandAsync(string command)
        {
            await this.SendAsync(command, this.ReadTimeout);  
        }

        public async void SendCommandAsync(byte[] command)
        {
            await this.SendAsync(command, this.ReadTimeout);
        }

        public override void SaveToFile(string fileName)
        {
            IniFiles.WriteStringValue(fileName, this.Caption, "Host", this.Host);
            IniFiles.WriteIntValue(fileName, this.Caption, "Port", this.Port);
            IniFiles.WriteIntValue(fileName, this.Caption, "ReadTimeout", this.ReadTimeout);
            IniFiles.WriteIntValue(fileName, this.Caption, "TryConnectInterval", this.TryConnectInterval);

        }

        public override void LoadFromFile(string fileName)
        {
            this.Host =         IniFiles.GetStringValue(fileName, this.Caption, "Host", "127.0.0.1");
            this.Port =         IniFiles.GetIntValue(fileName, this.Caption, "Port", 8000);
            this.ReadTimeout =  IniFiles.GetIntValue(fileName, this.Caption, "ReadTimeout", 2000);
            this.TryConnectInterval = IniFiles.GetIntValue(fileName, this.Caption, "TryConnectInterval",10 );
            base.LoadFromFile(fileName);
        }







    }
}
