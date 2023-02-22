using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAI.Constants
{
    public class Contant
    {
        public static string AppName = "TestAdapter";

        public static readonly string HOME = System.Environment.CurrentDirectory;

        public static readonly string CONFIG = Path.Combine(HOME, "config.ini");


    }
}
