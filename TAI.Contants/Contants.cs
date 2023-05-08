using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace TAI.Constants
{
    public class Contant
    {
        public static string AppName = "TestAdapter";

        public static readonly string HOME = System.Environment.CurrentDirectory;

        public static readonly string CONFIG = Path.Combine(HOME, "config.ini");

        public static readonly string DIGITAL_CONFIG = Path.Combine(HOME, "digitaldevice.ini");

        public static readonly string ENVIRONMENT_CONFIG = Path.Combine(HOME, "environment.ini"); 

        public static readonly string VIS_CONFIG = Path.Combine(HOME, "viscontroller.ini");

        public static readonly string SWITCH_CONFIG = Path.Combine(HOME, "switchcontroller.ini");

        public static readonly string ANALOG_CONFIG = Path.Combine(HOME, "analogdevice.ini");

        public static readonly string PROCESS_CONFIG = Path.Combine(HOME, "processcontroller.ini");

        public static readonly string MODULE_CONFIG = Path.Combine(HOME, "module.ini");

        public static string STATIONS
        {
            get => Files.InitializePath(Path.Combine(HOME, "stations"));
        }

    }
}
