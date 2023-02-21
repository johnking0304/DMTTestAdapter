using System;
using System.Collections.Generic;
using System.Text;

namespace DMTTestAapter
{


    public class AnalogDeviceFactory
    {

        public static DeviceMaster CreateDevice(string deviceName)
        {
            return new DeviceMaster();


        }

    }
}
