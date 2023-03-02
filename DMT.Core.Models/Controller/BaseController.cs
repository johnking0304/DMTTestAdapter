using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DMT.Core.Utils;

namespace DMT.Core.Models
{
     public class BaseController : SuperSubject 
    {
        public string Caption { get; set; }
        public string ConfigFileName { get; set; }


        public Thread Processor { get; set; }
        public bool Terminated { get; set; }

        public int WaitSeconds = 5;
        public DateTime StartWaitDateTime = DateTime.Now;
        public BaseController()
        {
            this.Caption = "Controller";
            this.ConfigFileName = "";
        }

        public virtual void LoadFromFile(string fileName)
        {
            this.ConfigFileName = fileName;
            string[] list = IniFiles.GetAllSectionNames(fileName);
            

            if (!((System.Collections.IList)list).Contains(this.Caption))
            {
                this.SaveToFile(fileName);
            }
        }

        public virtual void SaveToFile(string fileName)
        {
            this.ConfigFileName = fileName;
        }

        public virtual void SaveToFile()
        {
            if (this.ConfigFileName.Length > 0)
            {
                this.SaveToFile(this.ConfigFileName);
            }
        }


        public bool WaitTimeOut()
        {
            TimeSpan span = DateTime.Now - this.StartWaitDateTime;
            if (span.TotalSeconds >= this.WaitSeconds)
            {
                this.StartWaitDateTime = DateTime.Now;
                return true;
            }
            return false;

        }

        public  virtual  void ProcessEvent()
        {
            return;
        }
        public  void Process()
        {
            while (!this.Terminated)
            {
                try
                {
                    Thread.Sleep(1);

                    this.ProcessEvent();
                }
                catch (System.Threading.ThreadInterruptedException)
                {
                }
            }

        }

        public void StartThread()
        {
            this.Processor = new Thread(new ThreadStart(this.Process))
            {
                IsBackground = true
            };
            this.Processor.Start();
        }

        public void StopThread()
        {
            this.Terminated = true;
        }





    }



    public class Controller : BaseController
    {
        public int LastErrorCode { get; set; }
        public string LastMessage { get; set; }
    }
}
