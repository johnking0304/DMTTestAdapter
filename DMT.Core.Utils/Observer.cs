using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DMT.Core.Utils
{
    /**/
    /// <summary>
    ///     在Observer Pattern(观察者模式)中,此类作为所有Subject(目标)的抽象基类
    /// 所有要充当Subject的类都继承于此类.
    ///     我们说此类作为模型,用于规划目标(即发布方)所产生的事件,及提供触发
    /// 事件的方法.
    ///     此抽象类无抽象方法,主要是为了不能实例化该类对象,确保模式完整性.
    ///     具体实施:
    ///     1.声明委托
    ///     2.声明委托类型事件
    ///     3.提供触发事件的方法
    /// </summary>
    public abstract class Subject
    {
        public Subject()
        {
        }
        /**/
        /// <summary>
        /// 声明一个委托,用于代理一系列"无返回"及"不带参"的自定义方法
        /// </summary>
        public delegate void NotifyEventHandler(int notifyEvent,string flag,string content,object result,string message,object sender);
        /**/
        /// <summary>
        /// 声明一个绑定于上行所定义的委托的事件
        /// </summary>
        public event NotifyEventHandler NotifyEvent;


        public abstract void ProcessResponse(int notifyEvent, string flag, string content, object result, string message, object sender);

        public void AttachObserver(NotifyEventHandler observer)
        {
            NotifyEvent += observer;
        }
        public void DetachObserver(NotifyEventHandler observer)
        {
            
            NotifyEvent -= observer;
        }


        /**/
        /// <summary>
        /// 封装了触发事件的方法
        /// 主要为了规范化及安全性,除观察者基类外,其派生类不直接触发委托事件
        /// </summary>
        public void Notify(int notifyEvent, string flag, string content, object result, string Message)
        {
            //提高执行效率及安全性
            if (this.NotifyEvent != null)
            {
                this.NotifyEvent(notifyEvent, flag, content, result, Message, this);
            }
        }
    }




    /**/
    /// <summary>
    ///     在Observer Pattern(观察者模式)中,此类作为所有Observer(观察者)的抽象基类
    /// 所有要充当观察者的类都继承于此类.
    ///     我们说此类作为观察者基类,用于规划所有观察者(即订阅方)订阅行为.
    ///     在此事例中,规划了针对目标基类(Subject)中声明的NotifyEventHandler委托的一个
    /// 方法(update),并于构造该观察者时将其注册于具体目标(参数传递)的委托事件中.
    ///     具体实施过程:
    ///     1.指定观察者所观察的对象(即发布方).(通过构造器传递)
    ///     2.规划观察者自身需要作出响应方法列表
    ///     3.注册需要委托执行的方法.(通过构造器实现)
    /// </summary>
    public abstract class Observer
    {
        /**/
        /// <summary>
        /// 构造时通过传入模型对象,把观察者与模型关联,并完成订阅.
        /// 在此确定需要观察的模型对象.
        /// </summary>

        public Observer()
        { 
        
        }
        public Observer(Subject subject)
        {
            //订阅
            //把观察者行为(这里是Update)注册于委托事件
            subject.NotifyEvent += new Subject.NotifyEventHandler(Update);
        }

        /**/
        /// <summary>
        /// 规划了观察者的一种行为(方法),所有派生于该观察者基类的具体观察者都
        /// 通过覆盖该方法来实现作出响应的行为.
        /// </summary>
        public abstract void Update(int notifyEvent, string flag, string content, object result, string message, object sender);
    }



    public class SubjectObserver : Observer
    {
        public delegate void FormInvoke(int Event, string flag, string content, object result, string message, object sender);

        public delegate void UpdateNotify(int notifyEvent, string flag, string content, object result, string message, object sender);

        public UpdateNotify updateNotify;

        public void SetCallBackOnUpdateNotify(UpdateNotify lpUpdateNotify)
        {
            updateNotify = lpUpdateNotify;
        }
        public override void Update(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            if (updateNotify != null)
            {
                updateNotify(notifyEvent, flag ,content,result, message, sender);
            }
        }
    }



    public class SuperSubject : Subject
    {
        public SubjectObserver subjectObserver;

        public SuperSubject()
        {
            this.subjectObserver = new SubjectObserver();
            this.subjectObserver.SetCallBackOnUpdateNotify(this.ProcessResponse);            
        }

        public override void ProcessResponse(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            this.Notify(notifyEvent, flag, content, result, message);
        }


    }



 



}
