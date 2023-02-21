//---------- Copyright (C) 2009 by Oy Beamex Ab, Pietarsaari Finland ----------
//
// FILE:     Log.cs
//
// AUTHOR:   MGB 15.10.2010
//
// ABSTRACT: Debug window and file logging for debug purposes.
//
//-----------------------------------------------------------------------------

using System.Threading;
using System.IO;
using System.Diagnostics;

namespace BeamexDotNetUsbLowLib
{

    public class Log
    {
        public enum Level
        {
            ERROR = 0,
            INFO,
            DEBUG,
        }

#if TRACE
        private static Mutex m_Mutex = new Mutex();
        private static StreamWriter m_File = null;
        private static Level m_Detail = Level.DEBUG;
#endif

	    static Log()
        {
#if TRACE
            string filename;
            // filename = Directory.GetCurrentDirectory();
            // filename += "\\BeamexDotNetUsbLowLib_log.txt";
            filename = "C:\\temp\\BeamexDotNetUsbLowLib_log.txt";

            try
            {
                m_File = new StreamWriter(filename, true);
            }
            catch
            {
                m_File = null;
            }
            WriteLine("############## Log opened ##############", Level.ERROR);
#endif
        }

        public static void WriteLine(string text, Level detail)
        {
#if TRACE
            if (detail <= m_Detail)
            {
                System.DateTime now = System.DateTime.Now;

                m_Mutex.WaitOne();

                if (m_File != null)
                {
                    try
                    {
                        m_File.WriteLine("" + 
                            now.Hour.ToString("D2") + ":" +
                            now.Minute.ToString("D2") + ":" +
                            now.Second.ToString("D2") + "." +
                            now.Millisecond.ToString("D3") + "  " +
                            "(TID " + System.Threading.Thread.CurrentThread.ManagedThreadId + ") " +
                            text);
                        m_File.Flush();
                    }
                    catch
                    {
                        // Ignore
                    }
                }
                Debug.WriteLine(text);

                m_Mutex.ReleaseMutex();
            }
#endif
        }

    }


}

