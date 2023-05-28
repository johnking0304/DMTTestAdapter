using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAI.Modules;
using DMT.DatabaseAdapter;
using System.ComponentModel;
using TAI.Device;
using DMT.Core.Utils;

namespace TAI.Test.Scheme
{

    public delegate bool SetChannelValueDelegate(int stationId, string channelId, int dataType, float value,ref string message);

    public delegate bool GetChannelValueDelegate(int stationId, string channelId, int dataType, ref float value,ref string message);


    public enum SignalType 
    {
        None = 0,
        [Description("模拟量信号")]
        AnalogCom =1,
        [Description("数字量信号")]
        DigitalCom =2,
        [Description("NuCON信号")]
        NuCON =3,
    }


    public enum CriteriaType
    { 
         Equals= 0,
         Compare =1,
         Less = 2,
         Greater =3,    
    }

    public class TestItemNode
    {
        public int Id { get; set; }
        public string ProductId { get; set; }

        public string Name { get; set; }

        public string AliasName { get; set; }


        public int Level { get; set; }

        public float SignalValue { get; set; }

        public CriteriaType CriteriaType { get; set; }

        public float CriteriaValue { get; set; }

        public float CriteriaTolerance { get; set; }

        public string ReportKey { get; set; }

        public SignalItem SignalItem { get; set; }


        public bool Updated { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime StopDateTime { get; set; }
        public float SimpleData { get; set; }
        public bool Conclusion { get; set; }
        public string Message { get; set; }

        public SetChannelValueDelegate SetChannelValue { get; set; }
        public GetChannelValueDelegate GetChannelValue { get; set; }

        public int ChannelId { get
            {
                return this.SignalItem.ChannelId;
            }
        }


        public TestItemNode()
        {
            this.Clear();
        }
        public void Assign(TestItem item)
        {
            this.Id = item.Id;
            this.ProductId = item.ProductId;
            this.Name = item.Name;
            this.AliasName = item.AliasName;
            this.Level = item.Level;
            this.SignalValue = item.Signalvalue;
            this.CriteriaType =(CriteriaType)item.Criteriatype;
            this.CriteriaValue = item.Criteriavalue;
            this.CriteriaTolerance = item.Criteriatolerance;
            this.ReportKey = item.Reportkey;
        }

        public void Clear()
        {
            this.SimpleData = 0;
            this.Conclusion = false;
            this.Updated = false;
            this.Message = "";
        }


        private bool Equals(double value)
        {
            return DoubleUtils.AreEqual(value, this.CriteriaValue);
        }
        private bool Compare(double value)
        {
            return DoubleUtils.LessThanOrEqual(Math.Abs(value - this.CriteriaValue), this.CriteriaTolerance);
        }
        private bool Less(double value)
        {
            return DoubleUtils.LessThan(value, this.CriteriaValue);
        }
        private bool Greater(double value)
        {
            return DoubleUtils.GreaterThan(value, this.CriteriaValue);
        }

        public bool ProcessConclusion(float data)
        {
            this.SimpleData = data;
            switch (this.CriteriaType)
            {
                case CriteriaType.Equals:
                    {
                        this.Conclusion = this.Equals(data);
                        break;
                    }
                case CriteriaType.Compare:
                    {
                        this.Conclusion = this.Compare(data);
                        break;
                    }
                case CriteriaType.Less:
                    {
                        this.Conclusion = this.Less(data);
                        break;
                    }
                case CriteriaType.Greater:
                    {
                        this.Conclusion = this.Greater(data);
                        break;
                    }
                default:
                    {
                        this.Conclusion = this.Equals(data);
                        break;
                    }
            }

            this.StopDateTime = DateTime.Now;
            return this.Conclusion;
        }
        

    }

    public class SignalItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AliasName { get; set; }

        public int ChannelId { get; set; }

        public string SignalFrom { get; set; }

        public SignalType SignalTypeFrom { get; set; }

        public string SignalFromPort { get; set; }

        public string SignalTo { get; set; }

        public SignalType SignalTypeTo { get; set; }

        public string SignalToPort { get; set; }

        public List<TestItemNode> Items { get; set; }
        public SignalItem()
        {
            this.ChannelId = -1;
            this.Items = new List<TestItemNode>();
        }
        public void Assign(Signals signal)
        {
            this.Id = signal.SignalId;
            this.Name = signal.SignalName;
            this.AliasName = signal.AliasName;
            this.SignalFrom = signal.SignalFrom;
            this.SignalFromPort = signal.SignalFromPort;
            this.SignalTo = signal.SignalTo;
            this.SignalToPort = signal.SignalToPort;

            try
            {
                this.SignalTypeFrom = (SignalType)SignalType.Parse(typeof(SignalType), signal.SignalFrom);
                this.SignalTypeTo = (SignalType)SignalType.Parse(typeof(SignalType), signal.SignalTo);
            }
            catch
            {
                this.SignalTypeFrom = SignalType.None;
                this.SignalTypeTo = SignalType.None;
            }


            string channelStr = signal.SignalFromPort;
            if (this.SignalFrom.Equals("AnalogCom") || this.SignalFrom.Equals("DigitalCom"))
            {
                channelStr = signal.SignalFromPort;
            }
            else
            {
                channelStr = signal.SignalToPort;
            }
            int value = 0;
            if (int.TryParse(channelStr, out value))
            {
                this.ChannelId = value;
            }

            foreach (var node in signal.TestItems)
            {
                TestItemNode testItem = new TestItemNode();
                testItem.Assign(node);
                testItem.SignalItem = this;
                this.Items.Add(testItem);
            }
        
        }
    }



    public class TestScheme
    {        
        public string Caption { get; set; }
        public string ProducId{ get; set; }
        public List<SignalItem> SignalItems { get; set; }

        public List<TestItemNode> TestItems { get; set; }

        /// <summary>
        /// 通道收发处理间隔时间
        /// </summary>
        public int ProcessIntervalMillisecond { get; set; }
        public ChannelType ChannelDataType { get; set; }


        public TestScheme(ModuleType moduleType)
        {
            this.Caption = "TestSchemeConfig";

            this.ProducId = moduleType.ToString();

            this.SignalItems = new List<SignalItem>();

            this.TestItems = new List<TestItemNode>();

            this.ProcessIntervalMillisecond = 0;
        }
        public bool LoadSchemeFromDatabase()
        {
            this.SignalItems.Clear();
            this.TestItems.Clear();

           var List = MysqlSugarContext.MysqlSugarDB.Queryable<Signals>().Where(x=>x.SignalName.Contains(this.ProducId)).Includes(x => x.TestItems).ToList();
            foreach (var node in List)
            {
                SignalItem signal = new SignalItem();
                signal.Assign(node);
                this.SignalItems.Add(signal);
                this.TestItems.AddRange(signal.Items);
            }
            return this.TestItems.Count > 0;

        }

        public void LoadFromFile(string fileName)
        {
            this.ProcessIntervalMillisecond = IniFiles.GetIntValue(fileName, this.Caption, "ProcessIntervalMillisecond", 500);
            this.ChannelDataType = (ChannelType)IniFiles.GetIntValue(fileName, this.Caption, "ChannelDataType", (int)ChannelType.None);

            string[] list = IniFiles.GetAllSectionNames(fileName);

            if (!((System.Collections.IList)list).Contains(this.Caption))
            {
                this.SaveToFile(fileName);
            }
        }

        public void SaveToFile(string fileName)
        {
             IniFiles.WriteIntValue(fileName, this.Caption, "ProcessIntervalMillisecond", this.ProcessIntervalMillisecond);
             IniFiles.WriteIntValue(fileName, this.Caption, "ChannelDataType", (int)this.ChannelDataType);
        }


    }


    



}
