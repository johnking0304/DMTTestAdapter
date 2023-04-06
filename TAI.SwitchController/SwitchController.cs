using System;
using DMT.Core.Protocols;
using DMT.Core.Models;
using TAI.Modules;
using DMT.Core.Utils;


namespace TAI.Manager
{
    /// <summary>
    /// SwitchController  通道切换控制器
    /// 
    /// 
    /// </summary>


    public class SwitchController : OperatorController, IController
    {
      
        public ModbusTCPClient Channel { get; set; }
        public SwitchOperator Operator { get; set; }

        public StationType StationType { get; set; }

        public SwitchController(StationType stationType)
        {
            this.Caption = string.Format("{0}SwitchController", stationType.ToString());
            this.Channel = new ModbusTCPClient(this.Caption);
            
            this.StatusMessage.Name = string.Format("{0} SwitchController", stationType.ToString());
        }


        public override void LoadFromFile(string fileName)
        {
            this.Channel.LoadFromFile(fileName);
            

            this.Operator = new SwitchOperator(this.Channel.BaseIndex);
            this.Operator.LoadFromFile(fileName);

        }
        public bool Active()
        {
            return !this.Channel.HasError;
        }

        public bool Close()
        {
            return true;
        }

        public bool Initialize()
        {
            this.SwitchModeOperate(SwitchMode.Off);
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


        public bool SwitchModeOperate(SwitchMode mode)
        {
            this.Operator.ModeSelect.Datas[0] = (ushort)mode;
            this.Channel.WriteMultipleRegisters(this.Operator.ModeSelect.StartAddress, this.Operator.ModeSelect.Datas);
            return !this.Channel.HasError;
        }
        public bool SwitchChannelOperate(ushort channelId)
        {
            this.Operator.ChannelSelectId.Datas[0] = channelId;
            this.Channel.WriteMultipleRegisters(this.Operator.ChannelSelectId.StartAddress, this.Operator.ChannelSelectId.Datas);
            return !this.Channel.HasError;
        }


        public override void ProcessEvent()
        {
            if (!this.Channel.HasError)
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
