using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TAI.NuCONController
{

    public class NuCONController
    {
        [DllImport(@"NuCONAPI.dll", EntryPoint = "_ZN8NuCONAPI8getValueEPcdi")]
        public static extern double GetValue(IntPtr pointName, double value, int cardtype);


        [DllImport(@"NuCONAPI.dll", EntryPoint = "_ZN8NuCONAPI8setValueEdPc")]
        public static extern int SetValue(double value, StringBuilder pointName);


        public   bool SetChannelValue(int stationId, string channel, int dataType, double value)
        {
            IntPtr intPtr = Marshal.StringToHGlobalAnsi(channel);
            StringBuilder sb1 = new StringBuilder(channel);
            try
            {
                int result = SetValue(value, sb1);
                return result >= 0;
            }
            catch
            {
                return false;
            }
        }

        public   double GetChannelValue(int stationId, string channel, int dataType, out double value)
        {
            value = 0.0f;
            int moduleType = stationId - 1;//偏移 -1
            IntPtr intPtr = Marshal.StringToHGlobalAnsi(channel.ToString());
            try
            {
                double data = GetValue(intPtr, value, moduleType);
                return data;
            }
            catch {
                return -1;
            }
        }


    }
}
