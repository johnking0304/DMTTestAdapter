using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Channels;

namespace DMT.Core.LogManager
{
    public class LogManager
    {
        public static UDPClientChannel Channel { get; set; }
        public bool UDPEnable { get; set; }


        public static void LogMessage(string message)
        { 
            
        }

    }
}
