using System;
using DMT.Core.Models;


namespace TAI.Device
{


    public enum DeviceCommand
    {
        Initialize = 100,
        Identify   =101,
        GetValue  =201,
        SetValue  =201, 

    }



    public class DeviceMaster : BaseDevice
    {
        public string Identify { get; set; }

        public DeviceMaster() : base()
        {
            this.Identify = "";

        }

        public StatusMessage GetStatusMessage()
        {
            return this.StatusMessage;
        }



    }


}
