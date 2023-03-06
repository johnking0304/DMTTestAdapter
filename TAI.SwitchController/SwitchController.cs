using System;
using DMT.Core.Protocols;
using DMT.Core.Models;



namespace TAI.Manager
{



     public class SwitchController : Controller, IController
    {
       
        public ModbusTCPClient Channel { get; set; }
        public SwitchOperator Operator { get; set; }

        public SwitchController()
        {
            this.Caption = "SwitchController";
            this.Channel = new ModbusTCPClient(this.Caption);
            this.Operator = new SwitchOperator();    
        }


        public override void LoadFromFile(string fileName)
        {
            this.Operator.LoadFromFile(fileName);
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


        public bool SwitchMode(SwitchMode mode)
        {
            this.Operator.ModeItem.Datas[0] = (ushort)mode;
            this.Channel.WriteMultipleRegisters(this.Operator.ModeItem.StartAddress, this.Operator.ModeItem.Datas);
            return this.Channel.HasError;
        }
        public bool SwitchChannel(ushort channelId)
        {
            this.Operator.SwitchItem.Datas[0] = channelId;
            this.Channel.WriteMultipleRegisters(this.Operator.SwitchItem.StartAddress, this.Operator.SwitchItem.Datas);
            return this.Channel.HasError;
        }


        public override void ProcessEvent()
        {
            if (!this.Channel.HasError)
            { 
                
            }
            else if( this.WaitTimeOut())
            {
                //通道读写错误，重连判断及动作
                this.Channel.ReConnectTCPServer();
            }
        }
    }
}
