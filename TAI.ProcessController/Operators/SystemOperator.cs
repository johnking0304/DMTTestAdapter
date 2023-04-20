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
        /// 暂停按钮 W
        /// </summary>
        public ModbusItem SystemPause { get; set; }
        public readonly ushort DefaultSystemPauseOffset = 4;

        /// <summary>
        /// 系统急停
        /// </summary>
        public ModbusItem SystemStop { get; set; }
        public readonly ushort DefaultSystemStopOffset = 5;


        /// </summary>
        public ModbusItem TestDeviceType { get; set; }
        public readonly ushort DefaultTestDeviceTypeOffset = 6;

        /// <summary>
        /// 状态表
        /// </summary>
        public ModbusItem SystemStatusMap { get; set; }
        public readonly ushort DefaultSystemStatusMapOffset = 1;
        public readonly ushort SystemStatusMapLength = 50;


        public ModbusItem FeedLackSignal{ get; set; }
        public readonly ushort DefaultFeedLackSignalOffset = 30;
        public ModbusItem OKBlankFullSignal { get; set; }
        public readonly ushort DefaultOKBlankFullSignalOffset = 31;
        public ModbusItem NGBlankFullSignal { get; set; }
        public readonly ushort DefaultNGBlankFullSignalOffset = 32;

        public ModbusItem DIStationBusy { get; set; }
        public readonly ushort DefaultDIStationBusyOffset = 33;
        public ModbusItem DOStationBusy { get; set; }
        public readonly ushort DefaultDOStationBusyOffset = 34;
        public ModbusItem PIStationBusy { get; set; }
        public readonly ushort DefaultPIStationBusyOffset = 35;
        public ModbusItem AIStationBusy { get; set; }
        public readonly ushort DefaultAIStationBusyOffset = 36;
        public ModbusItem AOStationBusy { get; set; }
        public readonly ushort DefaultAOStationBusyOffset = 37;
        public ModbusItem RTD3StationBusy { get; set; }
        public readonly ushort DefaultRTD3StationBusyOffset = 38;
        public ModbusItem RTD4StationBusy { get; set; }
        public readonly ushort DefaultRTD4StationBusyOffset = 39;
        public ModbusItem TCStationBusy { get; set; }
        public readonly ushort DefaultTCStationBusyOffset = 40;
        public ModbusItem PrepareStationBusy { get; set; }
        public readonly ushort DefaultPrepareStationBusyOffset = 41;

        public ModbusItem NewFeedSignal { get; set; }
        public readonly ushort DefaultNewFeedSignalOffset = 42;

        public ModbusItem NewFeedSignalReset { get; set; }
        public readonly ushort DefaultNewFeedSignalResetOffset = 43;
        


        public SystemOperator(short baseIndex) : base(baseIndex)
        {
            this.Caption = "SystemOperator";
            this.InitializeOperate = new ModbusItem(this.Caption, "初始化", "InitializeOperate", this.BaseIndex, DefaultInitializeOperateOffset, 1, ChannelType.AO);
            this.Items.Add(this.InitializeOperate);

            this.InitializeCompleted = new ModbusItem(this.Caption, "初始化状态", "InitializeCompleted", this.BaseIndex, DefaultInitializeCompletedOffset, 1, ChannelType.AI);
            this.Items.Add(this.InitializeCompleted);

            this.ResetOperate = new ModbusItem(this.Caption, "复位", "ResetOperate", this.BaseIndex, DefaultResetOperateOffset, 1, ChannelType.AO);
            this.Items.Add(this.ResetOperate);

            this.SystemPause = new ModbusItem(this.Caption, "系统暂停", "SystemPause", this.BaseIndex, DefaultSystemPauseOffset, 1, ChannelType.AO);
            this.Items.Add(this.SystemPause);

            this.SystemStop = new ModbusItem(this.Caption, "系统急停", "SystemStop", this.BaseIndex, DefaultSystemStopOffset, 1, ChannelType.AI);
            this.Items.Add(this.SystemStop);

            this.TestDeviceType = new ModbusItem(this.Caption, "测试产品类型", "TestDeviceType", this.BaseIndex, DefaultTestDeviceTypeOffset, 1, ChannelType.AI);
            this.Items.Add(this.TestDeviceType);

            this.SystemStatusMap = new ModbusItem(this.Caption, "系统状态", "SystemStatusMap", this.BaseIndex, DefaultSystemStatusMapOffset, SystemStatusMapLength, ChannelType.AI);
            this.Items.Add(this.SystemStatusMap);


            this.FeedLackSignal = new ModbusItem(this.Caption, "上料缺料信号", "FeedLackSignal", this.BaseIndex, DefaultFeedLackSignalOffset, 1, ChannelType.AI);
            this.Items.Add(this.FeedLackSignal);

            this.OKBlankFullSignal = new ModbusItem(this.Caption, "OK下料满料信号", "OKBlankFullSignal", this.BaseIndex, DefaultOKBlankFullSignalOffset, 1, ChannelType.AI);
            this.Items.Add(this.OKBlankFullSignal);

            this.NGBlankFullSignal = new ModbusItem(this.Caption, "NG下料满料信号", "NGBlankFullSignal", this.BaseIndex, DefaultNGBlankFullSignalOffset, 1, ChannelType.AI);
            this.Items.Add(this.NGBlankFullSignal);


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


            this.NewFeedSignal = new ModbusItem(this.Caption, "新上料信号", "NewFeedSignal", this.BaseIndex, DefaultNewFeedSignalOffset, 1, ChannelType.AI);
            this.Items.Add(this.NewFeedSignal);

            this.NewFeedSignalReset = new ModbusItem(this.Caption, "新上料信号复位", "NewFeedSignalReset", this.BaseIndex, DefaultNewFeedSignalResetOffset, 1, ChannelType.AI);
            this.Items.Add(this.NewFeedSignalReset);



        }



    }
}
