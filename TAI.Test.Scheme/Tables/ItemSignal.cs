using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
namespace Test.Scheme
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("item_signal")]
    public class ItemSignal
    {
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="id" ,IsPrimaryKey = true ,IsIdentity = true  )]
         public int Id { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="itemid"    )]
         public int? Itemid { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalid"    )]
         public int? Signalid { get; set; }
    }
}
