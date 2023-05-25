using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace TAI.TestDispatcher
{
    public class FinishTestState: TestState
    {
        public bool Operated { get; set; }
        
        public FinishTestState(TestDispatcher dispatcher) : base(dispatcher)
        {
            this.Caption = "结束状态";
            this.TestingState = TestingState.Finish;
            this.Operated = false;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入【结束状态】";
            LogHelper.LogInfoMsg(this.LastMessage);
        }

        public override void Execute()
        {
            //处理报告


            this.StateCheck();
        }
        public override void StateCheck()
        {
            if (this.Operated)
            {
                this.Dispatcher.StopThread();
            }
        }
    }
}
