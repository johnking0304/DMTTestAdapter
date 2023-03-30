using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;
using TAI.Modules;

namespace DMTTestAdapter
{
    public class FeedingToTestTestState : TestState
    {
        public bool TransferCompleted { get; set; }
        public bool CaptureCompleted { get; set; }
        public Module ActiveModule { get; set; }
        public FeedingToTestTestState(TestAdapter manager,Module module) : base(manager)
        {
            this.Caption = "上料状态-模块上料步骤";
            this.TestingState = TestingState.FeedingToTest;
            this.TransferCompleted = false;
            this.CaptureCompleted = false;
            this.ActiveModule = module;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入模块上料步骤";
            LogHelper.LogInfoMsg(this.LastMessage);
            
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
                            LogHelper.LogInfoMsg(string.Format("机械手已到达工位[{0}]，开始识别模块二维码", 
                                this.ActiveModule.TargetPosition.ToString()));
                            this.TransferCompleted = true;
                            this.ActiveModule.LinkStation.LinkedModule = this.ActiveModule;

                        }
                    }
                    else
                    {
                        if (this.Manager.ProcessController.RobotIdle)
                        {
                            this.Manager.ProcessController.SetRobotMoveParams(this.ActiveModule.CurrentPositionValue,
                                this.ActiveModule.TargetPositionValue, TAI.Manager.ActionMode.Transport);
                            this.Manager.ProcessController.SetRobotMoveEnable();
                            LogHelper.LogInfoMsg(string.Format("开始上料动作"));
                            this.ActiveModule.TestStep = TestStep.Feeding;
                            this.RobotMoving = true;
                        }
                    }

                }

                if (!this.CaptureCompleted && this.TransferCompleted)
                {
                    //如果在移动状态，
                    if (this.RobotMoving)
                    {
                        //判断移动是否结束
                        if (this.Manager.ProcessController.RobotMoveCompleted)
                        {
                            this.RobotMoving = false;
                            LogHelper.LogInfoMsg(string.Format("机械手已到达工位[{0}]，开始识别模块二维码", this.ActiveModule.TargetPosition.ToString()));

                            string serialCode = "";
                            //FIXME 尝试3次
                            if (this.Manager.VISController.QRModelSerialCode(ref serialCode))
                            {
                                this.ActiveModule.SerialCode = serialCode;
                                LogHelper.LogInfoMsg(string.Format("待测模块型号[{0}]识别完成-二维码信息[{1}]", this.ActiveModule.ModuleType, serialCode));
                                //二维码识别完成信号
                                this.Manager.ProcessController.SetModuleQRCompleted();
                                LogHelper.LogInfoMsg(string.Format("使能PLC模块二维码识别完成信号"));

                            }
                            else
                            {
                                this.ActiveModule.SerialCode = "";
                                LogHelper.LogInfoMsg(string.Format("待测模块型号[{0}]识别失败", this.ActiveModule.ModuleType));
                            }
                            this.ActiveModule.TestStep = TestStep.Ready;
                            this.CaptureCompleted = true;
                            LogHelper.LogInfoMsg(string.Format("待测模块[{0}]上料完成,等待下发测试启动命令", this.ActiveModule.ModuleType));
                        }
                    }
                    else
                    {
                        if (this.Manager.ProcessController.RobotIdle)
                        {
                            this.Manager.ProcessController.SetRobotMoveParams(this.ActiveModule.TargetPositionValue,
                                this.ActiveModule.QRPositionValue, TAI.Manager.ActionMode.Capture);
                            this.Manager.ProcessController.SetRobotMoveEnable();
                            LogHelper.LogInfoMsg(string.Format("移动到二维码拍摄位置"));
                            this.ActiveModule.TestStep = TestStep.Feeding;
                            this.RobotMoving = true;
                        }
                    }

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
                    this.Manager.StartModuleTest(this.ActiveModule);
                    this.Manager.TestState = new ModuleTestingTestState(this.Manager,this.ActiveModule);
                }
            }

        }
    }
}
