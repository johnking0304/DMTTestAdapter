using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;
using TAI.Modules;

namespace DMTTestAdapter
{
    
    public class DispatchTestState : TestState
    {
        
        public DispatchTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "系统总调度状态";
            this.TestingState = TestingState.PreFeeding;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入【系统总调度状态】";
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
                if (!this.Manager.ProcessController.StationIsBusy((StationType)((int)(this.Manager.PrepareStation.LinkedModule.ModuleType))))
                {
                    this.LastMessage = string.Format("预热工位模块[{0}]预热已完成,且对应测试工位空闲，切换到【预热工位搬运到测试工位】步骤", this.Manager.PrepareStation.LinkedModule.ModuleType.ToString());
                    LogHelper.LogInfoMsg(this.LastMessage);
                    this.Manager.TestState = new FeedingToTestTestState(this.Manager, this.Manager.PrepareStation.LinkedModule, true);
                    return;
                }  
            }


            if (this.Manager.ModuleIdle)
            {
                //查看整体上料产品中是否有需要预热的模块，
                Module module = this.Manager.ModuleNeedPrepare;
                if ((module!=null)  && !this.Manager.ProcessController.StationIsBusy(StationType.Prepare))
                {

                    //预热工位是否空闲
                    this.LastMessage = "找到需要预热的模块为空闲状态";
                    LogHelper.LogInfoMsg(this.LastMessage);
                    
                    this.LastMessage = "预热工位空闲，可以执行预热工位上料动作";
                    LogHelper.LogInfoMsg(this.LastMessage);
                    //进入模拟量模块预热工位上料流程
                    module.TargetPosition = Position.Prepare;
                    this.Manager.TestState = new FeedingToPrepareTestState(this.Manager,module);
                    return;
                    
                }
                else 
                {
                    module = this.Manager.ModuleNeedFeed;
                    //进入数字量模块上料流程
                    if (module != null  && !this.Manager.ProcessController.StationIsBusy((StationType)module.StationId))
                    {
                        this.Manager.TestState = new FeedingToTestTestState(this.Manager, module,false);
                    }
                }
            }
 
            //先响应下料请求
            if (this.Manager.ProcessController.RobotIdle)
            {
                Station station = this.Manager.StationWaitToBlanking;
                if (station != null  && station.LinkedModule!=null)
                {
                    this.LastMessage = string.Format("模块[{0}]测试等待下料，转换到【工位下料状态】", station.LinkedModule.Description);
                    LogHelper.LogInfoMsg(this.LastMessage);
                    station.LinkedModule.CurrentPosition = station.TestPosition;
                    this.Manager.ProcessController.SetModuleTestResult(station.LinkedModule.Conclusion);                    
                    this.Manager.TestState = new BlankingTestState(this.Manager, station.LinkedModule);
                }
            }

            //响应灯测
            if (this.Manager.RequestCommand == OperateCommand.RequestVISLighting)
            {
                this.Manager.RequestCommand = OperateCommand.None;
                int stationId = this.Manager.ActiveStation;
                Module module = this.Manager.Stations[stationId - 1].LinkedModule;
                if (module != null)
                {
                    this.LastMessage = string.Format("工位[{0}]灯测请求,进入工位测试状态", module.Description);
                    LogHelper.LogInfoMsg(this.LastMessage);
                    this.Manager.TestState = new ModuleTestingTestState(this.Manager, module);
                }
            }
        }
    }
}
