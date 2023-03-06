using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Channels;
using DMT.Core.Protocols;


namespace DMT.Core.Models
{
    public abstract class BaseOperator
    {
        public short BaseIndex = 0;
        public string Caption = "Operator";
        public List<ModbusItem> Items { get; set; }
       
        public BaseOperator()
        {
            this.Items = new List<ModbusItem>();

        }
        public abstract void LoadFromFile(string fileName);

        public abstract void SaveToFile(string fileName);
    }

}
