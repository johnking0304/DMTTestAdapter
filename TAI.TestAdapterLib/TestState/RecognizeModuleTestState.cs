using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;
using TAI.Modules;

namespace DMTTestAdapter
{
    public class RecognizeModuleTestState : TestState
    {
        public int FeedIndex { get; set; }

        public List<Module> Modules {get;set;}
        public RecognizeModuleTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "上料状态-模块识别步骤";
            this.TestingState = TestingState.Feeding;
            this.FeedIndex = 0;
            this.Modules = new List<Module>();
            for (int i = 1; i <= TestAdapter.FeedCountMax; i++)
            {
                Module module = new Module(i);
                this.Modules.Add(module);
            }
        }

        private Module ActiveModule
        {
            get { 
                if((this.FeedIndex>=0) && (this.FeedIndex< TestAdapter.FeedCountMax))
                {
                    return this.Modules[this.FeedIndex];
                }
                return null;
            
            }
        }

        public override void Initialize()
        {
            this.LastMessage = "进入模块识别";
            LogHelper.LogInfoMsg(this.LastMessage);
            
        }

        public override void Execute()
        {
            if (this.ActiveModule != null)
            {

                //模块识别

            }
           
        }
   

        public override void StateCheck()
        {
            if (this.ActiveModule == null)
            {
                //模块识别完成
                this.LastMessage = "模块识别完成，转换为系统上料状态";
                LogHelper.LogInfoMsg(this.LastMessage);

                foreach (Module module in this.Modules)
                {
                    this.Manager.TestingModules.Add(module.Clone());
                }
                this.Manager.TestState = new FeedingTestState(this.Manager);

            }
        }
    }
}
