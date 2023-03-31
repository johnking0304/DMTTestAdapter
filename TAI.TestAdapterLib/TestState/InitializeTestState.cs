using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace DMTTestAdapter
{
    public class InitializeTestState: TestState
    {
        
        public InitializeTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "系统启动状态";
            this.TestingState = TestingState.Initial;
        }

        public override void Initialize()
        {
            this.LastMessage = "【系统启动状态】，等待初始化";
            LogHelper.LogInfoMsg(this.LastMessage);
        }

        public override void StateCheck()
        {
            if (this.Manager.InitializeCompleted)
            {
                this.LastMessage = "系统初始化完成";
                LogHelper.LogInfoMsg(this.LastMessage);

                if (this.Manager.SystemMessage.LastErrorCode == (int)SystemCode.Ok)
                {
                    this.Manager.TestState = new IdleTestState(this.Manager);
                }
                else
                {
                    this.Manager.TestState = new FaultTestState(this.Manager);
                }               
            }
        }
    }
}
