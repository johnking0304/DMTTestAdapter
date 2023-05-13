using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;
using TAI.Modules;

namespace DMTTestAdapter
{
    public class ModuleTestingTestState : TestState
    {
        public Module ActiveModule { get; set; }
        public bool PreparedForOCRLighting { get; set; }

        public ModuleTestingTestState(TestAdapter manager, Module module) : base(manager)
        {
            this.Caption = "模块测试状态";
            this.TestingState = TestingState.Testing;
            this.ActiveModule = module;
            this.PreparedForOCRLighting = false;
            this.LastMessage = string.Format("进入模块[{0}]【测试状态】，等待下发测试参数", this.ActiveModule.Description);
            LogHelper.LogInfoMsg(this.LastMessage);
        }

        public override void Initialize()
        {
            
            
        }

        public override void Execute()
        {

            //灯测位置移动，

            //如果不在移动状态，
            if ((!this.RobotMoving) && (!this.PreparedForOCRLighting))
            {
                if (this.Manager.ProcessController.RobotIdle)
                {
                    this.Manager.ProcessController.SetRobotMoveParams((int)Position.Origin, (int)this.ActiveModule.LinkStation.TestPosition, TAI.Manager.ActionMode.Lighting);
                    this.Manager.ProcessController.SetRobotMoveEnable();
                    LogHelper.LogInfoMsg(string.Format("移动机械手到灯测位置", (int)this.ActiveModule.LinkStation.TestPosition));
                    this.RobotMoving = true;
                }
            }
            else
            {   //判断移动是否结束
                if ((this.Manager.ProcessController.RobotMoveCompleted) && (!this.PreparedForOCRLighting))
                {
                    this.RobotMoving = false;
                    this.PreparedForOCRLighting = true;
                    LogHelper.LogInfoMsg(string.Format("机械手已到达位置,等待模块测试和灯测驱动"));
                    this.PrepareCompleted = true;
                }
            }

            //灯测完成后需要上端给信号，使能机械手空闲

            this.StateCheck();
        }
        public override void StateCheck()
        {
            if (this.Manager.Command == OperateCommand.StopStationTest  && this.ActiveModule.LinkStation.WaitToBlanking)
            {
                this.Manager.Command = OperateCommand.None;
                this.LastMessage = string.Format("模块[{0}]测试完成，转换到【工位下料状态】", this.ActiveModule.Description);
                LogHelper.LogInfoMsg(this.LastMessage);
                this.ActiveModule.CurrentPosition = this.ActiveModule.LinkStation.TestPosition;
                
                this.Manager.TestState = new BlankingTestState(this.Manager, this.ActiveModule);
            }
            //暂时关闭 响应释放灯测
/*            if (this.Manager.ReleaseCommand == OperateCommand.ReleaseVISLighting && this.PreparedForOCRLighting)
            {
                this.Manager.ReleaseCommand = OperateCommand.None;
                this.Manager.ProcessController.SetModuleOCRLightingCompleted();
                this.LastMessage = string.Format("模块[{0}]测试工位释放灯测，转换到【总调度状态】", this.ActiveModule.Description);
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Manager.TestState = new DispatchTestState(this.Manager);
            }*/

        }
    }
}
