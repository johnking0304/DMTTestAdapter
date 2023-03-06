using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;
using DMT.Core.Protocols;

namespace TAI.Manager
{

    public enum SwitchMode
    { 
        ModeOff = 0,
        ModeTC=1,
        ModeAI=1,
        ModeAO=1,
        ModeRTD=2,
    }
    public class SwitchOperator:BaseOperator
    {

        public ModbusItem SwitchItem { get; set; }

        /*      通道模式寄存器(可读可写)
                Mode: 0-关闭全部使能通道
                1-A 进 A 出 （TC 和 AI、AO 电压型）
                2-A 进 A 出，B 进 B 出（RTD3，4 线）*/
        public ModbusItem ModeItem { get; set; }

        public SwitchOperator()
        {
            this.Caption = "SwitchOperator";
            this.BaseIndex = 0;
            this.SwitchItem = new ModbusItem(this.Caption, "通道切换", "SwithChannel", this.BaseIndex, 6, 1, ChannelType.AO);
            this.Items.Add(this.SwitchItem);
            this.ModeItem = new ModbusItem(this.Caption, "通道模式", "SwithMode", this.BaseIndex, 5, 1, ChannelType.AO);
            this.Items.Add(this.ModeItem);
        }


        public override void LoadFromFile(string fileName)
        {

        }

        public override void SaveToFile(string fileName)
        {

        }
    }
}
