using System;
using DMT.Core.Protocols;
using DMT.Core.Models;
using System.Collections.Generic;

namespace TAI.Manager
{

    /// <summary>
    /// 测试流程控制
    /// </summary>
    public class ProcessController: Controller, IController
    {
        public ModbusTCPClient ReadChannel { get; set; }
        public ModbusTCPClient WriteChannel { get; set; }

        public ProcessController()
        {
            


        }


        public void OperateInitialize()
        {

            
        }


        public bool Initialize()
        {

            return true;
        }

        public bool Open()
        {
            throw new NotImplementedException();
        }

        public bool Close()
        {
            throw new NotImplementedException();
        }

        public bool Active()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {

            return;
        }


        public override void ProcessEvent()
        {
            if (!this.ReadChannel.HasError)
            {
                //读取Item值
                if (this.ReadTrigger())
                { 
                    
                
                }            
            }
            else if (this.WaitTimeOut())
            {
                //通道读写错误，重连判断及动作
                this.ReadChannel.ReConnectTCPServer();
            }

            if (this.WriteChannel.HasError)
            {
                if (this.WaitTimeOut())
                {
                    //通道读写错误，重连判断及动作
                    this.ReadChannel.ReConnectTCPServer();
                }
            }
        }
    
    }
}
