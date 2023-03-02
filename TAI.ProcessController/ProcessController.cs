using System;
using DMT.Core.Protocols;
using DMT.Core.Models;
using System.Collections.Generic;

namespace TAI.Manager
{

    public class OperateItems
    {
        public readonly string Section = "ProcessControlPLC";
        public List<ModbusItem> Items { get; set; }
        #region InitializeOperate 
        /// <summary>
        /// 初始化信号操作 W
        /// </summary>
        public ModbusItem InitializeOperate { get; set; }

        /// <summary>
        /// 初始化完成 R
        /// </summary>
        public ModbusItem InitializeStatus { get; set; }
        /// <summary>
        /// 初始化完成反馈 W
        /// </summary>
        public ModbusItem InitializeCompleted { get; set; }
        /// <summary>
        /// 初始化结束 W
        /// </summary>
        public ModbusItem InitializeFinished { get; set; }

        #endregion

        #region RobotOperate 
        
        ///机器人可调度信号 bool 4005	R
        
/*        public RobotCan

        机器人起始位置 word	4006	W
        机器人目标位置 word	4007	W
        机器人到达信号 bool	4008	R
        机器人到达反馈 bool	4009	W*/
        #endregion


        public OperateItems()
        {
            this.Items = new List<ModbusItem>();
               
        }



        public void Initialize(string fileName)
        {
            //  (string section, string name, short baseIndex, ushort offset, ushort length, ChannelType type
/*            this.detectResult = new ModbusItem(this.name, "DetectResult", @"全部检测结果数据区域", 1, 4);
            ModbusItem item = new ModbusItem(Section,"",1,);*/
        
        
        }



    }


    /// <summary>
    /// 测试流程控制
    /// </summary>
    public class ProcessController: Controller, IController
    {
        public ModbusTCPClient Channel { get; set; }

        public OperateItems OperateItems { get; set; }

        public ProcessController()
        {
            this.OperateItems = new OperateItems();


        }



        private void OperateWrite(ModbusItem item)
        {
            this.Channel.WriteMultipleRegisters(item.StartAddress, item.Datas);
        }

        public void OperateInitialize()
        {

            this.OperateWrite(this.OperateItems.InitializeOperate);
        }


        public bool Initialize()
        {
            this.OperateItems.Initialize("");
            return true;
        }

        public bool Open()
        {
            throw new NotImplementedException();
        }

        public bool Close()
        {
            throw new NotImplementedException();
        }

        public bool Active()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {

            return;
        }


        public override void ProcessEvent()
        {
            if (!this.Channel.HasError)
            {

            }
            else
            {
                //通道读写错误，重连判断及动作
                this.Channel.ReConnectTCPServer();
            }
        }
    }
}
