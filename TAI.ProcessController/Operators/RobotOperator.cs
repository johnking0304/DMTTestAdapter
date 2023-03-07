using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Protocols;
using DMT.Core.Models;

namespace TAI.Manager
{
/*  机器人 机器人可调度信号    bool	4005	R FALSE   机器人若处于可调度位置置1
    机器人目标位置             word	4007	W	0	"100:原点位置，1—6 上料位置  7—15检测工位"
	机器人到达信号             bool	4008	R FALSE   到达位置置1
    机器人到达反馈             bool	4009	W FALSE   复位机器人到达信号*/




    public enum ActionMode
    { 
        Capture =1,  //拍摄模式
        Transport=2,  //运送模式
    }
    public class RobotOperator : BaseOperator
    {
        public ModbusItem GetIdleStatus { get; set; }
        public readonly ushort DefaultGetIdleStatusOffset = 5;
        /// <summary>
        /// 设置移动位置及模式
        /// </summary>
        public ModbusItem MoveActionParams { get; set; }
        public readonly ushort DefaultMoveActionParamsOffset = 6;
        public readonly ushort MoveActionParamsDataLength = 3;
        /// <summary>
        /// 使能移动
        /// </summary>
        public ModbusItem EnabelMoveAction { get; set; }
        public readonly ushort DefaultEnabelMoveActionOffset = 9;
        /// <summary>
        /// 移动是否完成
        /// </summary>
        public ModbusItem MoveCompletedStatus { get; set; }
        public readonly ushort DefaultMoveCompletedStatusOffset = 10;


        public RobotOperator() : base()
        {
            this.Caption = "RobotOperator";
            this.BaseIndex = 0;
            this.GetIdleStatus = new ModbusItem(this.Caption, "初始化", "GetIdleStatus", this.BaseIndex, DefaultGetIdleStatusOffset, 1, ChannelType.AI);
            this.Items.Add(this.GetIdleStatus);

            this.MoveActionParams = new ModbusItem(this.Caption, "移动参数（起始位置、结束位置、模式）", "MoveActionParams", this.BaseIndex, DefaultMoveActionParamsOffset, MoveActionParamsDataLength, ChannelType.AO);
            this.Items.Add(this.MoveActionParams);

            this.EnabelMoveAction = new ModbusItem(this.Caption, "移动使能", "EnabelMoveAction", this.BaseIndex, DefaultEnabelMoveActionOffset, 1, ChannelType.AO);
            this.Items.Add(this.EnabelMoveAction);

            this.MoveCompletedStatus = new ModbusItem(this.Caption, "移动完成", "MoveCompletedStatus", this.BaseIndex, DefaultMoveCompletedStatusOffset, 1, ChannelType.AO);
            this.Items.Add(this.MoveCompletedStatus);
        }


    }
}
