using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
namespace TAI.Test.Scheme
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("user")]
    public class User
    {
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="id" ,IsPrimaryKey = true ,IsIdentity = true  )]
         public int Id { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="name"    )]
         public string Name { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="password"    )]
         public string Password { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="role"    )]
         public string Role { get; set; }
    }
}
