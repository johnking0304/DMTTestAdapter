using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DMTTestAdapter;
using DMT.Core.Utils;

namespace DMTTestManager
{
    static class Program
    {

        public static TestAdapter DMTTestAdapter;
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
            Application.Run(new FormMain());
        }
    }
}
