using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;


namespace DMT.Core.Channels
{
    // 摘要: 
    //     指定发送数据后的结果。
    public enum ResponseResult
    {
        /// 摘要: 
        ///     成功返回。
        Ok = 0,
        ///
        /// 摘要: 
        ///     超时。
        TimeOut = 1,
        ///
        /// 摘要: 
        ///     错误。
        Error = 2,
        ///
        /// 摘要: 
        ///     错误。
        Invalid = 3,
        ///
        /// 摘要: 
        ///     未知。
        Unknown = 4,
        /// <summary>
        /// 
        /// </summary>
        Fail = 5
    }


    public enum ChannelControl
    {
        None = 99,
        Init = 0,
        Open = 1,
        Close = 2,
        Send = 3,
        Receive = 4,
        Report = 5,
        Connect= 6,
        Disconnect =7,

    }

    public enum ChannelNotifyEvent
    { 
        Info  =1,
        Debug =2,
    }

    public enum ChannelResult
    { 
        OK              = 0,
        Fail           = 1,
        CanNotOpen      = 2,
        CanNotClose     = 3,
        InvalidParam    = 4,
        SendError       = 5,
        ReceiveError    = 6,
        Operating       = 7,
        ReceiveTimeOut   = 8,
        UnknownError    = 10  

    }

    


    public abstract class abstractChannel:SuperSubject
    {
        abstract public  Boolean Open();
        abstract public void  OpenSync();
        abstract public  Boolean Close();
        abstract public  Boolean Active();
        abstract public  Boolean SendCommand(string Command);
        abstract public Boolean SendCommand(byte[] Command);
        abstract public  Boolean SendCommandNoReply(string Command);
        abstract public Boolean SendCommandNoReply(byte[] command);

        abstract public  void LoadFromFile(string fileName);
        abstract public  void SaveToFile(string fileName);

        abstract public string Receive();
        abstract public  void StartReceiveData();
        abstract public void StopReceiveData();


    }

    public class BaseChannel : abstractChannel
    {
        public const int CHANNEL_EVENT = 10099;
        public string Caption { get; set; }

        public string ConfigFileName { get; set; }
        public String LastMessage { get; set; }  //最后的消息（错误）
        public ChannelResult LastErrorCode { get; set; }



        public override void ProcessResponse(int notifyEvent, string flag,string content, object result, string message, object sender)
        {
            this.Notify(notifyEvent,  flag,content,  result,  message);
        }


        public override Boolean Open()
        {
            return true;
        }
        public override Boolean Close()
        {
            return true;
        }
        public override Boolean Active()
        {
            return true;
        }

        public override void OpenSync()
        {
            return ;
        }

        public override Boolean SendCommand(String command)
        {
            return true;
        }

        public override Boolean SendCommand(byte[] command)
        {
            return true;
        }

        public override string Receive()
        {
            return "";
        }

        public override Boolean SendCommandNoReply(string command)
        {
            return true;
        }

        public override Boolean SendCommandNoReply(byte[] command)
        {
            return true;
        }

        


        public override void StartReceiveData()
        {
            return;
        }


        public override void StopReceiveData()
        {
            return;
        }
        

        public override void LoadFromFile(string fileName)
        {
            string[] list = IniFiles.GetAllSectionNames(fileName);
            if (!list.Contains(this.Caption))
            {
                this.SaveToFile(fileName);
            }
        }
        public override void SaveToFile(string fileName)
        {
            this.ConfigFileName = fileName; 
        }

    
    }

    public class ChannelObserver : SubjectObserver
    { 


    }



}
