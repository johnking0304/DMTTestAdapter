using System;
using DMT.Core.Protocols;
using DMT.Core.Models;
using System.Collections.Generic;
using TAI.Modules;

namespace TAI.Manager
{

    /// <summary>
    /// 测试流程控制
    /// </summary>
    public class ProcessController: OperatorController, IController
    {
        
        public ModbusTCPClient ReadChannel { get; set; }
        public ModbusTCPClient WriteChannel { get; set; }
        public RobotOperator RobotOperator { get; set; }
        public SystemOperator SystemOperator { get; set; }
        
        private ushort[] SystemStatusValues { get; set; }



        public ProcessController()
        {
            this.Caption = "ProcessController";
            this.ReadChannel = new ModbusTCPClient(string.Format("{0}Read",this.Caption));
            this.WriteChannel = new ModbusTCPClient(string.Format("{0}Write", this.Caption));

            this.RobotOperator = new RobotOperator();

            this.SystemOperator = new SystemOperator();

            this.SystemStatusValues = new ushort[SystemOperator.SystemStatusMapLength];
        }

        public override void LoadFromFile(string fileName)
        {
            this.ReadChannel.Initialize(fileName);
            this.WriteChannel.Initialize(fileName);

            this.RobotOperator.LoadFromFile(fileName);
            this.SystemOperator.LoadFromFile(fileName);
        }


 

        public bool Initialize()
        {
            this.InitializeSystem();
            return this.Active();
        }

        public bool Open()
        {
            bool result = this.ReadChannel.ConnectToTCPServer();
            result &= this.WriteChannel.ConnectToTCPServer();
            return result; 
        }

        public bool Close()
        {
            return true;
        }

        public bool Active()
        {
            return (!this.ReadChannel.HasError) && (!this.WriteChannel.HasError);
        }

        public void Start()
        {
            this.StartThread();
            
        }


        public override void ProcessEvent()
        {
            if (!this.ReadChannel.HasError)
            {
                //读取Item值
                if (this.ReadTrigger())
                {
                    this.UpdateSystemStatus();
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







        #region  系统模块
        /// <summary>
        /// 工位是否油料判断
        /// </summary>
        /// <param name="stationType"></param>
        /// <returns></returns>
        public bool StationBusy(StationType stationType)
        {
            ModbusItem item = null;
            switch (stationType)
            {
                case StationType.DI:
                    {
                        item = this.SystemOperator.DIStationBusy;
                        break;
                    }
                case StationType.DO:
                    {
                        item = this.SystemOperator.DOStationBusy;
                        break;
                    }
                case StationType.AI:
                    {
                        item = this.SystemOperator.AIStationBusy;
                        break;
                    }
                case StationType.AO:
                    {
                        item = this.SystemOperator.AOStationBusy;
                        break;
                    }
                case StationType.PI:
                    {
                        item = this.SystemOperator.PIStationBusy;
                        break;
                    }
                case StationType.RTD_3L:
                    {
                        item = this.SystemOperator.RTD3StationBusy;
                        break;
                    }
                case StationType.RTD_4L:
                    {
                        item = this.SystemOperator.RTD4StationBusy;
                        break;
                    }
                case StationType.TC:
                    {
                        item = this.SystemOperator.TCStationBusy;
                        break;
                    }
                case StationType.PREPARE:
                    {
                        item = this.SystemOperator.PrepareStationBusy;
                        break;
                    }

            }
            if (item != null)
            {
                return this.SystemStatusValues[item.StartAddress] == (ushort)Status.StationBusy;
            }
            return false;
        }


        /// <summary>
        /// 初始化流控模块
        /// </summary>
        /// <returns></returns>
        public bool InitializeSystem()
        {
            this.SystemOperator.InitializeOperate.Datas[0] = 1;
            this.WriteChannel.WriteMultipleRegisters(this.SystemOperator.InitializeOperate.StartAddress, this.SystemOperator.InitializeOperate.Datas);
            return (!this.WriteChannel.HasError);
        }


        private void UpdateSystemStatus()
        {
            try
            {
                ushort[] datas = this.ReadChannel.ReadHoldingRegisters(this.SystemOperator.SystemStatusMap.StartAddress, this.SystemOperator.SystemStatusMapLength);
                if (!this.ReadChannel.HasError)
                {
                    Array.Copy(datas, this.SystemStatusValues, this.SystemOperator.SystemStatusMapLength);
                }
            }
            catch
            {

            }
        }




        #endregion

    }
}
