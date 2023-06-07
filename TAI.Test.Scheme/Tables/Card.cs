using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;


namespace TAI.Test.Scheme
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
         public string ProductId { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="productname"    )]
         public string ProductName { get; set; }

        /// <summary>
        ///  
        ///</summary>
        [SugarColumn(ColumnName="ispassed"    )]
         public bool IsPassed { get; set; }
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
         public string TestResult { get; set; }
        /// <summary>
        ///  
        ///</summary>
         [SugarColumn(ColumnName="testtimestamp")]
         public DateTime TestTimeStamp { get; set; }

    }
}
