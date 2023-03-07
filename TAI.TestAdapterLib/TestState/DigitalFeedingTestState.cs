using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace DMTTestAdapter
{
    public class DigitalFeedingTestState : TestState
    {
        
        public DigitalFeedingTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "上料状态-数字量模块上料步骤";
            this.TestingState = TestingState.Feeding;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入数字量模块上料步骤";
            LogHelper.LogInfoMsg(this.LastMessage);
        }

        public override void Execute()
        {
           
        }

        public override void StateCheck()
        {
            if (this.Manager.Command == OperateCommand.StartTest)
            {

            }

        }
    }
}
