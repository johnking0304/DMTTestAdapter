using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;
using TAI.Modules;

namespace DMTTestAdapter
{
    public class ModuleTestingTestState: TestState
    {
        public Module ActiveModule { get; set; }
        
        public ModuleTestingTestState(TestAdapter manager,Module module) : base(manager)
        {
            this.Caption = "模块测试状态";
            this.TestingState = TestingState.Testing;
            this.ActiveModule = module;
        }

        public override void Initialize()
        {
            this.LastMessage = string.Format("进入模块[{0}]测试状态，等待下发测试参数", this.ActiveModule.Description);
            LogHelper.LogInfoMsg(this.LastMessage);
        }


        public override void StateCheck()
        {
            if (this.Manager.Command == OperateCommand.StopStationTest)
            {
                this.Manager.Command = OperateCommand.None;
                this.LastMessage = string.Format("模块[{0}]测试完成，转换到工位下料状态", this.ActiveModule.Description);
                LogHelper.LogInfoMsg(this.LastMessage);

                this.Manager.TestState = new BlankingTestState(this.Manager,this.ActiveModule);

            }
        }
    }
}
