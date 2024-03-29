﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Protocols;
using DMT.Core.Models;

namespace TAI.Manager
{


    public class DetectOperator : BaseOperator
    {

        /// <summary>
        /// 模块类型识别完成
        /// </summary>
        public ModbusItem ModuleTypeOCRCompleted { get; set; }
        public readonly ushort DefaultModuleTypeOCRCompletedOffset = 19;
        /// <summary>
        /// 模块上电就绪
        /// </summary>
        public ModbusItem ModuleTestReadyStatus { get; set; }
        public readonly ushort DefaultModuleTestReadyStatusOffset = 20;
        /// <summary>
        /// 模块二维码识别完成
        /// </summary>
        public ModbusItem ModuleQRCompleted { get; set; }
        public readonly ushort DefaultModuleQRCompletedOffset = 21;
        /// <summary>
        /// 模块灯测完成
        /// </summary>
        public ModbusItem ModuleOCRLightingCompleted { get; set; }
        public readonly ushort DefaultModuleOCRLightingCompletedOffset = 22;

        /// <summary>
        /// 总体测试结果
        /// </summary>
        public ModbusItem TestResult { get; set; }
        public readonly ushort DefaultTestResultOffset = 23;

        public ModbusItem StartStationTest { get; set; }
        public readonly ushort DefaultStartStationTestOffset = 24;

        public ModbusItem StartTest { get; set; }
        public readonly ushort DefaultStartTestOffset = 25;

        public ModbusItem FeedingBoxMapValue { get; set; }
        public readonly ushort DefaultFeedingBoxMapValueOffset = 26;

        public ModbusItem StationInTesting { get; set; }
        public readonly ushort DefaultStationInTestingOffset = 27;

        public DetectOperator(short baseIndex) : base(baseIndex)
        {
            this.Caption = "DetectOperator";

            this.TestResult = new ModbusItem(this.Caption, "总体测试结果", "TestResult", this.BaseIndex, DefaultTestResultOffset, 1, ChannelType.AO);
            this.Items.Add(this.TestResult);

            this.ModuleTestReadyStatus = new ModbusItem(this.Caption, "模块测试准备完成", "ModuleTestReadyStatus", this.BaseIndex, DefaultModuleTestReadyStatusOffset, 1, ChannelType.AI);
            this.Items.Add(this.ModuleTestReadyStatus);

            this.ModuleQRCompleted = new ModbusItem(this.Caption, "模块二维码识别完成", "ModuleQRCompleted", this.BaseIndex, DefaultModuleQRCompletedOffset, 1, ChannelType.AO);
            this.Items.Add(this.ModuleQRCompleted);

            this.ModuleOCRLightingCompleted = new ModbusItem(this.Caption, "模块灯测识别完成", "ModuleOCRLightingCompleted", this.BaseIndex, DefaultModuleOCRLightingCompletedOffset, 1, ChannelType.AO);
            this.Items.Add(this.ModuleOCRLightingCompleted);

            this.ModuleTypeOCRCompleted = new ModbusItem(this.Caption, "模块类型识别完成", "ModuleTypeOCRCompleted", this.BaseIndex, DefaultModuleTypeOCRCompletedOffset, 1, ChannelType.AO);
            this.Items.Add(this.ModuleTypeOCRCompleted);

            this.StartStationTest = new ModbusItem(this.Caption, "设置工位开始测试", "StartStationTest", this.BaseIndex, DefaultStartStationTestOffset, 1, ChannelType.AO);
            this.Items.Add(this.StartStationTest);

            this.StartTest = new ModbusItem(this.Caption, "设置系统开始测试", "StartTest", this.BaseIndex, DefaultStartTestOffset, 1, ChannelType.AO);
            this.Items.Add(this.StartTest);

            this.FeedingBoxMapValue = new ModbusItem(this.Caption, "上料工位点位状态", "FeedingBoxMapValue", this.BaseIndex, DefaultFeedingBoxMapValueOffset, 1, ChannelType.AO);
            this.Items.Add(this.FeedingBoxMapValue);

            this.StationInTesting = new ModbusItem(this.Caption, "工位进入测试状态", "StationInTesting", this.BaseIndex, DefaultStationInTestingOffset, 1, ChannelType.AO);
            this.Items.Add(this.StationInTesting);



        }


    }
}
