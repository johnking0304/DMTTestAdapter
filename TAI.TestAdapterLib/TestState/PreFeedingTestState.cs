using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;
using TAI.Modules;

namespace DMTTestAdapter
{
    public class PreFeedingTestState: TestState
    {
        
        public PreFeedingTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "系统预上料状态";
            this.TestingState = TestingState.PreFeeding;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入系统预上料状态";
            LogHelper.LogInfoMsg(this.LastMessage);
        }

        public override void StateCheck()
        {
            //如果有重新上料 进入上料识别状态  Module   
            if (this.Manager.ProcessController.NewFeedSignal)
            {
                //复位上料信号
                this.Manager.TestState = new RecognizeModuleTestState(this.Manager);
                return;
            }

            //查看预热工位中是否已经完成预热
            if (this.Manager.ModulePrepareCompleted())
            {
                this.LastMessage = "预热工位模块预热已完成，切换到预热工位搬运到测试工位步骤";
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Manager.TestState = new FeedingToTestTestState(this.Manager,this.Manager.PrepareStation.LinkedModule);
                return;
            }


            if (this.Manager.ModuleIdle)
            {
                //查看整体上料产品中是否有需要预热的模块，
                Module module = this.Manager.ModuleNeedPrepare;
                if (module!=null)
                {
                    this.LastMessage = "找到需要预热的模块为空闲状态";
                    LogHelper.LogInfoMsg(this.LastMessage);
                    //预热工位是否空闲
                    if (!this.Manager.ProcessController.StationIsBusy(StationType.Prepare))
                    {
                        this.LastMessage = "预热工位空闲，可以执行预热工位上料动作";
                        LogHelper.LogInfoMsg(this.LastMessage);
                        //进入模拟量模块预热工位上料流程
                        module.TargetPosition = Position.Prepare;
                        this.Manager.TestState = new FeedingToPrepareTestState(this.Manager,module);
                        return;
                    }
                }
                else if (this.Manager.ModuleNeedFeed)
                {                 
                    //进入数字量模块上料流程
                    this.Manager.TestState = new FeedingToTestTestState(this.Manager,module);
                }
            }                           
        }
    }
}
