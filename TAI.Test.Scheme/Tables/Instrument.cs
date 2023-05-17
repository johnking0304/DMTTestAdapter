using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;

namespace Test.Scheme
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("instrument")]
    public class Instrument
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
         [SugarColumn(ColumnName="type"    )]
         public string Type { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="port"    )]
         public string Port { get; set; }
    }
}
