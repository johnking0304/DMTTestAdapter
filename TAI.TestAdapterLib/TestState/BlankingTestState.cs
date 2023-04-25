using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;
using TAI.Modules;

namespace DMTTestAdapter
{
    public class BlankingTestState: TestState
    {
        private Module ActiveModule { get; set; }

        private bool BlankCompleted { get; set; }

        public BlankingTestState(TestAdapter manager, Module module) : base(manager)
        {
            this.Caption = "下料状态";
            this.ActiveModule = module;
            this.ActiveModule.TestStep = TestStep.Blanking;
            this.TestingState = TestingState.Blanking;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入【工位下料状态】";
            LogHelper.LogInfoMsg(this.LastMessage);
        }

        public override void Execute()
        {
            if (this.ActiveModule != null)
            {
                //如果在移动状态，
                if (this.RobotMoving)
                {
                    //判断移动是否结束
                    if (this.Manager.ProcessController.RobotMoveCompleted)
                    {
                        this.RobotMoving = false;                       
                        this.BlankCompleted = true;
                    }
                }
                else
                {
                    //if (this.Manager.ProcessController.RobotIdle)
                    {
                        this.Manager.ProcessController.SetRobotMoveParams(this.ActiveModule.CurrentPositionValue, this.ActiveModule.TargetPositionValue, TAI.Manager.ActionMode.Blanking);
                        this.Manager.ProcessController.SetRobotMoveEnable();
                        LogHelper.LogInfoMsg(string.Format("开始模块[{0}]下料步骤", this.ActiveModule.Description));
                        this.RobotMoving = true;
                    }
                }
            }

            this.StateCheck();
        }

        public override void StateCheck()
        {
            if (this.BlankCompleted)
            {
                this.Manager.StartModuleBlank(this.ActiveModule);
                LogHelper.LogInfoMsg(string.Format("模块已到达位置[{0}]，下料完成", this.ActiveModule.TargetPosition.ToString()));


                if (this.Manager.ModuleCount == 0)
                {
                    this.Manager.ProcessController.NotifyFeedLackSignal();
                }

                this.Manager.TestState = new DispatchTestState(this.Manager);
            }
        }
    }
}
