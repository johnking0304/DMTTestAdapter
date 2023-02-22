using System;
using System.Collections.Generic;
using System.Text;
using DMT.Core.Utils;

namespace DMT.Core.Models
{
     public class BaseController : SuperSubject 
    {
        public string Caption { get; set; }
        public string ConfigFileName { get; set; }

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
    }



    public class Controller : BaseController
    {

    }
}
