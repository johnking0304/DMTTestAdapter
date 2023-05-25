using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;

namespace TAI.Test.Scheme
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("criteria")]
    public class Criteria
    {
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="criteriatype" ,IsPrimaryKey = true   )]
         public int Criteriatype { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="value"    )]
         public decimal? Value { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="torlance"    )]
         public decimal? Torlance { get; set; }
    }
}
