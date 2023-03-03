using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DMT.Core.Models;
using DMT.Core.Channels;

namespace TAI.Manager.VIS
{


    public enum OCRType
    {
        ModelType = 0,
        ChannelLighting = 1,
        ModelSerialCode = 2,
    }


    //{"Module":0,"Type":"24CHDigitalInput"}

    class ModuleTypeResponse
    {
        public int Module { get; set; }
        public string Type { get; set; }
    }


    //{"Code":"KC94ES6010"}


    class ModuleSerialCodeResponse
    {
        public string Code { get; set; }
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
            return string.Format("T1");

        }



        public override bool ParseResponse(string content)
        {
            bool result = true;
            //FIXME
            switch (this.OCRType)
            {
                case OCRType.ModelType:
                    {
                        try
                        {
                            ModuleTypeResponse response = JsonConvert.DeserializeObject<ModuleTypeResponse>(content);
                            if (response.Module == 0)
                            {
                                this.controller.ModelType = response.Type;
                                result = true;
                            }
                            else
                            {
                                this.controller.ModelType = "";
                                result = false;
                            }
                        }
                        catch
                        {
                            result = false;
                        }

                        break;
                    }
                case OCRType.ModelSerialCode:
                    {
                        try
                        {
                            ModuleSerialCodeResponse response = JsonConvert.DeserializeObject<ModuleSerialCodeResponse>(content);
                            if (response.Code != "")
                            {
                                this.controller.ModelSerialCode = response.Code;
                                result = true;
                            }
                        }
                        catch
                        {
                            result = false;
                        }
                        break;
                    }
                case OCRType.ChannelLighting:
                    {
                        try
                        {
                            ChannelLightingRespone response = JsonConvert.DeserializeObject<ChannelLightingRespone>(content);
                            for (int i = 0; i < response.Channels.Count; i++)
                            {
                                this.controller.ChannelResults[i] = new KeyValuePair<string, int>(response.Channels[i].Key, response.Channels[i].Value);
                                result = true;
                            }

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
            return string.Format("PW,1,{0}", this.ProgramId);
        }

        public override bool ParseResponse(string content)
        {
            if (content == "OK")
            {
                return true;
            }
            if (content == "ER")
            {
                return false;
            }
            return false;

        }

    }
}
