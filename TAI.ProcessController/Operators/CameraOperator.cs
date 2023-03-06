using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Protocols;
using DMT.Core.Models;

namespace TAI.Manager
{
/*上料到位信号待拍摄bool    4010	R FALSE   到达位置置1，有结果值后复位
上料拍摄结果信号    word	4011	W	0	0：初始值，1：OK，2：NG
二维码待拍摄信号    bool	4012	W FALSE   到达位置置1，有结果值后复位
二维码拍摄完成信号   bool	4013	R FALSE   机器人处于可调度状态
模块顶部状态待拍摄信号 bool	4014	W FALSE   到达位置置1，有结果值后复位
模块顶部检测完成信号  bool	4015	W FALSE   机器人处于可调度状态
模块已上电待检测信号  bool	4016	R FALSE   "模块上电完成，可以开始检测。检测有结果时复位"*/


    public class CameraOperator :BaseOperator
    {
         


        public CameraOperator() : base()
        {

            this.Caption = "CameraOperator";
            this.BaseIndex = 0;
/*            this.SwitchItem = new ModbusItem(this.Caption, "通道切换", "SwithChannel", this.BaseIndex, 6, 1, ChannelType.AO);
            this.Items.Add(this.SwitchItem);
            this.ModeItem = new ModbusItem(this.Caption, "通道模式", "SwithMode", this.BaseIndex, 5, 1, ChannelType.AO);
            this.Items.Add(this.ModeItem);*/

        }



        public override void LoadFromFile(string fileName)
        {
           
        }

        public override void SaveToFile(string fileName)
        {
            
        }
    }
}
