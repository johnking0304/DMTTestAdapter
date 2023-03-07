using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DMT.Core.Utils;

namespace DMT.Core.Protocols
{
    public class ModbusTCPService:Subject
    {

        public static ushort DATA_LENGTH = 2096 ;
        public string Name = "ModbusTCPService";
        public ModbusSlave ModbusSlave { get; set; }        
        public int ListenPort { get; set; }
        public byte SlaveId { get; set; }
        public string LastMessage { get; set; }
        public ModbusErrorCode LastErrorCode { get; set; }


        public ModbusTCPService()
        {
            this.Name = "ModbusTCPService";
            this.LastMessage = "";
            this.ListenPort = 502;
            this.SlaveId = 1;
            this.LastErrorCode = ModbusErrorCode.Ok;
        }
        public override void ProcessResponse(int notifyEvent, string flag,string content, object result, string message, object sender)
        {
            
        }

        public void Initialaze(string configFileName)
        {
            this.LoadFromFile(configFileName);
            try
            {
                IPAddress address = new IPAddress(new byte[] { 0, 0, 0, 0 });

                TcpListener slaveTcpListener = new TcpListener(address, this.ListenPort);
                slaveTcpListener.Start();

                this.ModbusSlave = ModbusTcpSlave.CreateTcp(this.SlaveId, slaveTcpListener);
                this.ModbusSlave.DataStore = DataStoreFactory.CreateDefaultDataStore(DATA_LENGTH, DATA_LENGTH, DATA_LENGTH, DATA_LENGTH);

                this.ModbusSlave.Listen();
                this.LastErrorCode = ModbusErrorCode.Ok;   
                this.LastMessage = "Modbus TCP Service 启动成功";

            }
            catch (Exception)
            {
                this.LastErrorCode = ModbusErrorCode.InitError;   
                this.LastMessage = "Modbus TCP Service 启动失败";   
                
            }
            this.Notify((int)EVENT_MODBUS.MODBUS_TCP_INIT, this.LastErrorCode.ToString(),"", this.ModbusSlave, this.LastMessage);
        }

        public void LoadFromFile(string fileName)
        {
            string Section = this.Name;
            this.ListenPort = IniFiles.GetIntValue(fileName, Section, "ListenPort", 502);
            this.SlaveId = (byte)IniFiles.GetIntValue(fileName, Section, "SlaveId", 1);

            string[] list = IniFiles.GetAllSectionNames(fileName);
            if (!list.Contains(Section))
            {
                this.SaveToFile(fileName);
            }
        }
        public void SaveToFile(string fileName)
        {
            string Section = this.Name;
            IniFiles.WriteIntValue(fileName, Section, "ListenPort", this.ListenPort);
            IniFiles.WriteIntValue(fileName, Section, "SlaveId", this.SlaveId );
        }


        public Boolean hasError
        {
            get
            {
                return this.LastErrorCode != ModbusErrorCode.Ok;
            }
        }


        public ushort[] GetHoldingRegisterDatas(int startIndex, int length)
        {
            ushort[] datas = new ushort[length];
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                datas[i - startIndex] = this.ModbusSlave.DataStore.HoldingRegisters[i];     
            }
            return datas;
        }

        public ushort[] GetInputRegisterDatas(int startIndex, int length)
        {
            ushort[] datas = new ushort[length];
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                datas[i - startIndex] = this.ModbusSlave.DataStore.InputRegisters[i];     
            }
            return datas;
        }

        public Boolean[] GetCoilDiscreteDatas(int startIndex, int length)
        {
            bool [] datas = new bool[length];
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                datas[i - startIndex] = this.ModbusSlave.DataStore.CoilDiscretes[i];
            }
            return datas;
        }


        public Boolean[] GetInputDiscreteDatas(int startIndex, int length)
        {
            bool[] datas = new bool[length];
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                datas[i - startIndex] = this.ModbusSlave.DataStore.InputDiscretes[i];
            }
            return datas;
        }



        public void SetHoldingRegisterDatas(ushort[] datas,int startIndex, int length)
        {
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                this.ModbusSlave.DataStore.HoldingRegisters[i] = datas[i - startIndex] ;
            }
        }


        public void SetInputRegisterDatas(ushort[] datas, int startIndex, int length)
        {
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                this.ModbusSlave.DataStore.InputRegisters[i] = datas[i - startIndex];
            }
        }





        public void SetInputDiscreteDatas(Boolean[] datas,int startIndex, int length)
        {

            for (int i = startIndex; i < (startIndex + length); i++)
            {
                this.ModbusSlave.DataStore.InputDiscretes[i] = datas[i - startIndex];
            }
        }

        public void SetCoilDiscreteDatas(Boolean[] datas, int startIndex, int length)
        {

            for (int i = startIndex; i < (startIndex + length); i++)
            {
                this.ModbusSlave.DataStore.CoilDiscretes[i] = datas[i - startIndex];
            }
        }


        public void ClearHoldingRegisters()
        {
            this.ClearData(ModbusDataType.HoldingRegister, 1, DATA_LENGTH);
        }

        public void ClearInputRegisters()
        {
            this.ClearData(ModbusDataType.InputRegister, 1, DATA_LENGTH);
        }


        public void ClearCoilDiscretes()
        {
            this.ClearData(ModbusDataType.Coil, 1, DATA_LENGTH);
        }

        public void ClearInputDiscretes()
        {
            this.ClearData(ModbusDataType.Input, 1, DATA_LENGTH);
        }


        public void ClearData(ModbusDataType dataType, int startIndex, int length)
        {
            for(int i=startIndex;i<(startIndex+length);i++)
            {
                switch (dataType)
                {
                    case ModbusDataType.Coil:
                        {
                            this.ModbusSlave.DataStore.CoilDiscretes[i] = false;
                            break;
                        }
                    case ModbusDataType.HoldingRegister:
                        {
                            this.ModbusSlave.DataStore.HoldingRegisters[i] = 0;
                            break;
                        }
                    case ModbusDataType.InputRegister:
                        {
                            this.ModbusSlave.DataStore.InputRegisters[i] = 0;
                            break;
                        }
                    case ModbusDataType.Input:
                        {
                            this.ModbusSlave.DataStore.InputDiscretes[i] = false;
                            break;
                        }
            
                }
            }
        }



    }

    public class ModbusTCPClient : Subject
    {
        public string Name = "ModbusTCPClient";
        public static int DEFAULT_PORT = 502;

        public const int WRITE_MAX = 123;
        public ModbusErrorCode LastErrorCode { get; set; }
        public short BaseIndex { get; set; }
        public ModbusMaster ModbusClient { get; set; }

        private TcpClient TcpClient;
        public byte SlaveAddress { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        private DateTime LastConnectedDatetime{get;set;}

        public string LastMessage { get; set; }
        public void LoadFromFile(string fileName)
        {
            string Section = this.Name;

            this.Ip = IniFiles.GetStringValue(fileName, Section, "IPAddress", "127.0.0.1");
            this.Port = IniFiles.GetIntValue(fileName, Section, "Port", DEFAULT_PORT);
            this.SlaveAddress = (byte)IniFiles.GetIntValue(fileName, Section, "SlaveAddress", 1);
            this.BaseIndex = (short)IniFiles.GetIntValue(fileName, Section, "BaseIndex", 0);

            string[] list = IniFiles.GetAllSectionNames(fileName);
            if (!list.Contains(Section))
            {
                this.SaveToFile(fileName);
            }
        }
        public void SaveToFile(string fileName)
        {
            string Section = this.Name;
            IniFiles.WriteStringValue(fileName, Section, "IPAddress", this.Ip);
            IniFiles.WriteIntValue(fileName, Section, "Port", this.Port);
            IniFiles.WriteIntValue(fileName, Section, "SlaveAddress", this.SlaveAddress);
            IniFiles.WriteIntValue(fileName, Section, "BaseIndex", this.BaseIndex);
        }
        public ModbusTCPClient(string caption)
        {
            this.Name =string.Format("{0}ModbusTCPClient", caption);
            this.LastMessage = "";
            this.LastErrorCode = ModbusErrorCode.Ok;
            this.SlaveAddress = 1;
        }


        public Boolean HasError
        {
            get {
                return this.LastErrorCode != ModbusErrorCode.Ok;
            }
        }

        
        public Boolean ReConnectTCPServer()
        {
            if (this.LastErrorCode != ModbusErrorCode.Ok &&　this.ConnectTimeOut())
            {
               return  this.ConnectToTCPServer();
            }
            return true;
        }

        public Boolean ConnectTimeOut()
        {
            TimeSpan span = DateTime.Now - this.LastConnectedDatetime;
            if (span.TotalSeconds >= 5)
            {
                this.LastConnectedDatetime = DateTime.Now;
                return true;
            }
            return false;
        }
        public Boolean ConnectToTCPServer()
        {
            try
            {
                this.TcpClient = new TcpClient();
                this.ModbusClient = ModbusIpMaster.CreateIp(this.TcpClient);    

                IAsyncResult connResult = this.TcpClient.BeginConnect(this.Ip, this.Port, null, null);
                connResult.AsyncWaitHandle.WaitOne(1500, true);
                if (!connResult.IsCompleted)
                {
                    this.TcpClient.Close();
                    this.LastMessage = "无法连接到Modbus TCP 服务器";
                    this.LastErrorCode = ModbusErrorCode.InitError;
                    this.Notify((int)EVENT_MODBUS.MODBUS_TCP_INIT, ModbusErrorCode.InitError.ToString(), "", this.ModbusClient, this.LastMessage);
                    return false;
                }
                else if (this.TcpClient != null && this.TcpClient.Connected)
                {
                    this.LastMessage = "连接到Modbus TCP服务器!";
                    this.LastConnectedDatetime = DateTime.Now;
                    this.LastErrorCode = ModbusErrorCode.Ok;
                    this.Notify((int)EVENT_MODBUS.MODBUS_TCP_INIT, ModbusErrorCode.Ok.ToString(), "", this.ModbusClient, this.LastMessage);
                    return true;
                }
                else
                {
                    this.TcpClient.Close();
                    this.LastMessage = "无法连接到Modbus TCP 服务器";
                    this.LastErrorCode = ModbusErrorCode.InitError;
                    this.Notify((int)EVENT_MODBUS.MODBUS_TCP_INIT, ModbusErrorCode.InitError.ToString(), "", this.ModbusClient, this.LastMessage);
                    return false;
                }
            }
            catch (Exception msg)
            {
                this.TcpClient.Close();
                this.LastMessage = "无法连接到Modbus TCP 服务器";
                this.LastErrorCode = ModbusErrorCode.InitError;
                this.Notify((int)EVENT_MODBUS.MODBUS_TCP_INIT, ModbusErrorCode.InitError.ToString(), "", this.ModbusClient, this.LastMessage + ":" + msg.ToString());
                return false;
            }

        }
        public void Initialize(string fileName)
        {
            this.LoadFromFile(fileName);
            this.ConnectToTCPServer();
        }

        public ushort[] ReadModbusItem(ModbusItem item)
        {
            return this.ReadHoldingRegisters(item.StartAddress, item.Length);
        }

        public ushort[] ReadHoldingRegisters(ushort startAddress, ushort numberOfPoints)
        {
            ushort len = numberOfPoints;
            ushort remainder = (ushort)(len % WRITE_MAX);
            int count = len / WRITE_MAX;
            ushort newAddress = startAddress;
            int index = 0;

            ushort[] datas = new ushort[numberOfPoints];

            try
            {
                for (int i = 0; i < count; i++)
                {
                    ushort[] readDatas = this.ModbusClient.ReadHoldingRegisters(this.SlaveAddress, newAddress, WRITE_MAX);
                    Array.Copy(readDatas, 0, datas, index, WRITE_MAX);
                    newAddress += WRITE_MAX;
                    index += WRITE_MAX;
                }
                if (remainder != 0)
                {
                    ushort[] readDatas = this.ModbusClient.ReadHoldingRegisters(this.SlaveAddress, newAddress, remainder);
                    Array.Copy(readDatas, 0, datas, index, remainder);
                }
        
                this.LastErrorCode = ModbusErrorCode.Ok;
                this.Notify((int)EVENT_MODBUS.MODBUS_TCP_READ, ModbusErrorCode.Ok.ToString(), "", this.ModbusClient, this.LastMessage + "成功");
                return datas;
            }
            catch (System.InvalidOperationException)
            {
                this.LastErrorCode = ModbusErrorCode.ReadError;
                this.LastMessage = "无法读取Modbus TCP数据";
                this.Notify((int)EVENT_MODBUS.MODBUS_TCP_READ, ModbusErrorCode.ReadError.ToString(), "", this.ModbusClient, this.LastMessage);
                return datas;
            }
            catch (Exception )
            {
                this.LastErrorCode = ModbusErrorCode.ReadError;
                this.LastMessage = "无法读取Modbus TCP数据";
                this.Notify((int)EVENT_MODBUS.MODBUS_TCP_READ, ModbusErrorCode.ReadError.ToString(), "", this.ModbusClient, this.LastMessage);
                return datas;
            }
        }



        public override  void ProcessResponse(int notifyEvent, string flag, string content,object result, string message, object sender)
        {
            this.Notify( notifyEvent,  flag,content,  result,  message);
        }

        public void WriteModbusItem(ModbusItem item)
        {
            this.WriteMultipleRegisters(item.StartAddress, item.Datas);
        }

        public void WriteMultipleRegisters(ushort startAddress, ushort[] data)
        {
            try
            {
                ushort len =(ushort) data.Length;
                ushort remainder =(ushort)( len % WRITE_MAX);
                int count = len / WRITE_MAX;
                ushort newAddress = startAddress;
                int index = 0;

                ushort[] datas = new ushort[WRITE_MAX];
                for (int i = 0; i < count;i++ )
                {
                    Array.Copy(data, index, datas, 0, WRITE_MAX);
                    this.ModbusClient.WriteMultipleRegisters(this.SlaveAddress, newAddress, datas);
                    newAddress += WRITE_MAX;
                    index += WRITE_MAX;
                }
                if (remainder != 0)
                {
                    datas = new ushort[remainder];
                    Array.Copy(data, index, datas, 0, remainder);
                    this.ModbusClient.WriteMultipleRegisters(this.SlaveAddress, newAddress, datas);                  
                }
                this.LastErrorCode = ModbusErrorCode.Ok;
                this.Notify((int)EVENT_MODBUS.MODBUS_TCP_READ, ModbusErrorCode.Ok.ToString(), "", this.ModbusClient, this.LastMessage + "成功");
            }
            catch (System.InvalidOperationException)
            {
                this.LastErrorCode = ModbusErrorCode.WriteError;
                this.LastMessage = "无法写入Modbus TCP数据";
                this.Notify((int)EVENT_MODBUS.MODBUS_TCP_WRITE, ModbusErrorCode.WriteError.ToString(), "", this.ModbusClient, this.LastMessage);
            }
            catch (Exception)
            {
                this.LastErrorCode = ModbusErrorCode.WriteError;
                this.LastMessage = "无法写入Modbus TCP数据";
                this.Notify((int)EVENT_MODBUS.MODBUS_TCP_READ, ModbusErrorCode.ReadError.ToString(), "", this.ModbusClient, this.LastMessage);
            }

        }
    }


}
