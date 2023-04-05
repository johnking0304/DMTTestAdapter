using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMTTestAdapter
{
    public enum TestingState
    {
        [Description("系统启动状态")]
        Initial = 0,//系统启动状态   
        [Description("空闲状态")]
        Idle = 1, //空闲状态
        [Description("模块识别状态")]
        Recognize = 2,//模块识别状态
        [Description("预上料状态-总调度状态")]
        PreFeeding = 3,//预上料状态
        [Description("上料到预热工位状态")]
        FeedingToPrepare = 4,//上料到预热工位状态
        [Description("上料到测试工位状态")]
        FeedingToTest = 5,//上料到测试工位状态
        [Description("测试状态")]
        Testing = 6,//测试状态
        [Description("下料状态")]
        Blanking =7,//下料状态
        [Description("暂停状态")]
        Pause = 8,  //暂停状态
        [Description("故障状态")]
        Fault = 9, //故障状态
    }


    public abstract class BaseTestState
    {
        public abstract void Initialize();
        public abstract void Execute();
        public abstract void StateCheck();
    }



    public class TestState : BaseTestState
    {
        protected TestAdapter Manager;

        public bool RobotMoving { get; set; }

        public bool PrepareCompleted { get; set; }

        public TestingState TestingState { get; set; }

        public int WaitMilliseconds { get; set; }

        public string Caption { get; set; }

  
        public string LastMessage { get; set; }


        public TestState(TestAdapter manager)
        {
            this.Manager = manager;
            this.RobotMoving = false;
            this.PrepareCompleted = false;
            this.Initialize();
        }

        public void Delay(int milliseconds)
        {
            DateTime now = DateTime.Now;
            Boolean inTime = true;
            while (inTime)
            {
                TimeSpan value = DateTime.Now - now;
                inTime = value.TotalMilliseconds < milliseconds;
            }
            return;
        }


        public bool TimeOut(DateTime datetime)
        {
            TimeSpan span = DateTime.Now - datetime;
            if (span.TotalMilliseconds >= this.WaitMilliseconds)
            {
                datetime = DateTime.Now;
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
            return;
        }

        public override void Execute()
        {
            this.StateCheck();
        }

        public override void StateCheck()
        {
            return;
        }
    }
}
