using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Protocols;
using DMT.Core.Models;


namespace TAI.Manager
{
    public class SystemOperator:BaseOperator
    {
        /// <summary>
        /// 初始化信号操作 W
        /// </summary>
        public ModbusItem InitializeOperate { get; set; }
        public readonly ushort DefaultInitializeOperateOffset = 1;

        /// <summary>
        /// 初始化完成反馈 W
        /// </summary>
        public ModbusItem InitializeCompleted { get; set; }
        public readonly ushort DefaultInitializeCompletedOffset = 2;

        /// <summary>
        /// 复位
        /// </summary>
        public ModbusItem ResetOperate { get; set; }
        public readonly ushort DefaultResetOperateOffset = 3;
        
        /// <summary>
        /// 动作结束 W
        /// </summary>
        public ModbusItem ActionFinished { get; set; }
        public readonly ushort DefaultActionFinishedOffset = 4;
        public ModbusItem SystemStatusMap { get; set; }
        public readonly ushort DefaultSystemStatusMapOffset = 1;
        public readonly ushort SystemStatusMapLength = 32;
        public ModbusItem TestResult { get; set; }
        public readonly ushort DefaultTestResultOffset = 13;

        public ModbusItem DIStationBusy { get; set; }
        public readonly ushort DefaultDIStationBusyOffset = 18;
        public ModbusItem DOStationBusy { get; set; }
        public readonly ushort DefaultDOStationBusyOffset = 19;
        public ModbusItem PIStationBusy { get; set; }
        public readonly ushort DefaultPIStationBusyOffset = 20;
        public ModbusItem AIStationBusy { get; set; }
        public readonly ushort DefaultAIStationBusyOffset = 21;
        public ModbusItem AOStationBusy { get; set; }
        public readonly ushort DefaultAOStationBusyOffset = 22;
        public ModbusItem RTD3StationBusy { get; set; }
        public readonly ushort DefaultRTD3StationBusyOffset = 23;
        public ModbusItem RTD4StationBusy { get; set; }
        public readonly ushort DefaultRTD4StationBusyOffset = 24;
        public ModbusItem TCStationBusy { get; set; }
        public readonly ushort DefaultTCStationBusyOffset = 25;
        public ModbusItem PrepareStationBusy { get; set; }
        public readonly ushort DefaultPrepareStationBusyOffset = 26;

        public SystemOperator() : base()
        {
            this.Caption = "SystemOperator";
            this.BaseIndex = 0;
            this.InitializeOperate = new ModbusItem(this.Caption, "初始化", "InitializeOperate", this.BaseIndex, DefaultInitializeOperateOffset, 1, ChannelType.AO);
            this.Items.Add(this.InitializeOperate);

            this.InitializeCompleted = new ModbusItem(this.Caption, "初始化状态", "InitializeCompleted", this.BaseIndex, DefaultInitializeCompletedOffset, 1, ChannelType.AI);
            this.Items.Add(this.InitializeCompleted);

            this.ResetOperate = new ModbusItem(this.Caption, "复位", "ResetOperate", this.BaseIndex, DefaultResetOperateOffset, 1, ChannelType.AO);
            this.Items.Add(this.ResetOperate);

            this.ActionFinished = new ModbusItem(this.Caption, "初始化状态", "ActionFinished", this.BaseIndex, DefaultActionFinishedOffset, 1, ChannelType.AO);
            this.Items.Add(this.ActionFinished);

            this.SystemStatusMap = new ModbusItem(this.Caption, "系统状态", "SystemStatusMap", 0, DefaultSystemStatusMapOffset, SystemStatusMapLength, ChannelType.AI);
            this.Items.Add(this.ActionFinished);

            this.TestResult = new ModbusItem(this.Caption, "总体测试结果", "TestResult", this.BaseIndex, DefaultTestResultOffset, 1, ChannelType.AO);
            this.Items.Add(this.TestResult);




            this.DIStationBusy = new ModbusItem(this.Caption, "DI工位有料", "DIStationBusy", this.BaseIndex, DefaultDIStationBusyOffset, 1, ChannelType.AI);
            this.Items.Add(this.DIStationBusy);

            this.DOStationBusy = new ModbusItem(this.Caption, "DO工位有料", "DOStationBusy", this.BaseIndex, DefaultDOStationBusyOffset, 1, ChannelType.AI);
            this.Items.Add(this.DOStationBusy);

            this.PIStationBusy = new ModbusItem(this.Caption, "PI工位有料", "PIStationBusy", this.BaseIndex, DefaultPIStationBusyOffset, 1, ChannelType.AI);
            this.Items.Add(this.PIStationBusy);

            this.AIStationBusy = new ModbusItem(this.Caption, "AI工位有料", "AIStationBusy", this.BaseIndex, DefaultAIStationBusyOffset, 1, ChannelType.AI);
            this.Items.Add(this.AIStationBusy);

            this.AOStationBusy = new ModbusItem(this.Caption, "AO工位有料", "AOStationBusy", this.BaseIndex, DefaultAOStationBusyOffset, 1, ChannelType.AI);
            this.Items.Add(this.AOStationBusy);

            this.RTD3StationBusy = new ModbusItem(this.Caption, "RTD3工位有料", "RTD3StationBusy", this.BaseIndex, DefaultRTD3StationBusyOffset, 1, ChannelType.AI);
            this.Items.Add(this.RTD3StationBusy);

            this.RTD4StationBusy = new ModbusItem(this.Caption, "RTD4工位有料", "RTD4StationBusy", this.BaseIndex, DefaultRTD4StationBusyOffset, 1, ChannelType.AI);
            this.Items.Add(this.RTD4StationBusy);

            this.TCStationBusy = new ModbusItem(this.Caption, "TC工位有料", "DIStationBusy", this.BaseIndex, DefaultTCStationBusyOffset, 1, ChannelType.AI);
            this.Items.Add(this.TCStationBusy);

            this.PrepareStationBusy = new ModbusItem(this.Caption, "预热工位有料", "PrepareStationBusy", this.BaseIndex, DefaultPrepareStationBusyOffset, 1, ChannelType.AI);
            this.Items.Add(this.PrepareStationBusy);


        }



    }
}
