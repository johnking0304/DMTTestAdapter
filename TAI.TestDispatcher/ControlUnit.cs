using DMT.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAI.Modules;
using Newtonsoft.Json;

namespace TAI.TestDispatcher
{
    public enum CardType
    {
        gNone = -1,
        gDIA = 1,
        gDOA = 2,
        gPIA = 3,
        gAIB = 4,
        gAOB = 5,
        gTDA = 6,
        gTCB = 8,
    }


    public class CalibratorFactory
    {
        public static BaseCalibrator CreateCalibrator(CalibrateController controller,ModuleType moduleType)
        {
            BaseCalibrator calibrator = null;
            switch (moduleType)
            {
                case ModuleType.AI:
                case ModuleType.TC:
                case ModuleType.RTD3:
                case ModuleType.RTD4:
                    {
                        calibrator =  new AnalogInputModuleCalibrator(controller);
                        break;
                    }
                case ModuleType.AO:
                    {
                        calibrator = new AnalogOutputModuleCalibrator(controller);
                        break;
                    }


            }
            return calibrator;

        }

    }





    public class BaseCardModule
    {
        [JsonProperty(propertyName: "index")]
        public int Index { get; set; }

        public string cardTypeText { get; set; }

        [JsonProperty(propertyName: "cardType")]
        public string CardTypeText
        {
            get
            {
                return cardTypeText;
            }
            set
            {
                this.cardTypeText = value;
                try
                {
                    CardType cardType = (CardType)Enum.Parse(typeof(CardType), value);
                    this.CardType = (ModuleType)cardType;
                }
                catch
                {
                    this.CardType = ModuleType.None;
                }


            }
        }

        public ModuleType CardType { get; set; }
    }

    public class CardModule:BaseCardModule
    {
        [JsonProperty(propertyName: "cageNum")]
        public string CageNum { get; set; }

        public string SerialCode { get; set; }

        public string AssistCardCode { get; set; }


        [JsonProperty(propertyName: "cardNo")]
        public int CardNo { get; set; }
        public int CardChannel { get; set; }

        [JsonProperty(propertyName: "iPAddress")]
        public string IPAddress { get; set; }
        [JsonProperty(propertyName: "remark")]

        public string Remark { get; set; }
        [JsonProperty(propertyName: "cardVersion")]
        public string CardVersion { get; set; }
        
        [JsonProperty(propertyName: "pointnames")]
        public List<string> PointNames { get; set; }
        

        [JsonProperty(propertyName: "tbcolumnpos")]
        public string ColumnPos { get; set; }

        [JsonProperty(propertyName: "tbrowpos")]
        public string RowPos { get; set; }

        public string IPAddressText
        {
            get {
                return string.Format("192.168.1.{0}", this.IPAddress);
            }
        }

        public TestDispatcher TestDispatcher { get; set; }

        public CardTestResult CardTestResult { get; set; }

        public int ChannelCount { get; set; }

        public object Operator { get; set; }

        public object Node { get; set; }

        public string Description
        {
            get
            {
                string content = "";
                if (string.IsNullOrEmpty(this.SerialCode))
                {
                   content = string.Format("[----]");
                }
                else
                {
                    content = string.Format("[{0}]",this.SerialCode );
                }
                content = string.Format("{0}{1}-({2},{3})-{4}",content, CardType.Description(),this.ColumnPos,this.RowPos,this.IPAddressText);
                return content;

            }
        }



        public CardModule()
        {
            this.CardTestResult = new CardTestResult();
            
        }



        public bool IsTesting {

            get {
                try
                {
                    return this.TestDispatcher.TestState.TestingState == TestingState.Testing || this.TestDispatcher.TestState.TestingState == TestingState.Pause;
                }
                catch
                {
                    return false;
                }
                
                
            }

        }
        public bool Initialize()
        {
            this.TestDispatcher = new TestDispatcher(this);
            return this.TestDispatcher.Initialize();                        
        }


        public bool Update(string assistCardCode)
        {

            if (assistCardCode.Contains("CZ2RBS01"))
            {
                this.ChannelCount = 16;
            }
            else if (assistCardCode.Contains("CZ2RBS02"))
            {
                this.ChannelCount = 10;
            }
            else
            {
                return false;
            }

            foreach (var item in this.TestDispatcher.TestScheme.TestItems)
            {
                item.Enable = item.ChannelId <= this.ChannelCount;                
            }

            this.AssistCardCode = assistCardCode; 
            return true;
        }

       
    }

    public class CardModuleGroup: BaseCardModule
    {
        [JsonProperty(propertyName: "cards")]
        public List<CardModule> Cards { get; set; }

        [JsonProperty(propertyName: "caption")]
        public string Caption { get; set; }


        public string Description
        {
            get => CardType.Description();
        }

    }

    public class ControlUnit
    {
        
        public int Index { get; set; }

        [JsonProperty(propertyName: "cu")]
        public string Caption { get; set; }

        public string Name { get; set; }


        [JsonProperty(propertyName: "cardGroups")]
        public List<CardModuleGroup> CardGroups { get; set; }

        public ControlUnit()
        {
            this.Index = 0;
            this.Name = "控制器";
            this.CardGroups = new List<CardModuleGroup>();
        }
    }



}
