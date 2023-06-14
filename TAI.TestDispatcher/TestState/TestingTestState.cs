using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;
using DMT.Core.Utils;


namespace TAI.TestDispatcher
{
    public class TestingTestState : TestState
    {


        public TestingTestState(TestDispatcher dispatcher) : base(dispatcher)
        {
            this.Caption = "测试状态";
            this.TestingState = TestingState.Testing;
            
            this.LastMessage = string.Format("进入【测试状态】，等待测试参数");
            LogHelper.LogInfoMsg(this.LastMessage);

            this.Dispatcher.Notify((int)NotifyEvents.Progress, this.TestingState.ToString(), "", this.Dispatcher.CardModule, this.LastMessage);

        }

        public override void Execute()
        {
            this.Dispatcher.ProcessAction();                                                  
           
            this.StateCheck();
        }
        public override void StateCheck()
        {
            if (this.ActiveCommand == OperateCommand.StopTest)
            {
                this.ActiveCommand = OperateCommand.None;
                this.LastMessage = string.Format("停止测试，转换到【结束状态】");
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Dispatcher.NotifyMessage(this.LastMessage);

                this.Dispatcher.TestState = new FinishTestState(this.Dispatcher);
                return;
            }else if (this.ActiveCommand == OperateCommand.PaseTest)
            {
                this.ActiveCommand = OperateCommand.None;
                this.LastMessage = string.Format("暂停测试，转换到【暂停状态】");
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Dispatcher.NotifyMessage(this.LastMessage);

                this.Dispatcher.TestState = new PauseTestState(this.Dispatcher);
                return;
            }

            if (this.Dispatcher.TestCompleted)
            {
                this.LastMessage = string.Format("测试完成，转换到【结束状态】");
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Dispatcher.NotifyMessage(this.LastMessage);

                this.Dispatcher.TestState = new FinishTestState(this.Dispatcher);
                return;
            }




        }
    }
}
