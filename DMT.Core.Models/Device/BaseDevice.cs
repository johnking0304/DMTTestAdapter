using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;
using DMT.Core.Channels;


namespace DMT.Core.Models
{
    public class BaseDevice :BaseController
    {

        public BaseChannel Channel;

        public CommandPool CommandPool;


        public BaseDevice()
        {
            this.Caption = "Device";
            this.ConfigFileName = "";
            this.CommandPool = new CommandPool(this);
            this.CommandPool.AttachObserver(this.subjectObserver.Update);
        }



        public virtual void CommandAppend(BaseCommand command)
        {
            command.AttachObserver(this.subjectObserver.Update);
            this.Channel.AttachObserver(command.subjectObserver.Update);
            this.CommandPool.AppendCommand(command);
        }

        public virtual void CommandDelete(BaseCommand command)
        {
            
            this.Channel.DetachObserver(command.subjectObserver.Update);
            this.CommandPool.DeleteCommand(command);
        }

        public virtual bool SendCommand(byte[] command)
        {
            return this.Channel.SendCommand(command);
        }

        public virtual bool SendCommand(string command)
        {
            return this.Channel.SendCommand(command);
        }

        public virtual void Dispose()
        {
            this.CommandPool.Terminated = true;
        }

    }
}
