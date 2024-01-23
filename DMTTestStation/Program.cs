using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TAI.StationManager;
using DMT.Core.Utils;
using System.Windows.Media.Converters;

namespace DMTTestStation
{
    static class Program
    {
        public static StationManager StationManager { get; set; }

        public static bool FormMainIsExist = false;

        public static bool FormLoginIsExist = false;


        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (SingleInstance.AlreadyRunning())
            {
                return;
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormLogin());
        }
    }
}
