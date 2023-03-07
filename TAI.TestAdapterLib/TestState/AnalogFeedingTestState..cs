using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace DMTTestAdapter
{
    public class AnalogFeedingTestState : TestState
    {
        
        public AnalogFeedingTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "上料状态-模拟量设备上料步骤";
            this.TestingState = TestingState.Feeding;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入模拟量设备上料步骤";
            LogHelper.LogInfoMsg(this.LastMessage);
        }


        public override void Execute()
        {

        }


        public override void StateCheck()
        {


            if (this.Manager.SystemMessage.LastErrorCode == (int)SystemCode.Fault)
            {
                this.Manager.TestState = new FaultTestState(this.Manager);
            }
        }
    }
}
