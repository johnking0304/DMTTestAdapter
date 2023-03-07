using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using DMT.Core.Utils;





namespace DMT.Core.Channels
{
    public class UDPClientChannel:BaseChannel
    {
        public const int RECEIVE_BUFFER_SIZE = 4096;

        public const int UDP_CONTROL_EVENT = 3000;
        public const int UDP_COMMAND_EVENT = 3001;
		public const int UDP_DATA_EVENT = 3002;

        private UdpClient UDPClient;

        public int Port { get; set; }
        public string Host { get; set; }
        public int ReadTimeout { get; set; }

        private Thread receiveProcessor;
        private Boolean terminated { get; set; }


        private void Initialize()
        {           
            this.ReadTimeout = 1000;          
        }
        public UDPClientChannel()
        {
            this.Caption = "UDPClient";
            this.Initialize();
        }



        public UDPClientChannel(String caption)
        {
            this.Caption = caption + "UDPClient";
            this.Initialize();
        }

        public Boolean OpenBoardCast()
        {
            try
            {
                this.UDPClient = new UdpClient(new IPEndPoint(IPAddress.Parse(this.Host), this.Port));
                this.UDPClient.EnableBroadcast = true;
                this.LastMessage = "UDP 广播启动！";
                this.LastErrorCode = ChannelResult.OK;
                this.Notify(UDP_CONTROL_EVENT, ChannelControl.Open.ToString(),"", ChannelResult.OK, this.LastMessage);

                return true;
            }
            catch (System.ObjectDisposedException)
            {
                this.LastMessage = "UDP 启动广播出错！";
                this.LastErrorCode = ChannelResult.CanNotClose;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                this.LastMessage = "端口[" + this.Port.ToString() + "]无效！";
                this.LastErrorCode = ChannelResult.CanNotClose;
            }
            catch (System.Net.Sockets.SocketException)
            {
                this.LastMessage = "网络访问出错！";
                this.LastErrorCode = ChannelResult.CanNotClose;
            }
            this.Notify(UDP_CONTROL_EVENT, ChannelControl.Open.ToString(), "",ChannelResult.CanNotOpen, this.LastMessage);
            return false;        
        }

        public override Boolean Open()
        {
            try
            {
                this.UDPClient = new UdpClient();
                this.UDPClient.Connect(this.Host, this.Port);
                this.LastMessage = "UDP Client 连接成功！";
                this.LastErrorCode = ChannelResult.OK;
                this.Notify(UDP_CONTROL_EVENT,ChannelControl.Open.ToString(),"", ChannelResult.OK, this.LastMessage);
                return true;
            }
            catch (System.ObjectDisposedException)
            {
                this.LastMessage = "UDP Client是关闭的！";
                this.LastErrorCode = ChannelResult.CanNotOpen;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                this.LastMessage = "端口[" + this.Port.ToString() + "]无效！";
                this.LastErrorCode = ChannelResult.CanNotOpen;
            }
            catch (System.Net.Sockets.SocketException)
            {
                this.LastMessage = "网络访问出错！";
                this.LastErrorCode = ChannelResult.CanNotOpen;
            }
            this.Notify(UDP_CONTROL_EVENT, ChannelControl.Open.ToString(), "", ChannelResult.CanNotOpen, this.LastMessage);
            return false;
           
        }
        public override Boolean Close()
        {
            try
            {
                this.UDPClient.Close();
                this.LastMessage = "UDPClient 关闭！";
                this.LastErrorCode = ChannelResult.OK;
                this.Notify(UDP_CONTROL_EVENT, ChannelControl.Close.ToString(), "", ChannelResult.OK, this.LastMessage);
            }
            catch(System.Net.Sockets.SocketException)
            {
                this.LastMessage = "网络异常！";
                this.LastErrorCode = ChannelResult.CanNotClose;
                this.Notify(UDP_CONTROL_EVENT, ChannelControl.Close.ToString(), "", ChannelResult.CanNotClose, this.LastMessage);
                return false;
            }
            return true;
        }
        public override Boolean Active()
        {
            if (this.UDPClient.Client != null)
            {
                return this.UDPClient.Client.Connected;
            }
            return false;
        
        }



        //异步方法
        public Task<Boolean> SendAsync(string Command, int TimeOut)
        {
            return Task.Run<Boolean>(() =>
            {
                return SendSync(Command, TimeOut);
            });
        }


        public Boolean BoardCast(string command)
        {
            Boolean result = false;
            try
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Broadcast, this.Port);  
                var dataGram = Encoding.Default.GetBytes(command);
                this.UDPClient.Send(dataGram, dataGram.Length, endpoint);
                this.Notify(UDP_DATA_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.OK, command);
                result = true;
            }
            catch (System.Exception  )
            {
                this.Notify(UDP_DATA_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.SendError, command);
                result = false;

            }
            return result;          
        
        }

        public Boolean SendASCIIData(string command)
        {
            Boolean result = false;
            try
            {
                var dataGram = Encoding.ASCII.GetBytes(command);
                this.UDPClient.Send(dataGram, dataGram.Length);
                this.Notify(UDP_DATA_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.OK, command);
                result = true;
            }
            catch (System.Exception)
            {
                this.Notify(UDP_DATA_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.SendError, command);
                result = false;

            }
            return result;  
            
           }
        public Boolean SendData(string command)
        {
            Boolean result = false;
            try
            {
                var dataGram = Encoding.Unicode.GetBytes(command);
                this.UDPClient.Send(dataGram, dataGram.Length);
                this.Notify(UDP_DATA_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.OK, command);
                result = true;
            }
            catch (System.Exception)
            {
                this.Notify(UDP_DATA_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.SendError, command);
                result = false;

            }
            return result;                 
        }
        public void StartAsyncReceiveData()
        {
            this.terminated = false;
            this.receiveProcessor = new Thread(new ThreadStart(this.ReceiveData));
            this.receiveProcessor.IsBackground = true;
            this.receiveProcessor.Start();
        }
        public override void StopReceiveData()
        {
            this.terminated = true;
            this.receiveProcessor.Interrupt();
        }
        public  void ReceiveData()
        {
            while (!this.terminated)
            {
                try
                {
                    Thread.Sleep(1);
                    ChannelResult resResult = ChannelResult.OK;
                    string ReceiveString = "";
                    byte[] ReceiveBytes = new byte[RECEIVE_BUFFER_SIZE];

                    try
                    {
                        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); 
                        byte[] ReceiveByte = this.UDPClient.Receive(ref remoteEP);
                        ReceiveString = Encoding.Default.GetString(ReceiveByte);
                        resResult = ChannelResult.OK;
                    }
                    catch (System.Exception)
                    {
                        resResult = ChannelResult.ReceiveError;
                    }
                    if (ReceiveString.Length > 0)
                    {
                        this.Notify(UDP_DATA_EVENT, ChannelControl.Receive.ToString(), "", resResult, ReceiveString);
                    }
               
                }
                catch (System.Threading.ThreadInterruptedException)
                { 
                }
            }
        }


        //同步方法
        public  Boolean SendSync(string Command, int TimeOut)
        {
            if (!this.Active())
            { 
                this.Open();
            }
            ResponseResult resResult = ResponseResult.Unknown;
            string ReceiveString = "";
            Boolean result = false;
            if (this.Active())
            {
                try
                {
                    var dataGram = Encoding.ASCII.GetBytes(Command);
                    this.UDPClient.Send(dataGram, dataGram.Length);
                    var asyncResult = this.UDPClient.BeginReceive(null, null);
                    
                    asyncResult.AsyncWaitHandle.WaitOne(this.ReadTimeout);
                    if (asyncResult.IsCompleted)
                    {
                        try
                        {
                            IPEndPoint remoteEP = null;
                            byte[] ReceiveByte = this.UDPClient.EndReceive(asyncResult, ref remoteEP);
                            ReceiveString = System.Text.ASCIIEncoding.Default.GetString(ReceiveByte);
                            resResult = ResponseResult.Ok;
                            result = true;
                        }
                        catch (System.Exception)
                        {
                            resResult = ResponseResult.Error;
                        }
                    }
                    else
                    {
                        
                        resResult = ResponseResult.TimeOut;
                        this.Close();
                        
                    }
                }
                catch (System.Exception)
                {
                    resResult = ResponseResult.Error;
                }

            }
            else 
            {
                resResult = ResponseResult.Error;
            }
            return result;  
        }

        public override Boolean SendCommand(String Command)
        { 
            this.SendCommandAsync(Command);
            return true;
        }

        public async void SendCommandAsync(String Command)
        {
            Boolean result = await this.SendAsync(Command, this.ReadTimeout);  
        }

        public override void SaveToFile(string fileName)
        {
            IniFiles.WriteStringValue(fileName, this.Caption, "Host", this.Host);
            IniFiles.WriteIntValue(fileName, this.Caption, "Port", this.Port);
            IniFiles.WriteIntValue(fileName, this.Caption, "ReadTimeout", this.ReadTimeout);
        }

        public override void LoadFromFile(string fileName)
        {
            this.Host = IniFiles.GetStringValue(fileName, this.Caption, "Host", "127.0.0.1");
            this.Port = IniFiles.GetIntValue(fileName, this.Caption, "Port", 8000);
            this.ReadTimeout = IniFiles.GetIntValue(fileName, this.Caption, "ReadTimeout", 1000);
            base.LoadFromFile(fileName);
        }








    }
}
