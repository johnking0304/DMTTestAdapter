using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DMT.Core.Models;
using DMT.Core.Channels;
using DMT.Core.Utils;

namespace TAI.Manager.VIS
{


    public enum OCRType
    {
        ModelType = 0,
        ChannelLighting = 1,
        ModelSerialCode = 2,
    }


    //{"Module":0,"Type":"24CHDigitalInput"}

    //{"Module":0,"Type":[{"Key":"DI","Value":1},{"Key":"DO","Value":1},{"Key":"PI","Value":1},{"Key":"AI","Value":1},
    //{"Key":"AO","Value":0},{"Key":"RTD","Value":1},{"Key":"RTD","Value":1},{"Key":"TC","Value":1}]}

    class ModuleTypeResponse
    {
        public int Module { get; set; }
        public List<KeyValuePair<string, int>> Type { get; set; }

    }


    //{"Code":"KC94ES6010"}


    class ModuleSerialCodeResponse
    {
        public string Code { get; set; }
        public ModuleSerialCodeResponse()
        {
            this.Code = "";
        }
    }


    //{"Channels":[{"Key":"CH1","Value":0},{"Key":"CH2","Value":0},{"Key":"CH3","Value":0},{"Key":"CH4","Value":1}]}
    class ChannelLightingRespone
    {

        public List<KeyValuePair<string, int>> Channels { get; set; }

        public ChannelLightingRespone()
        {
            this.Channels = new List<KeyValuePair<string, int>>();
        }
    }

    public class VISCommand : BaseCommand
    {
        public VISCommand(VISController device) : base(device)
        {

        }

        public VISController controller
        {
            get
            {
                return (VISController)this.Device;
            }
        }

    }

    public class OCRecognizeCommand : VISCommand
    {
        private OCRType OCRType { get; set; }
        public OCRecognizeCommand(VISController device, OCRType ocrType) : base(device)
        {
            this.OCRType = ocrType;
        }
        public override string PackageString()
        {
            return string.Format("T1\r\n");

        }



        public override bool ParseResponse(string content)
        {
            
            bool result = true;

            int index = content.IndexOf("{");

            if (index < 0)
            {
                return false;
            }
            content = content.Substring(index);
            try
            {
                switch (this.OCRType)
                {
                    case OCRType.ModelType:
                        {
                            try
                            {
                                ModuleTypeResponse response = JsonConvert.DeserializeObject<ModuleTypeResponse>(content);
                                if (response.Module == 0)
                                {
                                    this.controller.ModelType = "";
                                    foreach (KeyValuePair<string, int> pair in response.Type)
                                    {
                                        if (pair.Value == 0)
                                        {
                                            this.controller.ModelType = pair.Key;
                                            break;
                                        }
                                    }                                  
                                    result = this.controller.ModelType != "";
                                }
                                else
                                {
                                    this.controller.ModelType = "";
                                    result = false;
                                }
                            }
                            catch
                            {
                                LogHelper.LogErrMsg(string.Format("解析模块类型识别值出错：{0}",content));
                                result = false;
                            }

                            break;
                        }
                    case OCRType.ModelSerialCode:
                        {
                            try
                            {
                                ModuleSerialCodeResponse response = JsonConvert.DeserializeObject<ModuleSerialCodeResponse>(content);
                                if (response.Code!=null  && response.Code != "")
                                {
                                    this.controller.ModelSerialCode = response.Code.Trim();
                                    result = true;
                                }
                            }
                            catch
                            {
                                LogHelper.LogErrMsg(string.Format("解析模块二维码识别值出错：{0}", content));
                                result = false;
                            }
                            break;
                        }
                    case OCRType.ChannelLighting:
                        {
                            try
                            {
                                this.controller.LightingContent = content;
                                return true;
/*                                ChannelLightingRespone response = JsonConvert.DeserializeObject<ChannelLightingRespone>(content);
                                for (int i = 0; i < response.Channels.Count; i++)
                                {
                                    this.controller.ChannelResults.Add(new KeyValuePair<string, int>(response.Channels[i].Key, response.Channels[i].Value));
                                    result = true;
                                }*/

                            }
                            catch
                            {
                                result = false;
                            }
                            break;
                        }

                }
                return result;
            }
            catch
            {
                return false;
            }
        }



    }


    public class SwitchProgramCommand : VISCommand
    {
        private int ProgramId { get; set; }
        public SwitchProgramCommand(VISController device, int programId) : base(device)
        {
            this.ProgramId = programId;
        }
        public override string PackageString()
        {
            return string.Format("PW,1,{0}\r\n", this.ProgramId);
        }

        public override bool ParseResponse(string content)
        {
            if (content.Contains("PW"))
            {
                return true;
            }
            if (content.Contains("ER"))
            {
                return false;
            }
            return false;

        }

    }
}
