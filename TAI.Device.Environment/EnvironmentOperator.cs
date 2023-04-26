using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;
using DMT.Core.Protocols;

namespace TAI.Device
{



    public class EnvironmentOperator : BaseOperator
    {

        public ModbusItem EnvironmentData { get; set; }
        public readonly ushort DefaultEnvironmentDataOffset = 1;



        public EnvironmentOperator(short baseIndex) : base(baseIndex)
        {
            this.Caption = " EnvironmentOperator";

            this.EnvironmentData = new ModbusItem(this.Caption, "环境参数", "EnvironmentData", this.BaseIndex, DefaultEnvironmentDataOffset, 2, ChannelType.AO);
            this.Items.Add(this.EnvironmentData);
            
        }

    }
}
