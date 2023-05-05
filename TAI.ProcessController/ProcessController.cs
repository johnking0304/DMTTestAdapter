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

        public  ushort[] SystemStatusValues { get; set; }



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
            //this.InitializeSystem();
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
                ushort[] data = this.ReadChannel.ReadModbusItem(item);
                if (!this.ReadChannel.HasError)
                {
                    return data[0] == (ushort)Status.Busy;
                }
                return true;
            }
            return true;
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


        public bool NotifyFeedLackSignal()
        {
            LogHelper.LogInfoMsg(string.Format("下料完成,上料盘已空，下发上料盘已空信号"));

            this.SystemOperator.FeedLackSignal.Datas[0] = 1;
            this.WriteChannel.WriteModbusItem(this.SystemOperator.FeedLackSignal);
            return (!this.WriteChannel.HasError);

        }


        public bool NotifyStartTest()
        {
            this.DetectOperator.StartTest.Datas[0] = 1;
            this.WriteChannel.WriteModbusItem(this.DetectOperator.StartTest);
            return (!this.WriteChannel.HasError);
        }


        public bool NotifyFeedingBoxMapValue(ushort value)
        {
            //LogHelper.LogInfoMsg(string.Format("下发上料盘状态[{0}]",value));
            this.DetectOperator.FeedingBoxMapValue.Datas[0] = value;
            this.WriteChannel.WriteModbusItem(this.DetectOperator.FeedingBoxMapValue);
            return (!this.WriteChannel.HasError);

        }

        public bool InitializeCompleted
        {
            get
            {
                bool result = false;
                ushort[] data = this.ReadChannel.ReadModbusItem(this.SystemOperator.InitializeCompleted);
                if (!this.ReadChannel.HasError)
                {
                    result = data[0] == (ushort)Status.Completed;
                    if (result)
                    {
                        this.SystemOperator.InitializeCompleted.Datas[0] = (ushort)0;
                        this.WriteChannel.WriteModbusItem(this.SystemOperator.InitializeCompleted);
                    }                   
                }
                return result;
            }
        }


        /// <summary>
        /// 初始化流控模块
        /// </summary>
        /// <returns></returns>

        public bool InitializeSystem()
        {

            LogHelper.LogInfoMsg("初始化系统");
           
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

                bool result = false;
                ushort[] data = this.ReadChannel.ReadModbusItem(this.SystemOperator.SystemStop);
                if (!this.ReadChannel.HasError)
                {
                    result = data[0] == (ushort)Status.Valid;
                    if (result)
                    {
                        this.SystemOperator.SystemStop.Datas[0] = (ushort)0;
                        this.WriteChannel.WriteModbusItem(this.SystemOperator.SystemStop);
                    }
                }
                return result;


            }
        }

        public bool SystemStarted
        {
            get
            {
                bool result = false;
                ushort[] data = this.ReadChannel.ReadModbusItem(this.SystemOperator.SystemStart);
                if (!this.ReadChannel.HasError)
                {
                    result = data[0] == (ushort)Status.Valid;
                    if (result)
                    {
                        this.SystemOperator.SystemStart.Datas[0] = (ushort)0;
                        this.WriteChannel.WriteModbusItem(this.SystemOperator.SystemStart);
                    }
                }
                return result;
            }
        }


        public DeviceType TestDeviceType
        {
            get
            {
                return (DeviceType)this.SystemStatusValues[this.SystemOperator.TestDeviceType.StartAddress];
            }
        }
        public bool NewFeedSignal
        {
            get
            {

                bool result = false;
                ushort[] data = this.ReadChannel.ReadModbusItem(this.SystemOperator.NewFeedSignal);
                if (!this.ReadChannel.HasError)
                {
                    result = data[0] == (ushort)Status.Valid;
                    if (result)
                    {
                        this.SystemOperator.NewFeedSignal.Datas[0] = (ushort)0;
                        this.WriteChannel.WriteModbusItem(this.SystemOperator.NewFeedSignal);
                    }
                }
                return result;

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
                ushort[] data = this.ReadChannel.ReadModbusItem(this.RobotOperator.GetIdleStatus);
                if (!this.ReadChannel.HasError)
                {
                    return data[0] == (ushort)Status.Valid;
                }
                else
                {
                    return false;
                }
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
            this.DetectOperator.StartStationTest.Datas[0] = 0;  //启动 OK =1  NG=2

            this.WriteChannel.WriteModbusItem(this.DetectOperator.StartStationTest);

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
                /*              if (this.SystemStatusValues[this.RobotOperator.MoveCompletedStatus.StartAddress] == (ushort)Status.Completed)
                                {
                                    this.SystemStatusValues[this.RobotOperator.MoveCompletedStatus.StartAddress] == 0;
                                    this.RobotOperator.MoveCompletedStatus.Datas[0] = (ushort)0;
                                    this.WriteChannel.WriteModbusItem(this.RobotOperator.MoveCompletedStatus);
                                    return true;
                                }
                                return false;*/
                ushort[] data  =this.ReadChannel.ReadModbusItem(this.RobotOperator.MoveCompletedStatus);
                if (!this.ReadChannel.HasError)
                {
                    return data[0] == (ushort)Status.Completed;
                }
                else
                {
                    return false;
                }
                    

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


        public bool SetModuleTestResult(bool result)
        {
            this.DetectOperator.TestResult.Datas[0] = (ushort)(result?1:2);
            this.WriteChannel.WriteModbusItem(this.DetectOperator.TestResult);
            return (!this.WriteChannel.HasError);
        }


        #endregion



    }
}
