using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
namespace TAI.Test.Scheme
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("user_role")]
    public class UserRole
    {
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="id" ,IsPrimaryKey = true ,IsIdentity = true  )]
         public int Id { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="userid"    )]
         public int Userid { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="roleid"    )]
         public int Roleid { get; set; }
    }
}
