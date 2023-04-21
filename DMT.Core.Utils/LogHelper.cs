using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DMT.Core.Utils
{
    public class LogHelper
    {
        public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("LogInfo");//这里的 loginfo 和 log4net.config 里的名字要一样
        public static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("LogError");//这里的 logerror 和 log4net.config 里的名字要一样
        public static readonly log4net.ILog lognet = log4net.LogManager.GetLogger("LogNet");//网络请求日志
        public static readonly log4net.ILog logSql = log4net.LogManager.GetLogger("LogSQL");//数据库操作日志
        public static readonly log4net.ILog logudp = log4net.LogManager.GetLogger("LogUDP");//udp 



        /// <summary>
        /// 输出信息日志
        /// </summary>
        /// <param name="msg"></param>
        public static void LogInfoMsg(string msg)
        {
            loginfo.Info(msg);
            logudp.Info(msg);
        }

        public static void LogInfoMsg(string msg1, string msg2)
        {
            LogInfoMsg(string.Format("{0}{1}", msg1, msg2));
        }
        public static void LogInfoMsg(string msg1,string msg2,string msg3)
        {
            LogInfoMsg(string.Format("{0}{1}{2}", msg1, msg2,msg3));
        }
        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="msg"></param>
        public static void LogErrMsg(string msg)
        {

            logerror.Error(msg);
            logudp.Info(msg);
        }
        /// <summary>
        /// 输出调试日志
        /// </summary>
        /// <param name="msg"></param>
        public static void LogDebugMsg(string msg)
        {
            loginfo.Debug(msg);
            logudp.Info(msg);
        }

        /// <summary>
        /// 网络请求数据输出Refit
        /// </summary>
        /// <param name="msg"></param>
        public static void LogNetMsg(string msg)
        {
            lognet.Info(msg);
        }

        /// <summary>
        /// 数据库操作日志输出
        /// </summary>
        /// <param name="msg"></param>
        public static void LogSqlMsg(string msg)
        {
 
            logSql.Info(msg);
        }





    }
}
