using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DMT.Core.Utils;
using Newtonsoft.Json;

namespace DMT.Core.Models
{
    public class StatusMessage
    {
        [JsonProperty(propertyName: "message")]
        public string LastMessage { get; set; }
        
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "code")]
        public int LastErrorCode { get; set; }

        [JsonProperty(propertyName: "status")]
        public string Status { get; set; }

        public StatusMessage(string name)
        {
            this.LastErrorCode = 0;
            this.LastMessage = "";
            this.Status = "";
            this.Name = name;
        }


    }



     public class BaseController : SuperSubject 
    {
        public string Caption { get; set; }
        public string ConfigFileName { get; set; }
        public Thread Processor { get; set; }
        public bool Terminated { get; set; }

        public int WaitMilliseconds = 5000;
        public DateTime StartWaitDateTime { get; set; }
        public int ReadIntervalMilliseconds { get; set; }
        public DateTime StartReadDateTime { get; set; }

        public StatusMessage StatusMessage { get; set; }

        public string StatusMessageText { 
            get
            { 
                return JsonConvert.SerializeObject(this.StatusMessage);
            } 
        }
        public BaseController()
        {
            this.Caption = "Controller";
            this.ConfigFileName = "";
            this.StartWaitDateTime = DateTime.Now;
            this.StartReadDateTime = DateTime.Now;
            this.WaitMilliseconds = 5000;
            this.ReadIntervalMilliseconds = 200;
            this.StatusMessage = new StatusMessage(this.Caption);
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


        public bool ReadTrigger()
        {
            TimeSpan span = DateTime.Now - this.StartReadDateTime;
            if (span.TotalMilliseconds >= this.ReadIntervalMilliseconds)
            {
                this.StartReadDateTime = DateTime.Now;
                return true;
            }
            return false;
        }
        public bool WaitTimeOut()
        {
            TimeSpan span = DateTime.Now - this.StartWaitDateTime;
            if (span.TotalMilliseconds >= this.WaitMilliseconds)
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



    public class OperatorController : BaseController
    {
        public OperatorController() : base()
        { 
        
        }
    }
}
