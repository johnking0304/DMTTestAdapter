using DMT.Core.Utils;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DMT.DatabaseAdapter
{
    public class MysqlConfig
    {
        public string Section { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public MysqlConfig(string section)
        {
            this.Section = section;
            this.Address = "127.0.0.1";
            this.Port = "3306";
            this.DatabaseName = "DataBaseName";
            this.UserName = "root";
            this.Password = "123456";
        }

        public void LoadFromFile(string fileName)
        {

            this.Address = IniFiles.GetStringValue(fileName, Section, "Address", "127.0.0.1");
            this.Port = IniFiles.GetStringValue(fileName, Section, "Port", "3306");
            this.DatabaseName = IniFiles.GetStringValue(fileName, Section, "DatabaseName", "DataBaseName");
            this.UserName = IniFiles.GetStringValue(fileName, Section, "UserName", "root");
            this.Password = IniFiles.GetStringValue(fileName, Section, "Password", "123456");

            string[] list = IniFiles.GetAllSectionNames(fileName);
            if (!list.Contains(Section))
            {
                this.SaveToFile(fileName);
            }
        }
        public void SaveToFile(string fileName)
        {
            IniFiles.WriteStringValue(fileName, Section, "Address", this.Address);
            IniFiles.WriteStringValue(fileName, Section, "Port", this.Port);
            IniFiles.WriteStringValue(fileName, Section, "DatabaseName", this.DatabaseName);
            IniFiles.WriteStringValue(fileName, Section, "UserName", this.UserName);
            IniFiles.WriteStringValue(fileName, Section, "Password", this.Password);
        }



    }



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
                return msg;
            }

            return "";
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
