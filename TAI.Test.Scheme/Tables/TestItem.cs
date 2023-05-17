using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
namespace Test.Scheme
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("testitems")]
    public class TestItem
    {
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="id" ,IsPrimaryKey = true ,IsIdentity = true  )]
         public int Id { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="productid"    )]
         public string Productid { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="name"    )]
         public string Name { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="namezh"    )]
         public string Namezh { get; set; }
        /// <summary>
        ///  
        ///</summary>
        [SugarColumn(ColumnName = "channelnum")]
        public int ChannelNum { get; set; }
        /// <summary>
        ///  
        ///</summary>
        [SugarColumn(ColumnName="level"    )]
         public int? Level { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="signalvalue"    )]
         public decimal? Signalvalue { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="criteriatype"    )]
         public int? Criteriatype { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="criteriavalue"    )]
         public decimal? Criteriavalue { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="criteriatolerance"    )]
         public decimal? Criteriatolerance { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="reportkey"    )]
         public string Reportkey { get; set; }

        [Navigate(typeof(ItemSignal), nameof(ItemSignal.Itemid), nameof(ItemSignal.Signalid))]
        public List<Signals> Signals{ get; set; }
    }
}
