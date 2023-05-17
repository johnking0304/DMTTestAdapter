using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
namespace Test.Scheme
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("role_menu")]
    public class RoleMenu
    {
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="id" ,IsPrimaryKey = true ,IsIdentity = true  )]
         public int Id { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="roleid"    )]
         public int Roleid { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="menuid"    )]
         public int Menuid { get; set; }
    }
}
