using System;
using DMT.Core.Protocols;
using DMT.Core.Models;


namespace TAI.Manager
{ 
     public class SwitchController : Controller, IController
    {
        public ModbusTCPClient Channel { get; set; }

        public ModbusItem SwitchItem { get; set; }

        public SwitchController()
        {
            this.Caption = "SwitchController";
            this.Channel = new ModbusTCPClient(this.Caption);
            this.SwitchItem = new ModbusItem("SwithChannel",this.Caption,"通道切换",0,1,1,ChannelType.AO);
        }


        public override void LoadFromFile(string fileName)
        {
            this.SwitchItem.LoadFromFile(fileName);
        }
        public bool Active()
        {
            return this.Channel.HasError;
        }

        public bool Close()
        {
            return true;
        }

        public bool Initialize()
        {
            return true;
        }

        public bool Open()
        {
            return this.Channel.ConnectToTCPServer();
        }

        public void Start()
        {
            this.StartThread();
        }


        public bool SwitchChannel(ushort channelId)
        {
            this.SwitchItem.Datas[0] = channelId;
            this.Channel.WriteMultipleRegisters(this.SwitchItem.StartAddress, this.SwitchItem.Datas);
            return true;
        }


        public override void ProcessEvent()
        {
            if (this.Channel.HasError && this.WaitTimeOut())
            {
                //通道读写错误，重连判断及动作
                this.Channel.ReConnectTCPServer();
            }
        }
    }
}
