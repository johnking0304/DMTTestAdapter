using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMT.Utils
{
    public class LogHelper
    {
        public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");//这里的 loginfo 和 log4net.config 里的名字要一样
        public static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("logerror");//这里的 logerror 和 log4net.config 里的名字要一样
        public static readonly log4net.ILog lognet = log4net.LogManager.GetLogger("lognet");//网络请求日志
        public static readonly log4net.ILog logSql = log4net.LogManager.GetLogger("logsql");//数据库操作日志
        /// <summary>
        /// 输出信息日志
        /// </summary>
        /// <param name="msg"></param>
        public static void LogInfoMsg(string msg)
        {
            loginfo.Info(msg);
        }
        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="msg"></param>
        public static void LogErrMsg(string msg)
        {
            logerror.Error(msg);
        }
        /// <summary>
        /// 输出调试日志
        /// </summary>
        /// <param name="msg"></param>
        public static void LogDebugMsg(string msg)
        {
            loginfo.Debug(msg);
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
