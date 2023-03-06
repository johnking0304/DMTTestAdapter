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
        public override void LoadFromFile(string fileName)
        {

        }

        public override void SaveToFile(string fileName)
        {

        }
    

 


    }
}
