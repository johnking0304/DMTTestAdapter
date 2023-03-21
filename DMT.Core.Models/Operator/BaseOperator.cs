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
       
        public BaseOperator(short baseIndex)
        {
            this.BaseIndex = baseIndex;
            this.Items = new List<ModbusItem>();

        }
        public virtual void LoadFromFile(string fileName)
        {
            foreach (ModbusItem item in this.Items)
            {
                item.LoadFromFile(fileName);
            }
    }

        public virtual void SaveToFile(string fileName)
        {

            foreach (ModbusItem item in this.Items)
            {
                item.SaveToFile(fileName);
            }
        }
    }

}
