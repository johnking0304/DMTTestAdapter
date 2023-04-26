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
        public bool SendInitialize { get; set; }
        private int SendCount { get; set; }
        private bool Auto { get; set; }
        public InitializeTestState(TestAdapter manager,bool auto=true) : base(manager)
        {
            this.Caption = "系统启动状态";
            this.TestingState = TestingState.Initial;
            this.WaitMilliseconds = 5000;
            this.SendCount = 0;
            this.SendInitialize = false;
            this.Auto = auto;
        }

        public override void Initialize()
        {
            this.LastMessage = string.Format("【系统启动状态】，初始化系统[{0}]",this.Auto?"自动":"手动");
            LogHelper.LogInfoMsg(this.LastMessage);

            this.Manager.Initialize();
        }

        public override void Execute()
        {
            if (!this.SendInitialize && this.TimeOut() && !this.Manager.InitializeCompleted) 
            {               
                this.SendInitialize = this.Manager.ProcessController.InitializeSystem();
                this.SendCount += 1;
                LogHelper.LogInfoMsg(string.Format("[流程控制PLC]发送系统初始化命令[{0}],发送次数[{1}]", this.SendInitialize?"成功":"失败",this.SendCount));
            }

            this.StateCheck();
        }

        public override void StateCheck()
        {
            this.Manager.InitializeCompleted = this.Manager.ProcessController.InitializeCompleted;

            if (this.Manager.InitializeCompleted)
            {
                this.LastMessage = "系统初始化完成!";
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
