using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DMT.Core.Utils;
using DMT.Core.Channels;


namespace DMT.Core.Models
{

    public  class CommandState
    {
        public BaseCommand Command { get; set; }
        public int WaitMilliseconds { get; set; }
        public virtual void Initialize()
        {
            return;
        }
        public virtual void Execute()
        {
            return;
        }
        public virtual void StateCheck()
        {
            return;
        }

        public CommandState(CommandState oldState)
        {
            this.Command = oldState.Command;
            this.Initialize();
        }
        public CommandState(BaseCommand command)
        {
            this.Command = command;
            this.Command.Initialize();
            this.Initialize();         
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

        public void NotifyMessage(string message)
        {
            this.Command.Notify((int)ChannelNotifyEvent.Debug, "", "", null, message);
        
        }

    }



    public class ReadyState :CommandState
    {
        public bool Sended = false;

       
        public ReadyState(CommandState oldState):base(oldState)
        {
            return;
        }

        public ReadyState(BaseCommand command) : base(command)
        {
            return;
        }

        public override void Initialize()
        {
            this.Sended = false;
            this.Command.Updated = false;
            this.Command.Finished = false;
        }

        public  virtual  void SendCommand()
        {
            string SendData = this.Command.PackageString();

            this.NotifyMessage(string.Format("发送数据[{0}]", SendData));

            this.Sended = this.Command.Device.SendCommand(SendData);
            this.Command.SendTime = DateTime.Now;
            this.Command.SendCount += 1;
        }

        public override void Execute()
        {
            // 尝试次数超出 直接到错误状态
            if (this.Command.SendTryTimesOut())
            {
                this.NotifyMessage("尝试次数超出,切换到错误状态");
                this.Command.State = new ErrorState(this);
                return;
            }
            else
            {
                if (this.Command.SendCount == 0)
                {
                    //第一次 直接发送
                    this.NotifyMessage("开始发送数据,次数[1]");
                    this.SendCommand();
                }
                else
                {
                    
                    //如果第二次以上进来，则根据重试间隔进行发送
                    if (this.TimeOut(this.Command.SendTime))
                    {
                        this.NotifyMessage(string.Format("发送间隔，开始发送数据，次数[{0}]", this.Command.SendCount + 1));
                        this.SendCommand();
                    }
                }           
            }
            this.StateCheck();
        }

        public override void StateCheck()
        {
            if (this.Sended)
            {
                this.NotifyMessage("发送成功，切换到等待状态");
                this.Command.State = new WaitState(this);
            }
        }
    }

    public class WaitState : CommandState
    {
        public DateTime StartDatetime { get; set; }
        public WaitState(CommandState oldState):base(oldState)
        {
            this.Command.Updated = false;
            this.StartDatetime = DateTime.Now;
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
            if (this.Command.Updated)
            {
                switch (this.Command.ResponseResult)
                {
                    case ResponseResult.Ok:
                        this.Command.State = new StopState(this);
                        break;
                    case ResponseResult.Invalid:
                    case ResponseResult.Error:
                    case ResponseResult.TimeOut:
                        this.Command.State = new ErrorState(this);
                        break;
                    default:
                        this.Command.State = new ErrorState(this);
                        break;
                }
            }
            else if (this.TimeOut(this.StartDatetime))
            {
                this.Command.ResponseResult = ResponseResult.TimeOut;
                this.Command.State = new ErrorState(this);
            }

        }

    }

    public class StopState : CommandState
    {
        public StopState(CommandState oldState) : base(oldState)
        {

        }
        public override void Initialize()
        {
            return;
        }

        public override void Execute()
        {
            this.NotifyMessage(string.Format("命令执行完成[{0}]",this.Command.LastMessage));
            this.Command.Finished = true;
        }

        public override void StateCheck()
        {
            return;
        }
    }

    public class ErrorState : CommandState
    {
        public ErrorState(CommandState oldState) : base(oldState)
        {

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
            if (this.Command.SendTryTimesOut())
            {
                this.Command.ResponseResult = ResponseResult.Error;
                this.Command.LastMessage = this.Command.ParseExecuteResult(ResponseResult.Error);
                this.Command.State = new StopState(this);
            }
            else
            {
                this.Command.State = new ReadyState(this);
            }
        }
    }


    public class BaseCommand : SuperSubject
    {
        public char ETX { get; set; }
        public char STX { get; set; }
        public CommandState State { get; set; }
        public BaseDevice Device { get; set; }
        public int SendCount { get; set; }
        public DateTime SendTime { get; set; }
        public ResponseResult ResponseResult { get; set; }
        public DateTime UpdatedTime { get; set; }



        //返回数据完成更新
        public bool Updated { get; set; }

        public int WaitUpdatedMilliSeconds { get; set; }

        //命令结束
        public bool Finished { get; set; }

        public bool NeedReply { get; set; }
        public string ReplyCommandFlag { get; set; }
        public int ReplyLength { get; set; }

        public string LastMessage { get; set; }
        public int TryCountMax { get; set; }



        /// <summary>
        /// 初始化
        /// </summary>
        public BaseCommand(BaseDevice device)
        {
            this.State = null;
            this.Device = device;
            this.SendCount = 0;
            this.SendTime = DateTime.Now;
            this.Updated = false;
            this.ResponseResult = ResponseResult.Unknown;
            this.UpdatedTime = DateTime.Now;
            this.Finished = false;
            this.ETX = '\n';
            this.STX = '\t';
            this.TryCountMax = 3;
            this.WaitUpdatedMilliSeconds = 60000;
            this.ReplyCommandFlag = "";
            this.NeedReply = false;
            this.ReplyLength = 0;
        }

        public Boolean SendTryTimesOut()
        {
            return this.SendCount >= this.TryCountMax;
        }


        public virtual bool ParseResponse(string content, ref double value)
        {
            return true;
        }

        public virtual bool ParseResponse(string content, ref string value)
        {
            return true;
        }

        public virtual bool ParseResponse(string content)
        {
            return true;
        }

        public string ParseExecuteResult(ResponseResult result)
        {
            string message = "";
            switch (result)
            {
                case ResponseResult.Ok:
                    message = "命令执行结束！";
                    break;
                case ResponseResult.TimeOut:
                    message = "命令执行超时！";
                    break;
                case ResponseResult.Error:
                    message = "命令执行错误（Error）！";
                    break;
                case ResponseResult.Fail:
                    message = "命令执行失败！";
                    break;
                case ResponseResult.Unknown:
                    message = "命令执行错误（未知）！";
                    break;
                default:
                    message = "命令执行错误（未知）！";
                    break;
            }
            return message;
        }

        public void FinishUpdate(ResponseResult result)
        {
            this.LastMessage = this.ParseExecuteResult(result);
            this.ResponseResult = result;
            this.UpdatedTime = DateTime.Now;
            this.Updated = true;
        }


        public virtual string Pack()
        {
            return "";
        }

        public virtual string FlagString()
        {
            return "None";
        }
        public virtual string PackageString()
        {
            return null;
        }

        public virtual byte[] PackageBytes()
        {
            return null;
        }

        public virtual bool Initialize()
        {
            return true;
        }

        public virtual bool Execute()
        {
            if (this.State != null)
            {
                this.State.Execute();
            }
            return true;
        }

        public void NotifyMessage(string message)
        {
            this.Notify((int)ChannelNotifyEvent.Debug, "", "", null, message);
        }
    }


    public class CommandPool : SuperSubject
    {
        public const int COMMAND_EVENT = 10101;
        public const int COMMAND_FINISHED_EVENT = 10999; 
        /// <summary>
        /// 功能码命令池
        /// </summary>
        public List<BaseCommand> CacheCommands { get; set; }
        /// <summary>
        /// 主动上送的命令池，主要处理主动上报的协议
        /// </summary>
        public List<BaseCommand> FixedCommands { get; set; }

        public BaseCommand ActiveCommand { get; set; }
        public Thread Processor;
        public Boolean Terminated { get; set; }
        public BaseDevice Device { get; set; }

        public CommandPool(BaseDevice device)
        {
            this.Device = device;
            this.Terminated = false;
            this.CacheCommands = new List<BaseCommand>();
            this.FixedCommands = new List<BaseCommand>();

            this.Processor = new Thread(new ThreadStart(this.ThreadExecute));
            this.Processor.IsBackground = true;
            this.Processor.Start();
        }
        public void AppendCommand(BaseCommand command)
        {
            this.CacheCommands.Add(command);
        }
        public void DeleteCommand(int index)
        {
            if (this.CacheCommands.Count > index)
            {
                this.CacheCommands.RemoveAt(index);
            }
        }

        public void DeleteCommand(BaseCommand command)
        {

            this.CacheCommands.Remove(command);

        }
        public void ThreadExecute()
        {
            while (!this.Terminated)
            {
                Thread.Sleep(1);
                if (this.CacheCommands.Count == 0)
                {
                    continue;
                }
                BaseCommand command = this.CacheCommands.First();
                this.ActiveCommand = command;
                if (command.Finished)
                {                   
                    this.Notify(COMMAND_FINISHED_EVENT, command.FlagString(), "", command.ResponseResult, command.LastMessage);
                    this.Device.CommandDelete(command);
                    this.ActiveCommand = null;
                }
                else
                {
                    command.Execute();
                }
            }
        }

    }


}
