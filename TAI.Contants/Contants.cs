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

        public static readonly string CALIBRATOR_CONFIG = Path.Combine(HOME, "calibrator.ini");

        public static readonly string MODULE_CONFIG = Path.Combine(HOME, "module.ini");

        public static readonly string TEST_RESULT = Path.Combine(HOME, "testresult.txt");

        public static string STATIONS
        {
            get => Files.InitializePath(Path.Combine(HOME, "stations"));
        }

        public static readonly string ControlUnitExportResultTool = Path.Combine(HOME, "TestReportExport.exe");

        public static readonly string ControlUnitParseTool = Path.Combine(HOME, "Data\\CU\\CUParseTool.exe");

        public static readonly string ControlUnitInfoFile = Path.Combine(HOME, "Data\\CU\\ControlUnitInfo.txt");
        public static string CARD_MODULES
        {
            get => Files.InitializePath(Path.Combine(HOME, "modules"));
        }

            }
}
