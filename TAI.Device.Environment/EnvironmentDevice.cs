using System;
using DMT.Core.Models;
using DMT.Core.Protocols;
using DMT.Core.Utils;


namespace TAI.Device
{

    public class EnvironmentDevice : OperatorController, IController
    {
        public ModbusTCPClient Channel { get; set; }

        public EnvironmentOperator EnvironmentOperator { get; set; }


        public EnvironmentDevice() : base()
        {
            this.Caption = "EnvironmentDevice";
            this.Channel = new ModbusTCPClient(this.Caption);
            
            this.StatusMessage.Name = this.Caption;

        }

        public bool Active()
        {
            return (!this.Channel.HasError);
        }

        public bool Close()
        {
            return true;
        }


        public override void LoadFromFile(string fileName)
        {
            this.Channel.Initialize(fileName);
            this.EnvironmentOperator = new EnvironmentOperator(this.Channel.BaseIndex);
            this.EnvironmentOperator.LoadFromFile(fileName);
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



 

        public bool GetEnvironmentData(ref float temperature, ref float humidity)
        {
            ushort[] data = this.Channel.ReadHoldingRegisters(this.EnvironmentOperator.EnvironmentData.StartAddress, this.EnvironmentOperator.EnvironmentData.Length);
            if (!this.Channel.HasError)
            {
                temperature = data[0] / 10;
                humidity = data[1] / 10;
            }
            return !this.Channel.HasError;
        }

        
        public override void ProcessEvent()
        {
            if (this.Active())
            {

            }
            else
            {
                //通道读写错误，重连判断及动作
                this.Channel.ReConnectTCPServer();
            }
        }

    }
}

