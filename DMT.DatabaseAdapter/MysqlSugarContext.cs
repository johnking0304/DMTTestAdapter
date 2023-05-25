using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMT.DatabaseAdapter
{
    public class MysqlSugarContext
    {
        private static string connStr { get; set; }
        private static SqlSugar.SqlSugarScope mysqlSugarDB { get; set; }

        public static SqlSugar.SqlSugarScope MysqlSugarDB
        {
            get
            {
                return mysqlSugarDB;
            }
            private set { mysqlSugarDB = value; }
        }

        /// <summary>
        /// 配置连接数据库
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="dbName"></param>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static string SetupMysql(string address, string port, string dbName, string userName, string pwd)
        {
            string connect = string.Format("server={0};Port={1};Database={2};Uid={3};Pwd={4}", address, port, dbName, userName, pwd);
            return SetupMysql(connect);
        }

        /// <summary>
        /// 配置连接数据库
        /// </summary>
        /// <param name="connection">server=localhost;Database=mysqlCodeFirst;Uid=root;Pwd=123456</param>
        /// <returns></returns>
        public static string SetupMysql(string connection)
        {
            string msg = string.Empty;
            try
            {
                ReleaseMysqlScope();
                mysqlSugarDB = new SqlSugar.SqlSugarScope(new SqlSugar.ConnectionConfig()
                {
                    DbType = SqlSugar.DbType.MySql,
                    ConnectionString = connection,
                    IsAutoCloseConnection = true,
                }, db =>
                {
                    //单例参数配置，所有上下文生效
                    db.Aop.OnLogExecuting = (s, p) =>
                    {
                    };
                });
                connStr = connection;
            }
            catch(Exception ex)
            {
                msg = ex.Message;
                ReleaseMysqlScope();
            }

            return msg;
        }

        /// <summary>
        /// 配置连接数据库
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public Task<string> SetupMysqlTask(string connection)
        {
            return Task.Run<string>(() =>
            {
                return SetupMysql(connection);
            });
        }

        /// <summary>
        /// 释放MysqlScope
        /// </summary>
        private static void ReleaseMysqlScope()
        {
            try
            {
                if (mysqlSugarDB != null)
                {
                    mysqlSugarDB.Close();
                    mysqlSugarDB.Dispose();
                }
            }
            catch { }
        }

        /// <summary>
        /// 连接Mysql测试
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="dbName"></param>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static string ConnectMysqlTest(string address,string port,string dbName,string userName,string pwd)
        {
            string connect = string.Format("server={0};Port={1};Database={2};Uid={3};Pwd={4}", address, port, dbName, userName, pwd);
            return ConnectMysqlTest(connect);
        }

        /// <summary>
        /// 连接Mysql测试
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static string ConnectMysqlTest(string connection)
        {
            string msg = string.Empty;
            try
            {
                SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = connection,
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = false//手动释放  是长连接 
                });
                if (!db.Ado.IsValidConnection())
                {
                    msg = "连接失败！";
                }

                db.Close();
                db.Dispose();
                return msg;
            }
            catch(Exception ex) { msg = ex.Message; }

            return msg;
        }
    }
}
