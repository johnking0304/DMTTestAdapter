using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace DMTTestAdapter
{
    public class AnalogTestingTestState : TestState
    {
        
        public AnalogTestingTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "模拟量模块测试状态";
            this.TestingState = TestingState.Testing;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入模拟量模块测试状态，等待下发测试参数";
            LogHelper.LogInfoMsg(this.LastMessage);
        }


        public override void Execute()
        {

        }

        public override void StateCheck()
        {

        }
    }
}
