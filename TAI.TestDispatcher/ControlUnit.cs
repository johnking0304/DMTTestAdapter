﻿using DMT.Core.Utils;
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
        gNone =-1,
        gDIA =1,
        gDOA =2,
        gPIA=3,
        gAIB=4,
        gAOB=5,
        gTDA=6,
        gTCB=8,
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
      

        [JsonProperty(propertyName: "cardNo")]
        public int CardNo { get; set; }
        public int CardChannel { get; set; }

        [JsonProperty(propertyName: "iPAddress")]
        public string IPAddress { get; set; }
        [JsonProperty(propertyName: "remark")]
        public string Remark { get; set; }
        [JsonProperty(propertyName: "cardVersion")]
        public string CardVersion { get; set; }

        public TestDispatcher TestDispatcher{get;set;}

        public object Operator { get; set; }

        public string Description
        {
            get => string.Format("{0}-{1}:{2}",this.CardNo,this.CageNum,CardType.Description());
        }


        

        public bool Initialize()
        {
            if (this.TestDispatcher == null)
            {
                this.TestDispatcher = new TestDispatcher(this);
                this.TestDispatcher.Initialize();
            }
            this.TestDispatcher.Clear();
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
