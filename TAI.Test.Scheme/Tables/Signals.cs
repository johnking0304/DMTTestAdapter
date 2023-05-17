using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
namespace Test.Scheme
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("signaltable")]
    public class Signals
    {
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalid" ,IsPrimaryKey = true ,IsIdentity = true  )]
         public int Signalid { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalname"    )]
         public string Signalname { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="namezh"    )]
         public string Namezh { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalfrom"    )]
         public string Signalfrom { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalfromport"    )]
         public string Signalfromport { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalto"    )]
         public string Signalto { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signaltoport"    )]
         public string Signaltoport { get; set; }
    }
}
