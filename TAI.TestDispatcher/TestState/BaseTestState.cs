using DMT.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAI.TestDispatcher
{
    public enum TestingState
    {
        [Description("空闲状态")]
        Idle = 1, 
        [Description("测试状态")]
        Testing = 2,
        [Description("暂停状态")]
        Pause = 3,
        [Description("结束状态")]
        Finish = 4,
        [Description("故障状态")]
        Fault = 5, 
    }


    public abstract class BaseTestState
    {
        public abstract void Initialize();
        public abstract void Execute();
        public abstract void StateCheck();
    }



    public class TestState : BaseTestState
    {
        protected TestDispatcher Dispatcher;

        public TestingState TestingState { get; set; }

        public int WaitMilliseconds { get; set; }

        private DateTime StartDateTime;

        public OperateCommand ActiveCommand { get; set; }
        public string Caption { get; set; }

 
        public string LastMessage { get; set; }


        public TestState(TestDispatcher dispatcher)
        {
            this.Dispatcher = dispatcher;
            this.StartDateTime = DateTime.Now;
            this.ActiveCommand = OperateCommand.None;
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


        public bool TimeOut(ref DateTime datetime)
        {
            TimeSpan span = DateTime.Now - datetime;
            if (span.TotalMilliseconds >= this.WaitMilliseconds)
            {
                datetime = DateTime.Now;
                return true;
            }
            return false;
        }

        public bool TimeOut()
        {
            return this.TimeOut(ref this.StartDateTime);
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
