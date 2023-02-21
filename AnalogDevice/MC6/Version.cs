//---------- Copyright (C) 2009 by Oy Beamex Ab, Pietarsaari Finland ----------
//
// FILE:     Version.cs
//
// AUTHOR:   MGB 13.10.2009
//
// ABSTRACT: Version info of USB library.
//
//-----------------------------------------------------------------------------

using System.Reflection;


namespace BeamexDotNetUsbLowLib
{
    public class UsbLibVersion
    {
        public static string VersionString()
        {
            // Modify AssemblyVersion & AssemblyFileVersion in AssemblyInfo.cs 

            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}

