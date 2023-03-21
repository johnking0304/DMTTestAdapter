using System;
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

        public DetectOperator(short baseIndex) : base(baseIndex)
        {
            this.Caption = "DetectOperator";

            this.TestResult = new ModbusItem(this.Caption, "总体测试结果", "TestResult", this.BaseIndex, DefaultTestResultOffset, 1, ChannelType.AO);
            this.Items.Add(this.TestResult);

            this.ModuleTestReadyStatus = new ModbusItem(this.Caption, "模块测试准备完成", "ModuleTestReadyStatus", this.BaseIndex, DefaultModuleTestReadyStatusOffset, 1, ChannelType.AI);
            this.Items.Add(this.ModuleTestReadyStatus);

            this.ModuleQRCompleted = new ModbusItem(this.Caption, "模块二维码识别完成", "ModuleQRCompleted", this.BaseIndex, DefaultModuleQRCompletedOffset, 1, ChannelType.AO);
            this.Items.Add(this.ModuleQRCompleted);

            this.ModuleOCRLightingCompleted = new ModbusItem(this.Caption, "模块等测识别完成", "ModuleOCRLightingCompleted", this.BaseIndex, DefaultModuleOCRLightingCompletedOffset, 1, ChannelType.AO);
            this.Items.Add(this.ModuleOCRLightingCompleted);


        }


    }
}
