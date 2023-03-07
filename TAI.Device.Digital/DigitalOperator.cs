using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;
using DMT.Core.Protocols;

namespace TAI.Device
{
     public enum PluseMode
    { 
        None =0,
        PL1HZ =1,
        PL10KHZ= 2,
    }


    public class DigitalOperator: BaseOperator
    {

        public ModbusItem PluseCount { get; set; }
        public readonly ushort DefaultPluseCountOffset = 1;
        /// <summary>
        /// 测量DO模块输出    Read PLC DI点位
        /// </summary>
        public ModbusItem InputChannels{ get; set; }
        public readonly ushort DefaultInputChannelsOffset = 2;

        /// <summary>
        /// 测量DI模块输出   Write PLC DO点位
        /// </summary>
        public ModbusItem OutputChannels { get; set; }
        public readonly ushort DefaultOutputChannelsOffset = 3;

        /// <summary>
        /// PI通道使能 1-8  0-初始状态  
        /// </summary>
        public ModbusItem PIChannelSelectId { get; set; }
        public readonly ushort DefaultPIChannelSelectIdOffset = 5;
        /// <summary>
        /// EPW  IPW 通道选择
        /// </summary>
        public ModbusItem PWChannelSelectId { get; set; }
        public readonly ushort DefaultPWChannelSelectIdOffset = 6;

        /// <summary>
        /// 脉冲输出模式    1HZ  10000HZ  0=无效
        /// </summary>
        public ModbusItem PluseOutputMode { get; set; }
        public readonly ushort DefaultPluseOutputModeOffset = 7;

        /// <summary>
        ///测量电压值  
        /// </summary>
        public ModbusItem MeasureVoltage { get; set; }
        public readonly ushort DefaultMeasureVoltageOffset = 9;


        public DigitalOperator() : base()
        {
            this.Caption = " DigitalChannelOperator";
            this.BaseIndex = 0;

            this.PluseCount = new ModbusItem(this.Caption, "脉冲计数", "PluseCount", this.BaseIndex, DefaultPluseCountOffset, 1, ChannelType.AO);
            this.Items.Add(this.PluseCount);

            this.InputChannels = new ModbusItem(this.Caption, "测量DO模块输出PLC-DI点位", "InputChannels", this.BaseIndex, DefaultInputChannelsOffset, 1, ChannelType.AO);
            this.Items.Add(this.InputChannels);

            this.OutputChannels = new ModbusItem(this.Caption, "测量DI模块输出PLC-DI点位", "OutputChannels", this.BaseIndex, DefaultOutputChannelsOffset, 2, ChannelType.AO);
            this.Items.Add(this.OutputChannels);

            this.PIChannelSelectId = new ModbusItem(this.Caption, " PI通道选择使能", "PIChannelSelectId", this.BaseIndex, DefaultPIChannelSelectIdOffset, 1, ChannelType.AO);
            this.Items.Add(this.PIChannelSelectId);

            this.PWChannelSelectId = new ModbusItem(this.Caption, "EPW IPW通道选择", "PWChannelSelectId", this.BaseIndex, DefaultPWChannelSelectIdOffset, 1, ChannelType.AO);
            this.Items.Add(this.OutputChannels);

            this.PluseOutputMode = new ModbusItem(this.Caption, " 脉冲输出模式", "PluseOutputMode", this.BaseIndex, DefaultPluseOutputModeOffset, 1, ChannelType.AO);
            this.Items.Add(this.PluseOutputMode);

            this.MeasureVoltage = new ModbusItem(this.Caption, "测量电压值", "MeasureVoltage", this.BaseIndex, DefaultMeasureVoltageOffset, 2, ChannelType.AO);
            this.Items.Add(this.MeasureVoltage);

        }

    }
}
