using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMT.Core.Channels
{
    public class UDPService : UDPClientChannel
    {
        public IPEndPoint remoteClient;
        public UDPService() :
            base()
        {
            this.Caption = "UDPService";
            this.remoteClient = new IPEndPoint(IPAddress.Any, 0);
        }

        public IPAddress localIPAddress
        {
            get
            {
                //获取本机可用IP地址
                IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress ipa in ips)
                {
                    if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ipa;//获取本地IP地址
                    }
                }
                return null;
            }
        }
        public override bool Open()
        {
            try
            {
                byte[] ipbyte = new byte[] { 0, 0, 0, 0 };
                IPAddress ip = new IPAddress(ipbyte);
                //IPEndPoint local = new IPEndPoint(this.localIPAddress, this.Port);
                IPEndPoint local = new IPEndPoint(ip, this.Port);
                this.UDPClient = new UdpClient(local);
                this.LastMessage = "UDP Server 启动成功！";
                this.StartAsyncReceiveData();
                this.Notify(UDP_CONTROL_EVENT, ChannelControl.Open.ToString(),"", ChannelResult.OK, this.LastMessage);
                
                return true;
            }
            catch (System.ObjectDisposedException)
            {
                this.LastMessage = "UDP Server是关闭的！";
            }
            catch (System.ArgumentOutOfRangeException)
            {
                this.LastMessage = "端口[" + this.Port.ToString() + "]无效！";
            }
            catch (System.Net.Sockets.SocketException)
            {
                this.LastMessage = "网络访问出错！";
            }
            this.Notify(UDP_CONTROL_EVENT, ChannelControl.Open.ToString(), "", ChannelResult.CanNotOpen, this.LastMessage);
            return false;
        }
        public Boolean SendData(string command, string toAddress, int port)
        {

            Boolean result = false;
            try
            {
                IPAddress ipAddress;
                if (IPAddress.TryParse(toAddress, out ipAddress))
                {
                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, port);
                    var dataGram = Encoding.Unicode.GetBytes(command);
                    this.UDPClient.Send(dataGram, dataGram.Length, remoteClient);
                    this.Notify(UDP_DATA_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.OK, command);
                    result = true;
                }

            }
            catch (System.Exception)
            {
                this.Notify(UDP_DATA_EVENT, ChannelControl.Send.ToString(), "", ChannelResult.SendError, command);
                result = false;

            }
            return result;
        }      

    }
}
