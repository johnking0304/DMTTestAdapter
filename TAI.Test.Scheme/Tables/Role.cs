using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
namespace TAI.Test.Scheme
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("role")]
    public class Role
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
    }
}
