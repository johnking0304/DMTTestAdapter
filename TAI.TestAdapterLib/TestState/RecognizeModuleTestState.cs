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
            this.TestingState = TestingState.Recognize;
            this.FeedIndex = 0;
            this.Modules = new List<Module>();
            for (int i = 1; i <= TestAdapter.FeedCountMax; i++)
            {
                Module module = new Module(i);
                module.CurrentPosition = Position.FeedBase;
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
            this.LastMessage = string.Format("进入【上料状态】-模块识别步骤");
            LogHelper.LogInfoMsg(this.LastMessage);
            
        }
        private int TargetIndex {
            get  =>this.ActiveModule.CurrentPositionValue;              }

        private int StartIndex
        {
            get{
                return (int)Position.Origin;
            }
        }

        public void NextModule()
        {
            this.FeedIndex += 1;
        }
        public override void Execute()
        {
            if (this.ActiveModule != null)
            {
                //如果不在移动状态，
                if (!this.RobotMoving)
                {                 
                    if (this.Manager.ProcessController.RobotIdle)
                    {
                        this.Manager.ProcessController.SetRobotMoveParams(this.StartIndex, this.TargetIndex, TAI.Manager.ActionMode.Capture);
                        this.Manager.ProcessController.SetRobotMoveEnable();
                        LogHelper.LogInfoMsg(string.Format("开始识别模块[{0}]型号", this.TargetIndex));

                        this.RobotMoving = true;
                    }                   
                }
                else
                {
                    //判断移动是否结束
                    if (this.Manager.ProcessController.RobotMoveCompleted)
                    {
                        this.RobotMoving = false;
                        LogHelper.LogInfoMsg(string.Format("机械手已到达位置[{0}]，开始识别模块型号",this.TargetIndex));
                        string moduleType = "";

                        if (this.Manager.VISController.OCRModelType(ref moduleType))
                        {
                            ModuleType type = this.Manager.ParseModuleType(moduleType);
                            if (type != ModuleType.None)
                            {
                                this.ActiveModule.ModuleType = type;
                                this.ActiveModule.LinkStation = this.Manager.GetModuleStation(this.ActiveModule.ModuleType);
                                this.ActiveModule.Enable = true;
                                LogHelper.LogInfoMsg(string.Format("待测模块型号[{0}]识别完成-类型[{1}]", moduleType, this.ActiveModule.ModuleType));
                            }
                            else
                            {
                                LogHelper.LogInfoMsg(string.Format("待测模块型号[{0}]识别失败", moduleType));
                            }

                        }
                        else
                        {
                            this.ActiveModule.ModuleType = ModuleType.None;
                            LogHelper.LogInfoMsg(string.Format("待测模块型号[{0}]识别失败", moduleType));
                        }
                        //设置类型识别完成信号 
                        this.Manager.ProcessController.SetModuleTypeOCRCompleted();
                        LogHelper.LogInfoMsg(string.Format("使能PLC模块类型识别完成信号"));

                        this.NextModule();                          
                    }
                }
                
  
            }

            this.StateCheck();
           
        }
   

        public override void StateCheck()
        {
            if (this.ActiveModule == null)
            {
                //模块识别完成
                this.LastMessage = "模块识别完成，转换为【系统预上料状态】";
                LogHelper.LogInfoMsg(this.LastMessage);

                foreach (Module module in this.Modules)
                {
                    if (module.Enable)
                    {                       
                        this.Manager.TestingModules.Add(module.Clone());
                    }
                }
                this.Modules.Clear();
                this.Manager.TestState = new PreFeedingTestState(this.Manager);
            }
        }
    }
}
