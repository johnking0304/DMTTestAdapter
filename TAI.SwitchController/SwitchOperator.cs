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
        Off = 0,    //0-关闭全部使能通道
        Voltage =1, //1-A 进 A 出 （TC 和 AI、AO 电压型）
        RTD =2,     //2-A 进 A 出，B 进 B 出（RTD3，4 线）
        Current=3,  //3-B 进 A 出 （AI、AO 电流型）*/
    }
    public class SwitchOperator:BaseOperator
    {

        public readonly ushort DefaultSwithModeOffset = 4;    //40005
        public readonly ushort DefaultSwithChannelOffset = 5; //40006
        
        /// <summary>
        /// 切换的通道号
        /// </summary>
        public ModbusItem ChannelSelectId { get; set; }

        /*      通道模式寄存器(可读可写)
                Mode: 0-关闭全部使能通道
                1-A 进 A 出 （TC 和 AI、AO 电压型）
                2-A 进 A 出，B 进 B 出（RTD3，4 线）*/
        public ModbusItem ModeSelect { get; set; }

        public SwitchOperator(short baseIndex):base(baseIndex)
        {
            this.Caption = "SwitchOperator";
            this.ChannelSelectId = new ModbusItem(this.Caption, "通道切换", "SwithChannel", this.BaseIndex, DefaultSwithChannelOffset, 1, ChannelType.AO);
            this.Items.Add(this.ChannelSelectId);
            this.ModeSelect = new ModbusItem(this.Caption, "通道模式", "SwithMode", this.BaseIndex, DefaultSwithModeOffset, 1, ChannelType.AO);
            this.Items.Add(this.ModeSelect);
        }


    }
}
