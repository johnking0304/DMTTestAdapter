using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMTTestAdapter
{
    public enum TestingState
    {   
        Initial = 0,//系统启动状态   

        Idle = 1, //空闲状态

        Feeding = 2,//上料状态
        
        Testing = 3,//测试状态

        Blanking =4,//下料状态

        Pause = 5,  //暂停状态
       
        Fault = 6, //故障状态
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

        public TestingState TestingState { get; set; }

        public int WaitMilliseconds { get; set; }

        public string Caption { get; set; }

        public string LastMessage { get; set; }

        public TestState(TestAdapter manager)
        {
            this.Manager = manager;
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
