
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using DMT.Core.Channels;
using DMT.Core.Utils;

namespace DMT.Core.Channels
{
	public delegate void ServerDelegate(int numConnections);
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
    public class TCPService : BaseChannel
	{
        public const int EVENT_TYPE = 40000;
		private Socket mainsocket = null; //socket of the main (listening socket)
		private System.Collections.Specialized.ListDictionary clientsockets = null; //List of all of the sockets to the clients
		private Boolean stopping = false;
        public int numConnections { get; set; } //Number of clients connected
		private int currentConnectionID = 1;  //Used for a unique id for each socket
		private Thread listeningThread; //Thread of the listener socket
		const int MaxConnectionsAllowed = 100;
		private int tcpPort { get; set; }

		/// <summary>
		/// Property for current number of connections.  Fires event on change.
		/// </summary>
		private int NumConnections
		{
			set
			{
				this.numConnections = value;
				if (null != this.OnConnectionChange)
					this.OnConnectionChange(this.numConnections);
			}
			get
			{
				return this.numConnections;
			}

		}

		/// <summary>
		/// Increments serial number on get()
		/// </summary>
		protected int NextConnectionNumber
		{
			get
			{
				return this.currentConnectionID++;
			}

		}

		/// <summary>
		/// Event that fires when a client connects or disconnects.
		/// </summary>
		public event ServerDelegate OnConnectionChange;

		/// <summary>
		/// Default constructor for the TCP Server Class.
		/// </summary>
		public TCPService()
		{
			this.Caption = "TCPService";
			this.clientsockets = new System.Collections.Specialized.ListDictionary();
			this.NumConnections = 0;
			this.tcpPort = 9000;
		}

		public TCPService(string caption)
		{
			this.Caption = caption + "TCPService";
			this.clientsockets = new System.Collections.Specialized.ListDictionary();
			this.NumConnections = 0;
		}

		/// <summary>
		/// Starts listening for client connections and accepts them
		/// </summary>
		public override Boolean Open()
		{
			this.stopping = false;

			if (null != this.mainsocket)
			{
				//Shouldn't have a problem
				//Check if connected
				if (this.mainsocket.Connected)
				{
					//Socket is connected.  Let's close the connection and start over
					this.mainsocket.Close();
				}
				
				//Let's get rid of the socket.  I don't know anything about it
				//This will abandon it for GC
				this.mainsocket = null;
			}

			try
			{
				//Bind to the local network
				String szHostName = System.Net.Dns.GetHostName();
				IPHostEntry he = System.Net.Dns.GetHostEntry(szHostName);
				IPEndPoint ep = new System.Net.IPEndPoint(IPAddress.Any,this.tcpPort);
				ep.Create(new SocketAddress(AddressFamily.InterNetwork));

				this.mainsocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
				this.mainsocket.Bind(ep);
				this.mainsocket.Listen(5);

				this.listeningThread = new Thread(new ThreadStart(this.Listen));
				this.listeningThread.IsBackground = true;
				this.listeningThread.Start();
                this.LastMessage = "TCP服务开启成功!";
                this.Notify(EVENT_TYPE, ChannelControl.Open.ToString(), "", ChannelResult.OK, this.LastMessage);
                return true;
			}
			catch (SocketException ex)
			{
				this.mainsocket = null;
                this.LastMessage = "TCP服务开启失败!";
                this.Notify(EVENT_TYPE, ChannelControl.Open.ToString(), ex.ToString(), ChannelResult.CanNotOpen, this.LastMessage);
                return false;
            }
		}

		private void Listen()
		{
			//Sanity Check
			if (null == this.mainsocket)
			{
                this.LastMessage = "无有效的Socket侦听!";
                this.Notify(EVENT_TYPE, ChannelControl.Open.ToString(), "", ChannelResult.CanNotOpen, this.LastMessage);
			}

			try
			{
				for(;;)
				{
					if (this.mainsocket.Poll(100000,SelectMode.SelectRead) && (this.NumConnections <= TCPService.MaxConnectionsAllowed))
					{
						//We must have an incoming connection
						Socket newsock = this.mainsocket.Accept();
						if (null != newsock)
						{
							//It is a good socket, add it to the queue
							//Start Receiving on the socket now
							SockWrapper s = new SockWrapper(this.NextConnectionNumber,newsock);
							ThreadPool.QueueUserWorkItem(new WaitCallback(this.ReceiveClient),s);
							this.LastMessage = string.Format("新的连接[{0}]!",s.Address);
							this.Notify(EVENT_TYPE, ChannelControl.Connect.ToString(), "", ChannelResult.CanNotOpen, this.LastMessage);
						}
					}
				}
			}
			catch(Exception ex)
			{
				//If a Stop() is issued while in Receive(), that's ok
				//else thow exception
				if (!this.stopping)
				{
                    this.LastMessage = "侦听错误,网络连接结束!";
                    this.Notify(EVENT_TYPE, ChannelControl.Open.ToString(), ex.ToString(), ChannelResult.CanNotOpen, this.LastMessage);
				}
				//throw ex;
			}

		}

		/// <summary>
		/// Stops listening for new connections and disconnects all conncected clients.
		/// </summary>
		public override Boolean Close()
		{
			try
			{
				this.stopping = true;
				if (this.listeningThread.IsAlive)
				{
					this.listeningThread.Abort();
					this.listeningThread = null;
				}
				//Stop incoming connections
				this.mainsocket.Close();
				this.DisconnectAll();
                this.LastMessage = "TCP服务关闭成功!";
                this.Notify(EVENT_TYPE, ChannelControl.Close.ToString(), "", ChannelResult.OK, this.LastMessage);
                return true;
				
			}
			catch (Exception ex)
			{
                this.LastMessage = "TCP服务关闭失败!";
                this.Notify(EVENT_TYPE, ChannelControl.Close.ToString(), ex.ToString(), ChannelResult.CanNotClose, this.LastMessage);
                return false;
            }
		}

		private void DisconnectAll()
		{
			for (int x = 0; x < this.clientsockets.Count;x++)
			{
				SockWrapper s = (SockWrapper) this.clientsockets[x];
				if (s != null)
				{
					this.CleanupClientSocket(s);
				}
			}
		}

		private void ReceiveClient(object objectO)
		{
			//SockWrapper s = (SockWrapper) this.sockqueue.Dequeue();
			SockWrapper s = (SockWrapper) objectO;
			this.clientsockets.Add(s.connectionID,s);
			this.NumConnections++;
			byte[] data = null;

			//Sanity Check
			if (null == s)
			{
                this.LastMessage = "错误：SockWrapper为空!";
                this.Notify(EVENT_TYPE, ChannelControl.Receive.ToString(), "", ChannelResult.ReceiveError, this.LastMessage);
				return;
			}
			if (s.State != SocketState.Connected)
			{
                this.LastMessage = "错误：Socket未连接!";
                this.Notify(EVENT_TYPE, ChannelControl.Receive.ToString(), "", ChannelResult.ReceiveError, this.LastMessage);
				this.CleanupClientSocket(s);
				return;
			}

			while (s.State == SocketState.Connected)
			{
				data = s.Receive();
				if (data != null)
				{
                    string ReceiveString = System.Text.Encoding.UTF8.GetString(data);
					ReceiveString = ReceiveString.Trim('\0');
					if (!string.IsNullOrEmpty(ReceiveString))
					{
						ChannelResult resResult = ChannelResult.OK;
						this.LastMessage = "信息：收到数据(上报)";
						this.Notify(EVENT_TYPE, ChannelControl.Report.ToString(), ReceiveString, resResult, this.LastMessage); 
					}		
				}
			}

			this.CleanupClientSocket(s);

		}

		private void CleanupClientSocket(SockWrapper s)
		{
			if (s.State == SocketState.Disconnected)
			{

				this.NumConnections--;
				this.clientsockets.Remove(s.connectionID);
			}
			else
			{
				s.Close();
				this.clientsockets.Remove(s.connectionID);
			}
			this.LastMessage = string.Format("连接断开[{0}]!", s.Address);
			this.Notify(EVENT_TYPE, ChannelControl.Disconnect.ToString(), "", ChannelResult.OK, this.LastMessage);
		}

        public override bool SendCommand(string Command)
        {
            byte[] dataGram = Encoding.ASCII.GetBytes(Command);
            return this.SendCommandNoReply(dataGram);
        }

        public override bool SendCommandNoReply(byte[] command)
        {
			int[] vals = new int[TCPService.MaxConnectionsAllowed];
			this.clientsockets.Keys.CopyTo(vals,0);
			foreach (int x in  vals)
			{
				SockWrapper s = (SockWrapper) this.clientsockets[x];
				if (s != null)
				{
					s.Send(command);
				}
			}
            return true;
		}


		public override void SaveToFile(string fileName)
		{
			IniFiles.WriteIntValue(fileName, this.Caption, "Port", this.tcpPort);

		}

		public override void LoadFromFile(string fileName)
		{
			this.tcpPort = IniFiles.GetIntValue(fileName, this.Caption, "Port", 8000);
			base.LoadFromFile(fileName);
		}



	}

	public enum SocketState
	{
		Disconnected,
		Connected,
		Disconnecting,
		Unknown
	}

	public class SockWrapper
	{
		public readonly int connectionID;
		private Socket sock;
		private SocketState sockState;
		protected static object critSec = new object();

		public string Address {

			get => this.sock.RemoteEndPoint.ToString();
				}

		public SocketState State
		{
			get
			{
				return this.sockState;
			}
		}

		~SockWrapper()
		{
			this.Close();
		}

		public SockWrapper(int connectionID, Socket sock)
		{
			this.sock = sock;
			this.connectionID = connectionID;
			this.sockState = SocketState.Connected;
			
		}

		public void Send(byte[] message)
		{
			if (this.sockState == SocketState.Connected)
			{
				try
				{
					if (null != this.sock)
					{						
						//We need to enter a critcal section so that sockets
						//do not send at the same time.  Streams could get really
						//screwed up
						Monitor.Enter(SockWrapper.critSec);
						sock.Send(message);
					}
				}
				catch(SocketException ex)
				{
					//The socket is invalid
					//Stop Sending to it
					this.Close();
					//Debug.Assert(false,"Failed during Send()","Somebody closed the socket.  Abandon the socket");
				}
				catch(Exception ex)
				{
					this.Close();
				}
				finally
				{
					//Exit the critsec in the finally.  Otherwise if exception thrown
					//the critsec never is exited
					Monitor.Exit(SockWrapper.critSec);
				}
			}
		}

		public byte[] Receive()
		{
			byte[] data = null;
			int nAvailable = 0;
			

			if (this.sockState == SocketState.Connected)
			{
				try
				{
					if (null != this.sock)
					{
						
						data = new Byte[4096];
						nAvailable = sock.Receive(data,4096,SocketFlags.None);
					}
						
				}
				catch(SocketException ex)
				{
					//The socket is invalid
					//Stop Sending to it
					this.Close();
					//Debug.Assert(false,"Failed during Receive()","Somebody closed the socket.  Abandon the socket");
				}
				catch(Exception ex)
				{
					this.Close();
		
				}
			}

			return data;
		}


		public void Close()
		{
			this.sockState = SocketState.Disconnecting;
			if (null!= this.sock && this.sock.Connected)
			{
				this.sock.Close();
			}
			this.sockState = SocketState.Disconnected;
		}

	}
}
