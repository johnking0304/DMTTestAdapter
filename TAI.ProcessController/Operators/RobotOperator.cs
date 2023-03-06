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
        Capture =1,
        Transport=2,
    }
    public class RobotOperator : BaseOperator
    {
        public ModbusItem GetIdleStatus { get; set; }
        /// <summary>
        /// 设置移动位置及模式
        /// </summary>
        public ModbusItem MoveActionParams { get; set; }
        /// <summary>
        /// 使能移动
        /// </summary>
        public ModbusItem EnabelMoveAction { get; set; }
        /// <summary>
        /// 移动是否完成
        /// </summary>
        public ModbusItem MoveCompletedStatus { get; set; }

        



    }
}
