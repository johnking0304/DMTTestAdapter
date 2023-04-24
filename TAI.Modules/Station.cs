using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DMT.Core.Utils;
using System.IO;

namespace TAI.Modules
{
    public enum DeviceType
    {
        [Description("Z200")]
        Z200 = 0,
        [Description("N200")]
        N200 = 100,
    }

    public enum StationType
    {
        [Description("DI测试工位")]
        DI = 1,     //数字量输入模块（DI）	        pDI240，24通道
        [Description("DO测试工位")]
        DO = 2,     //数字量输出模块（DO）	        pDO160，16通道
        [Description("PI测试工位")]
        PI = 3,     //脉冲量输入模块（PI）	        pPI080，8通道，24V/48V 、5V两种类型
        [Description("AI测试工位")]
        AI = 4,     //模拟量输入模块（AI）	        nAI160，16通道
        [Description("AO测试工位")]
        AO = 5,     //模拟量输出模块（AO）	        nAO080，8通道       
        [Description("RTD三线测试工位")]
        RTD_3L = 6,  //热电阻温度采集模块（RTD）	nTD160_3Wire，16通道三线制
        [Description("RTD四线测试工位")]
        RTD_4L = 7,  //热电阻温度采集模块（RTD）	nTD120_4Wire，12通道四线制
        [Description("TC测试工位")]
        TC = 8,    //热电偶温度采集模块（TC）	                    8路通道 
        [Description("预热工位")]
        Prepare = 9,  // 预热
    }


    public class StationStatus
    {
        [JsonProperty(propertyName: "id")]
        public StationType StationType { get; set; }

        [JsonProperty(propertyName: "station")]
        public string station { get => this.StationType.ToString(); }

        [JsonProperty(propertyName: "status")]
        public TestStep TestStep { get; set; }

        [JsonProperty(propertyName: "statusText")]
        public string StatusText
        {
            get {
                return this.TestStep.Description();
            }
        }

        public StationStatus(StationType type)
        {
            this.StationType = type;
            this.TestStep = TestStep.Idle;
            
        }
    }




    public class Station
    {
        public string Caption { get; set; }
        public Position TestPosition { get; set; }
        public Position QRPosition { get; set; }      
        public StationType StationType { get; set; }
        public Module LinkedModule { get; set; }

        public Dictionary<int, KeyValuePair<float,float>> Compensates { get; set; }

        public TestStep TestStep {  get {

                return this.StationStatus.TestStep;
            }
            set
            {
                this.StationStatus.TestStep = value;
            } 
        }
        public bool WaitToBlanking { get; set; }





        public StationStatus StationStatus { get; set; }

        public Station(StationType stationType)
        {
            this.Caption = stationType.ToString();
            this.StationType = stationType;
            this.StationStatus = new StationStatus(this.StationType);
            this.TestStep = TestStep.Idle;
            this.TestPosition = (Position)((int)Position.StationBase + (int)stationType);
            this.QRPosition = (Position)((int)Position.StationQRBase + (int)stationType);
            this.LinkedModule = null;
            this.WaitToBlanking = false;
            this.Compensates = new Dictionary<int, KeyValuePair<float,float>>();
        }


        public void Reset()
        {
            this.Clear();
        }
        public void Clear()
        {
            this.LinkedModule = null;
            this.TestStep = TestStep.Idle;
            this.WaitToBlanking = false;           
        }

        public float CompensateValue(int channelId,float value)
        {
            if (this.Compensates.ContainsKey(channelId))
            {
                KeyValuePair<float, float> pair = this.Compensates[channelId];
                float temp = value * pair.Key + pair.Value;
                temp = value  - temp;
                LogHelper.LogInfoMsg(string.Format("物理通道[{0}]执行补偿：原始值[{1}],补偿[{2}],输出值[{3}]", channelId, value, this.Compensates[channelId], temp));
                return temp;
            }
            LogHelper.LogInfoMsg(string.Format("物理通道[{0}]无补偿", channelId));
            return value;

        }

        public void LoadCompensates(string path)
        {
            string filename = Path.Combine(path, string.Format("{0}.cfg", this.Caption));
            if (File.Exists(filename))
            {
                string content = Files.LoadFromFile(filename);
                string[] lines = content.Split(new char[2] { '\r', '\n' });
                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] values = line.Split(',');
                        int key = int.Parse(values[0]);
                        float bvalue = float.Parse(values[1]);
                        float kvalue = float.Parse(values[2]);
                        this.Compensates.Add(key, new KeyValuePair<float, float>(kvalue, bvalue));
                    }
                }

            }

        }






    }
}
