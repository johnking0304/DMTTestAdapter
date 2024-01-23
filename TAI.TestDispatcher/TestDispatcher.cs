using DMT.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using TAI.Test.Scheme;
using Newtonsoft.Json;
using DMT.DatabaseAdapter;

namespace TAI.TestDispatcher
{


    public enum MessageType
    { 
        Normal = 0,
        Warning =1,
        Error = 2,  
        Pass =3,
    }

    public enum  OperateCommand
    {
        None = 0,
        StartTest = 1,
        PaseTest = 2,
        StopTest = 3,
    }




    public class CardTestResult
    {
        public string CPUUsage { get; set; }
        public string CUNum { get; set; }
        public string ERBPosition { get; set; }
        public string ROMUsage { get; set; }

        [JsonProperty(propertyName: "acceptorName")]
        public string AcceptorName { get; set; }

        [JsonProperty(propertyName: "siteName")] 
        public string SiteName { get; set; }

        [JsonProperty(propertyName: "cardAddress")]
        public string CardAddress { get; set; }

        [JsonProperty(propertyName: "cardPosition")]
        public string CardPosition { get; set; }

        [JsonProperty(propertyName: "cardTemperature")]
        public string CardTemperature { get; set; }


        [JsonProperty(propertyName: "dateTime")]
        public string TestDateTime { get; set; }

        [JsonProperty(propertyName: "finalResult")]
        public string FinalResult { get; set; }

        [JsonProperty(propertyName: "hardWareVersion")]
        public string HardWareVersion { get; set; }

        [JsonProperty(propertyName: "signalType")]
        public string SignalType { get; set; }

        [JsonProperty(propertyName: "terminalBoardPosition")]
        public string TerminalBoardPosition { get; set; }

        [JsonProperty(propertyName: "testResult")]
        public List<TestResult> TestResults { get; set; }

        [JsonProperty(propertyName: "testerName")]
        public string TesterName { get; set; }
        [JsonProperty(propertyName: "reviewer")] 
        public string Reviewer { get; set; }

        [JsonProperty(propertyName: "tolerance")]
        public string Tolerance { get; set; }
        [JsonProperty(propertyName: "channelNum")]
        public int ChannelCount { get; set; }

        public string QRCode { get; set; }

        public CardTestResult()
        {
            this.CPUUsage = "";
            this.CUNum = "";
            this.ERBPosition = "";
            this.ROMUsage = "";
            this.AcceptorName = "";
            this.SiteName = "";
            this.CardAddress = "";
            this.CardPosition = "";
            this.CardTemperature = "";
            this.TestDateTime = "";
            this.FinalResult = "";
            this.HardWareVersion = "";
            this.SignalType = "";
            this.TerminalBoardPosition = "";
            this.TesterName = "";
            this.Reviewer = "";
            this.Tolerance = "";
            this.QRCode = "";
            this.TestResults = new List<TestResult>();
        }
        public void Assign(TestDispatcher dispatcher)
        {
            CardModule cardModule = dispatcher.CardModule;
            TestScheme scheme = dispatcher.TestScheme;
            if (cardModule.CardType == Modules.ModuleType.DO)
            {
                this.QRCode = string.Format("{0},{1}", cardModule.SerialCode, cardModule.AssistCardCode);
            }
            else 
            {
                this.QRCode = cardModule.SerialCode;
                }
            this.SignalType = cardModule.CardType.ToString();
            this.HardWareVersion = cardModule.CardVersion;
            this.CardAddress = cardModule.IPAddressText;
            this.HardWareVersion = cardModule.HardwareVersion;
            this.CardPosition = string.Format("{0},{1}", cardModule.CageNum, cardModule.CardNo);
            this.TerminalBoardPosition = string.Format("{0},{1}", cardModule.ColumnPos, cardModule.RowPos);
            this.TestDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.ChannelCount = cardModule.ChannelCount; 
            this.FinalResult = scheme.Conclusion ? "PASS" : "NG";
            
        }
    }


    public class TestResult
    {
        //目标值
        [JsonProperty(propertyName: "aim")]
        public float Aim { get; set; }

        //通道编号
        [JsonProperty(propertyName: "channel")]
        public int Channel { get; set; }

        //容差
        [JsonProperty(propertyName: "error")]
        public float Error { get; set; }

        //输入值
        [JsonProperty(propertyName: "inputValue")]
        public float InputValue { get; set; }


        //实际值
        [JsonProperty(propertyName: "realNum")]
        public float RealNum { get; set; }

        //实际值
        [JsonProperty(propertyName: "channelResult")]
        public string channelResult { get; set; }

        public void Assign(TestItemNode node)
        {
            this.Aim = node.CriteriaValue;
            this.Channel = node.ChannelId;
            this.Error = node.CriteriaTolerance;
            this.InputValue = node.SignalValue;
            this.RealNum = node.SimpleData;
        }
    }



    public class TestDispatcher : BaseController
    {
        public CardModule CardModule { get; set; }
        public TestScheme TestScheme { get; set; }

        public int ActiveTestItemIndex { get; set; }

        public int ProgressCount { get; set; }

        public int ProgressValue
        {
            get

            {
                if (this.TestScheme.ActiveItemCount > 0)
                {
                    double percent = ((float)this.ProgressCount / this.TestScheme.ActiveItemCount) * 100.0f;
                    return (int)Math.Round(percent);
                }
                return 100;


            }
        }

        public string ProgressValueContent
        {
            get
            {
                return string.Format("{0}/{1}", this.ProgressCount, this.TestScheme.ActiveItemCount);
            }
        }

        public bool TestCompleted { get; set; }

        public TestState TestState { get; set; }
        public TestDispatcher(CardModule cardModule)
        {
            this.ActiveTestItemIndex = 0;
            this.ProgressCount = 0;
            this.CardModule = cardModule;
            this.TestScheme= new TestScheme(this.CardModule.CardType);           
        }

        public void Dispose()
        {
            this.Clear();
        }

        public bool Initialize()
        {   bool reuslt = this.TestScheme.LoadSchemeFromDatabase();

            try
            {
                for (int i = 0; i < this.TestScheme.TestItems.Count; i++)
                {
                    TestItemNode node = this.TestScheme.TestItems[i];
                    string nuconLabel = this.CardModule.PointNames[node.ChannelId - 1];
                    if (node.SignalItem.SignalTypeFrom == SignalType.NuCON)
                    {
                        node.SignalItem.SignalFromPort = nuconLabel;
                    }
                    else if (node.SignalItem.SignalTypeTo == SignalType.NuCON)
                    {
                        node.SignalItem.SignalToPort = nuconLabel;
                    }
                }
                this.CardModule.ChannelCount = this.CardModule.PointNames.Count;
                string fileName = string.Format("{0}.ini", this.CardModule.CardType.ToString());
                this.TestScheme.LoadFromFile(Path.Combine(Constants.Contant.CARD_MODULES, fileName));
                return reuslt;
            }
            catch
            {
                return false;
            }
        }

        public TestItemNode ActiveTestItem
        {
            get
            {
                if ((this.ActiveTestItemIndex < this.TestScheme.TestItems.Count) && (this.ActiveTestItemIndex >= 0))
                {
                    var item = this.TestScheme.TestItems[this.ActiveTestItemIndex];
                    if (item.Enable)
                    {
                        return item;
                    }

                }

                return null;
            }
        }

        public void SaveConclusion()
        {
            this.CardModule.CardTestResult.Assign(this);
            this.CardModule.CardTestResult.TestResults.Clear();
            foreach (TestItemNode node in this.TestScheme.TestItems)
            {
                if (node.Enable)
                {
                    TestResult result = new TestResult();
                    result.Assign(node);
                    result.channelResult = node.Conclusion? "Pass":"Fail";
                    this.CardModule.CardTestResult.TestResults.Add(result);
                }
            }


            Card card = new Card();
            card.SN = this.CardModule.CardTestResult.QRCode;
            card.ProductId = this.CardModule.CardType.ToString();
            card.Tested = true;
            card.IsPassed = this.TestScheme.Conclusion;
            card.TestResult = JsonConvert.SerializeObject(this.CardModule.CardTestResult);
            card.TestTimeStamp = DateTime.Now;

            MysqlSugarContext.MysqlSugarDB.Insertable<Card>(card).ExecuteCommand();
        }


        public void ProcessAction()
        {
            if (this.ActiveTestItemIndex < this.TestScheme.TestItems.Count)
            {
                if (this.ActiveTestItem != null)
                {
                    string message = "";
                    bool result = this.ActiveTestItem.SetChannelValue((int)this.CardModule.CardType, this.ActiveTestItem.SignalItem.SignalFromPort, (int)this.TestScheme.ChannelDataType, this.ActiveTestItem.SignalValue, ref message);
                    this.NotifyMessage(message);
                    this.Delay(this.TestScheme.ProcessIntervalMillisecond);
                    this.NotifyMessage(string.Format("等待[{0}]ms", this.TestScheme.ProcessIntervalMillisecond));
                    float dataValue = 0;
                    this.ActiveTestItem.GetChannelValue((int)this.CardModule.CardType, this.ActiveTestItem.SignalItem.SignalToPort, (int)this.TestScheme.ChannelDataType, ref dataValue, ref message);
                    this.NotifyMessage(message);
                    this.ActiveTestItem.ProcessConclusion(dataValue, ref message);
                    this.NotifyMessage(message);
                    message = string.Format("比对结果[{0}]", this.ActiveTestItem.Conclusion ? "PASS" : "NG");
                    this.NotifyMessage(message, this.ActiveTestItem.Conclusion ? MessageType.Pass : MessageType.Error);
                    this.ProgressCount++;
                   }
                this.ActiveTestItemIndex += 1;
            }
            else
            {
                this.CardModule.TestCompleted = true;
                this.TestCompleted = true;
            }


        }


        public override void ProcessEvent()
        {
            if (this.TestState != null)
            {
                this.TestState.Execute();
            }
        }

        public void NotifyMessage(string message,MessageType messageType=MessageType.Normal)
        {

            this.Notify((int)NotifyEvents.Message,this.CardModule.Description, messageType.ToString(), this.CardModule,message);
        }


        public void Clear()
        {
            
            this.Terminated = false;
            this.ActiveTestItemIndex = 0;
            this.TestCompleted = false;
            this.CardModule.TestCompleted = false;
            foreach (var item in this.TestScheme.TestItems)
            {
                item.Clear();
            }
            this.NotifyMessage("测试板卡数据初始化完成");
        
        }

        public void InitializeWorking()
        {
            this.Clear();
            this.ProgressCount = 0;
            this.TestState = new IdleTestState(this);
            this.StartThread();

        }


        public void StartTest()
        {
            if (this.TestState != null)
            {
                switch (this.TestState.TestingState)
                {
                    case TestingState.Finish:
                        {
                            this.InitializeWorking();
                            break;
                        }
                    case TestingState.Testing:
                        {
                            return;
                        }
                }
                this.TestState.ActiveCommand = OperateCommand.StartTest;
            }
        }

        public void StopTest()
        {
            if (this.TestState != null)
            {
                this.TestState.ActiveCommand = OperateCommand.StopTest;
            }
        }

        public void PauseTest()
        {
            if (this.TestState != null)
            {
                this.TestState.ActiveCommand = OperateCommand.PaseTest;
            }
        }

    }
}
