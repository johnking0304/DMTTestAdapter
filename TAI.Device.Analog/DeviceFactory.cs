using System;
using System.Collections.Generic;
using System.Text;


namespace TAI.Device
{


    public class AnalogDeviceFactory
    {

        public static IAnalogDevice CreateDevice(DeviceModel deviceModel)
        {
            IAnalogDevice device = null;
            switch (deviceModel)
            {
                case DeviceModel.BeamexMC6:
                    {
                        device = new BeamexMC6();
                        break;
                    }
                case DeviceModel.Fluke7526:
                    {
                        device = new Fluke7526();
                        break;
                        
                    }
                case DeviceModel.Fluke8846:
                    {
                        device = new Fluke8846();
                        break;

                    }
            }
            return device;

        }

    }
}
