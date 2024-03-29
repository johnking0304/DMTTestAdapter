﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;
using TAI.Modules;

namespace DMTTestAdapter
{
    public class FeedingToPrepareTestState : TestState
    {
        public Module ActiveModule { get; set; }

        public bool WaitForStationStarted { get; set; }
        public bool TransferCompleted { get; set; }
        public bool CaptureCompleted { get; set; }
        public FeedingToPrepareTestState(TestAdapter manager,Module module) : base(manager)
        {
            this.ActiveModule = module;
            this.Caption = "上料状态-设备预热工位上料步骤";
            this.TestingState = TestingState.FeedingToPrepare;
            this.TransferCompleted = false;
            this.CaptureCompleted = false;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入【设备预热工位上料】步骤";
            LogHelper.LogInfoMsg(this.LastMessage);

            // 
        }


        public override void Execute()
        {
            if (this.ActiveModule != null)
            {
                if (!this.TransferCompleted)
                {
                    //如果在移动状态，
                    if (this.RobotMoving)
                    {
                        //判断移动是否结束
                        if (this.Manager.ProcessController.RobotMoveCompleted)
                        {
                            this.RobotMoving = false;
                            LogHelper.LogInfoMsg(string.Format("机械手已到达工位[{0}]，预热工位上料完成",this.ActiveModule.TargetPosition.ToString()));
                            this.TransferCompleted = true;

                        }
                    }
                    else
                    {
                        if (this.Manager.ProcessController.RobotIdle)
                        {
                            this.Manager.ProcessController.SetRobotMoveParams(this.ActiveModule.CurrentPositionValue,
                                this.ActiveModule.TargetPositionValue, TAI.Manager.ActionMode.Transport);
                            this.Manager.ProcessController.SetRobotMoveEnable();
                            LogHelper.LogInfoMsg(string.Format("开始上料到预热工位动作"));
                            this.ActiveModule.TestStep = TestStep.Feeding;
                            this.RobotMoving = true;

                            this.Manager.VISController.SwitchProgramToQRModelSerialCode();
                            LogHelper.LogInfoMsg(string.Format("开始切换到[模块二维码识别]程序块[{0}]",this.Manager.VISController.ProgramId));
                        }
                    }

                }

                if (!this.CaptureCompleted && this.TransferCompleted)
                {
                    this.RobotMoving = false;

                    string serialCode = "";
                    //尝试3次
                    if (this.Manager.VISController.TryQRModelSerialCode(ref serialCode))
                    {
                        serialCode = serialCode.Replace(',', '|');
                        this.ActiveModule.SerialCode = serialCode;
                        LogHelper.LogInfoMsg(string.Format("待测模块型号[{0}]识别完成-二维码信息[{1}]", this.ActiveModule.ModuleType, serialCode));

                    }
                    else
                    {
                        this.ActiveModule.SerialCode = "";
                        LogHelper.LogInfoMsg(string.Format("待测模块型号[{0}]识别失败", this.ActiveModule.ModuleType));
                    }

                    //二维码识别完成信号
                    this.Delay(1000);
                    LogHelper.LogInfoMsg(string.Format("延迟1秒"));

                    this.Manager.ProcessController.SetModuleQRCompleted();
                    LogHelper.LogInfoMsg(string.Format("使能PLC模块二维码识别完成信号"));

                    //关联预热工位  模块

                    LogHelper.LogInfoMsg(string.Format("设置工位[{0}]关联模块[{1}]",
                                                        StationType.Prepare.Description(),
                                                        this.ActiveModule.ModuleType.Description()));
                    this.Manager.PrepareStation.LinkedModule = this.ActiveModule;
                    this.Manager.PrepareStation.TestStep = TestStep.Ready;
                    this.CaptureCompleted = true;
                    LogHelper.LogInfoMsg(string.Format("待测模块[{0}]上料完成,等待下发预热测试命令", this.ActiveModule.ModuleType));

                }


            }
            this.StateCheck();
        }


        public override void StateCheck()
        {

            if (this.CaptureCompleted && this.TransferCompleted)
            {
                if (this.Manager.Command == OperateCommand.StartStationTest)
                {
                    this.Manager.Command = OperateCommand.None;
                    this.LastMessage = string.Format("模块[{0}]预热开始", this.ActiveModule.Description);
                    this.Manager.StartModulePrepare(this.ActiveModule);
                    //工位下压动作
                    this.Manager.ProcessController.StartStationTest((int)StationType.Prepare);
                    this.Delay(1000);

                    this.WaitForStationStarted = true;

                }
                else if (this.Manager.Command == OperateCommand.StopStationTest)
                {
                    if (this.Manager.PrepareStation.WaitToBlanking)
                    {
                        this.Manager.Command = OperateCommand.None;
                        this.LastMessage = string.Format("模块[{0}]预热取消，转换到【工位下料状态】", this.ActiveModule.Description);
                        LogHelper.LogInfoMsg(this.LastMessage);
                        this.ActiveModule.CurrentPosition = Position.Prepare;
                        this.ActiveModule.TargetPosition = Position.Out_NG;
                        this.Manager.PrepareStation.LinkedModule = null;

                        this.Manager.ProcessController.SetModuleTestResult(false);
                        this.Manager.TestState = new BlankingTestState(this.Manager, this.ActiveModule);
                        return;
                    }
                }


                if (this.WaitForStationStarted)
                {
                    if (this.Manager.ProcessController.StationInTesting)
                    {
                        this.WaitForStationStarted = false;
                        this.LastMessage = string.Format("模块[{0}]预热已开始，转换到【总调度状态】", this.ActiveModule.Description);
                        LogHelper.LogInfoMsg(this.LastMessage);

                        this.Manager.TestState = new DispatchTestState(this.Manager);
                        return;
                    }
                }
            }

        }
    }
}
