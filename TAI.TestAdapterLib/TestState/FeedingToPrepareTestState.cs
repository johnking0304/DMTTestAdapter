using System;
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
        public bool TransferCompleted { get; set; }
        public FeedingToPrepareTestState(TestAdapter manager,Module module) : base(manager)
        {
            this.ActiveModule = module;
            this.Caption = "上料状态-设备预热工位上料步骤";
            this.TestingState = TestingState.FeedingToPrepare;
            this.TransferCompleted = false;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入设备预热工位上料步骤";
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
                            LogHelper.LogInfoMsg(string.Format("机械手已到达工位[{0}]，预热工位上料完成",
                                this.ActiveModule.TargetPosition.ToString()));
                            this.TransferCompleted = true;
                            this.Manager.StartModulePrepare(this.ActiveModule);  
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
                        }
                    }

                }         
            }
            this.StateCheck();
        }


        public override void StateCheck()
        {
            if (this.TransferCompleted)
            {
                this.Manager.TestState = new PreFeedingTestState(this.Manager);
            }

        }
    }
}
