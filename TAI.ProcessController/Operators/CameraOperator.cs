using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Protocols;
using DMT.Core.Models;

namespace TAI.Manager
{



    public class CameraOperator :BaseOperator
    {
         


        public CameraOperator() : base()
        {

            this.Caption = "CameraOperator";
            this.BaseIndex = 0;
/*            this.SwitchItem = new ModbusItem(this.Caption, "通道切换", "SwithChannel", this.BaseIndex, 6, 1, ChannelType.AO);
            this.Items.Add(this.SwitchItem);
            this.ModeItem = new ModbusItem(this.Caption, "通道模式", "SwithMode", this.BaseIndex, 5, 1, ChannelType.AO);
            this.Items.Add(this.ModeItem);*/

        }

    }
}
