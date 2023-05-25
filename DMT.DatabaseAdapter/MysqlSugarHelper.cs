using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMT.DatabaseAdapter
{
    public class MysqlSugarHelper
    {
        /// <summary>
        /// 查询所有记录 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetAll<T>()
        {
            return MysqlSugarContext.MysqlSugarDB.Queryable<T>().ToList();
        }

        /// <summary>
        /// 查询所有记录异步
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<List<T>> GetAllAsync<T>()
        {
            return await MysqlSugarContext.MysqlSugarDB.Queryable<T>().ToListAsync();
        }

        /// <summary>
        /// 根据id查询记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Get<T>(int id)
        {
            return MysqlSugarContext.MysqlSugarDB.Queryable<T>().InSingle(id);
        }

        /// <summary>
        /// 根据name查询记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Get<T>(string name)
        {

            var items = MysqlSugarContext.MysqlSugarDB.Queryable<T>().Where("Name=" + name).ToList();
            return items.Count > 0 ? items[0] : default(T);
        }

        public static T QueryByKV<T>(string key,string value)
        {
            var items = MysqlSugarContext.MysqlSugarDB.Queryable<T>().Where(key+"=" + value).ToList();
            return items.Count > 0 ? items[0] : default(T);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Update<T>(T value) where T : class, new()
        {
            return MysqlSugarContext.MysqlSugarDB.Updateable(value).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 更新数据异步
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateAsync<T>(T value) where T : class, new()
        {
            return await MysqlSugarContext.MysqlSugarDB.Updateable(value).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long Insert<T>(T value) where T : class, new()
        {
            return MysqlSugarContext.MysqlSugarDB.Insertable(value).ExecuteCommand();
        }


        /// <summary>
        /// 插入数据异步
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<long> InsertAsync<T>(T value) where T : class, new()
        {
            return await MysqlSugarContext.MysqlSugarDB.Insertable(value).ExecuteCommandAsync();
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static long InsertList<T>(List<T> values) where T : class, new()
        {
            return MysqlSugarContext.MysqlSugarDB.Insertable(values).ExecuteCommand();
        }

        /// <summary>
        /// 删除数据记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Delete<T>(T value) where T : class, new()
        {
            return MysqlSugarContext.MysqlSugarDB.DeleteableByObject(value).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 删除所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool DeleteAll<T>() where T : class, new()
        {
            return MysqlSugarContext.MysqlSugarDB.Deleteable<T>().Where("Id > 0").ExecuteCommand() > 0;
        }

        /// <summary>
        /// 初始化数据表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool ResetTable<T>()
        {
            return MysqlSugarContext.MysqlSugarDB.DbMaintenance.TruncateTable<T>();
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalNumber"></param>
        /// <returns></returns>
        public static List<T> GetPageList<T>(int pageNumber, int pageSize, ref int totalNumber)
        {
            return MysqlSugarContext.MysqlSugarDB.Queryable<T>().ToPageList(pageNumber, pageSize, ref totalNumber);
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalNumber"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetPageListAsync<T>(int pageNumber, int pageSize, SqlSugar.RefAsync<int> totalNumber)
        {
            return await MysqlSugarContext.MysqlSugarDB.Queryable<T>().ToPageListAsync(pageNumber, pageSize, totalNumber);
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static List<T> GetPageList<T>(int pageNumber, int pageSize)
        {
            return MysqlSugarContext.MysqlSugarDB.Queryable<T>().ToPageList(pageNumber, pageSize);
        }
        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetPageListAsync<T>(int pageNumber, int pageSize)
        {
            return await MysqlSugarContext.MysqlSugarDB.Queryable<T>().ToPageListAsync(pageNumber, pageSize);
        }
    }
}
