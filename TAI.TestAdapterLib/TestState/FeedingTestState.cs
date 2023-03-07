using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace DMTTestAdapter
{
    public class FeedingTestState: TestState
    {
        
        public FeedingTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "系统上料状态";
            this.TestingState = TestingState.Idle;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入系统上料状态";
            LogHelper.LogInfoMsg(this.LastMessage);
        }

        public override void StateCheck()
        {
            //如果为有重新上料 进入上料识别状态  Module

            //查看整体上料产品中是否有需要预热的模块，如果无，进入数字量模块上料流程
            //如果有进入模拟量模块上料流程




        }
    }
}
