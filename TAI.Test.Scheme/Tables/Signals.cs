using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
namespace TAI.Test.Scheme
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
         public int SignalId { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalname"    )]
         public string SignalName { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="namezh"    )]
         public string AliasName { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalfrom"    )]
         public string SignalFrom { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalfromport"    )]
         public string SignalFromPort { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalto"    )]
         public string SignalTo { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signaltoport"    )]
         public string SignalToPort { get; set; }



        [Navigate(typeof(ItemSignal), nameof(ItemSignal.Signalid), nameof(ItemSignal.Itemid))]
        public List<TestItem> TestItems { get; set; }
    }
}
