using DMT.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAI.Modules;
using Newtonsoft.Json;

namespace TAI.ControlUnit
{

    public class CardModule
    {
        [JsonProperty(propertyName: "cageNum")]
        public string CageNum { get; set; }

        [JsonProperty(propertyName: "index")]
        public int Index { get; set; }
        public string cardType { get; set; }
        public ModuleType CardType { get; set; }

        [JsonProperty(propertyName: "cardNo")]
        public int CardNo { get; set; }
        public int CardChannel { get; set; }

        [JsonProperty(propertyName: "iPAddress")]
        public string IPAddress { get; set; }
        [JsonProperty(propertyName: "remark")]
        public string Remark { get; set; }
        [JsonProperty(propertyName: "cardVersion")]
        public string CardVersion { get; set; }

        public TestDispatcher. TestDispatcher




    }

    public class CardModuleGroup
    {
        [JsonProperty(propertyName: "cards")]
        public List<CardModule> Cards { get; set; }

        [JsonProperty(propertyName: "caption")]
        public string Caption { get; set; }

        public string cardType { get; set; }

        public ModuleType CardType { get; set; }
    }

    public class ControlUnit
    {
        public int Index { get; set; }

        [JsonProperty(propertyName: "cu")]
        public string Caption { get; set; }


        [JsonProperty(propertyName: "cardGroups")]
        public List<CardModuleGroup> CardGroups { get; set; }

        public ControlUnit()
        {
            this.Index = 0;
            this.Caption = "";
            this.CardGroups = new List<CardModuleGroup>();
        }
    }



}
