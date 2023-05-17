using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;


namespace Test.Scheme
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("sn_card")]
    public class Card
    {
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="id" ,IsPrimaryKey = true ,IsIdentity = true  )]
         public int Id { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="SN"    )]
         public string SN { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="productid"    )]
         public string Productid { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="productname"    )]
         public string Productname { get; set; }


        [SugarColumn(ColumnName = "channelnum")]
        public string ChannelNum { get; set; }
        /// <summary>
        ///  
        ///</summary>
        [SugarColumn(ColumnName="ispassed"    )]
         public bool Ispassed { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="tested"    )]
         public bool Tested { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="testitem_id"    )]
         public int TestitemId { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="testresult"    )]
         public string Testresult { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="testtimestamp")]
         public DateTime Testtimestamp { get; set; }

    }
}
