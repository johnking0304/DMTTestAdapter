using System;
using DMT.Core.Protocols;
using DMT.Core.Models;
using System.Collections.Generic;
using TAI.Modules;
using DMT.Core.Utils;

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

        public DetectOperator DetectOperator { get; set; }

        private ushort[] SystemStatusValues { get; set; }



        public ProcessController()
        {
            this.Caption = "ProcessController";
            this.ReadChannel = new ModbusTCPClient(string.Format("{0}Read",this.Caption));
            this.WriteChannel = new ModbusTCPClient(string.Format("{0}Write", this.Caption));

            
            this.StatusMessage.Name = this.Caption;
        }

        public override void LoadFromFile(string fileName)
        {
            this.ReadChannel.Initialize(fileName);
            this.WriteChannel.Initialize(fileName);

            this.RobotOperator = new RobotOperator(this.ReadChannel.BaseIndex);
            this.SystemOperator = new SystemOperator(this.ReadChannel.BaseIndex);
            this.DetectOperator = new DetectOperator(this.ReadChannel.BaseIndex);

            this.SystemStatusValues = new ushort[SystemOperator.SystemStatusMapLength];
            this.RobotOperator.LoadFromFile(fileName);
            this.SystemOperator.LoadFromFile(fileName);
            this.DetectOperator.LoadFromFile(fileName);
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
        /// 工位是否有料判断
        /// </summary>
        /// <param name="stationType"></param>
        /// <returns></returns>
        public bool StationIsBusy(StationType stationType)
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
                case StationType.Prepare:
                    {
                        item = this.SystemOperator.PrepareStationBusy;
                        break;
                    }

            }
            if (item != null)
            {
                return this.SystemStatusValues[item.StartAddress] == (ushort)Status.Busy;
            }
            return false;
        }



        public bool FeedLack {
            get {
                return this.SystemStatusValues[this.SystemOperator.FeedLackSignal.StartAddress] == (ushort)Status.Valid;
            }
        }

        public bool BlankingOKFull
        {
            get
            {
                return this.SystemStatusValues[this.SystemOperator.OKBlankFullSignal.StartAddress] == (ushort)Status.Valid;
            }
        }


        public bool BlankingNGFull
        {
            get
            {
                return this.SystemStatusValues[this.SystemOperator.NGBlankFullSignal.StartAddress] == (ushort)Status.Valid;
            }
        }


        public bool InitializeCompleted
        {
            get
            {
                return this.SystemStatusValues[this.SystemOperator.InitializeCompleted.StartAddress] == (ushort)Status.Completed;
            }
        }


        /// <summary>
        /// 初始化流控模块
        /// </summary>
        /// <returns></returns>

        public bool InitializeSystem()
        {
            this.SystemOperator.InitializeOperate.Datas[0] = 1;
            this.WriteChannel.WriteModbusItem(this.SystemOperator.InitializeOperate);
            return (!this.WriteChannel.HasError);
        }

        private void UpdateSystemStatus()
        {
            try
            {              
                ushort[] datas = this.ReadChannel.ReadModbusItem(this.SystemOperator.SystemStatusMap);
                if (!this.ReadChannel.HasError)
                {
                    Array.Copy(datas, this.SystemStatusValues, this.SystemOperator.SystemStatusMapLength);
                    this.StatusMessage.LastErrorCode = this.SystemErrorCode;
                    this.StatusMessage.LastMessage = this.MessageText;
                }
            }
            catch
            {

            }
        }


        public bool SystemTerminated
        {
            get
            {
                return this.SystemStatusValues[this.SystemOperator.SystemStop.StartAddress] == (ushort)Status.Valid;
            }
        }

        public bool NewFeedSignal
        {
            get
            {
                if (this.SystemStatusValues[this.SystemOperator.NewFeedSignal.StartAddress] == (ushort)Status.Valid)
                {
                    this.SystemOperator.NewFeedSignalReset.Datas[0] = (ushort)1;
                    this.WriteChannel.WriteModbusItem(this.SystemOperator.NewFeedSignalReset);
                    return true;
                }
                return false;
            }
        }


        public ushort SystemErrorCode
        {
            get {
                return this.SystemStatusValues[this.SystemOperator.SystemStop.StartAddress];
               }
        }


        public string MessageText
        {
            get
            {
                return "ErrorCode=" + this.SystemErrorCode.ToString();
            }
        }

        #endregion


        #region Robot



        public bool RobotIdle
        {
            get {
                return this.SystemStatusValues[this.RobotOperator.GetIdleStatus.StartAddress] == (ushort)Status.Valid;
            }
        }


        public bool SetRobotMoveParams(int positionStart, int positionTarget , ActionMode mode)
        {
            this.RobotOperator.MoveActionParams.Datas[0] = (ushort)positionStart;
            this.RobotOperator.MoveActionParams.Datas[1] = (ushort)positionTarget;
            this.RobotOperator.MoveActionParams.Datas[2] = (ushort)mode;
            this.WriteChannel.WriteModbusItem(this.RobotOperator.MoveActionParams);

            LogHelper.LogInfoMsg(string.Format("机械手移动位置[{0}->{1}],模式：{2}", positionStart, positionTarget, mode.ToString()));
            return (! this.WriteChannel.HasError);
        }

        public bool SetRobotMoveEnable()
        {
            this.RobotOperator.EnabelMoveAction.Datas[0] = (ushort)Status.Enable;
            this.WriteChannel.WriteModbusItem(this.RobotOperator.EnabelMoveAction);          
            return !this.WriteChannel.HasError;
        }

        public bool SetRobotMoveEnable(Position start, Position target, ActionMode mode,bool check)
        {
            bool result = true;
            if (check)
            {
                result &= this.SystemStatusValues[this.RobotOperator.MoveActionParams.StartAddress] == (ushort)start;
                result &= this.SystemStatusValues[this.RobotOperator.MoveActionParams.StartAddress+1] == (ushort)target;
                result &= this.SystemStatusValues[this.RobotOperator.MoveActionParams.StartAddress+2] == (ushort)mode;
            }
            if (result)
            {
                this.RobotOperator.EnabelMoveAction.Datas[0] = (ushort)Status.Enable;
                this.WriteChannel.WriteModbusItem(this.RobotOperator.EnabelMoveAction);
            }
            return !this.WriteChannel.HasError;
        }




        public bool StartStationTest(int stationId)
        {
            ushort station = (ushort)((ushort)stationId + (ushort)(Position.StationBase));

            this.RobotOperator.SetTestStationId.Datas[0] = station;
            this.WriteChannel.WriteModbusItem(this.RobotOperator.SetTestStationId);

            this.DetectOperator.StartStationTest.Datas[0] = 1;  //启动 OK =1  NG=2

            this.WriteChannel.WriteModbusItem(this.DetectOperator.StartStationTest);

            LogHelper.LogInfoMsg(string.Format("启动工位[{0}]测试", stationId));
            return (!this.WriteChannel.HasError);
        }


        public bool RobotMoveCompleted
        {
            get
            {
                if (this.SystemStatusValues[this.RobotOperator.MoveCompletedStatus.StartAddress] == (ushort)Status.Completed)
                {
                    this.RobotOperator.MoveCompletedStatus.Datas[0] = (ushort)0;
                    this.WriteChannel.WriteModbusItem(this.RobotOperator.MoveCompletedStatus);
                    return true;
                }
                return false;
            }

        }





        #endregion





        #region Detector

        public bool ModuleTestReady
        {
            get
            {
                return  this.SystemStatusValues[this.DetectOperator.ModuleTestReadyStatus.StartAddress] == (ushort)Status.Valid;
            }

        }



        public bool SetModuleQRCompleted()
        {
            this.DetectOperator.ModuleQRCompleted.Datas[0] = (ushort)Status.Completed;

            this.WriteChannel.WriteModbusItem(this.DetectOperator.ModuleQRCompleted);
            return (!this.WriteChannel.HasError);
        }

        public bool SetModuleTypeOCRCompleted()
        {
            this.DetectOperator.ModuleTypeOCRCompleted.Datas[0] = (ushort)Status.Completed;

            this.WriteChannel.WriteModbusItem(this.DetectOperator.ModuleTypeOCRCompleted);
            return (!this.WriteChannel.HasError);
        }


        public bool SetModuleOCRLightingCompleted()
        {
            this.DetectOperator.ModuleOCRLightingCompleted.Datas[0] = (ushort)Status.Completed;
            this.WriteChannel.WriteModbusItem(this.DetectOperator.ModuleOCRLightingCompleted);
            return (!this.WriteChannel.HasError);
        }


        public bool SetModuleTestResult(Status result)
        {
            this.DetectOperator.TestResult.Datas[0] = (ushort)result;
            this.WriteChannel.WriteModbusItem(this.DetectOperator.TestResult);
            return (!this.WriteChannel.HasError);
        }


        #endregion



    }
}
