//---------- Copyright (C) 2009 by Oy Beamex Ab, Pietarsaari Finland ----------
//
// FILE:     Enumerator.cs
//
// AUTHOR:   MGB 13.10.2009, based on older Bemaex code by MKA?
//
// ABSTRACT: Functions for enumerating connected Beamex USB devices.
//
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;


namespace TAI.Device.MC6
    
{

    //-------------------------------------------------------------------------
    // USB PID (Product ID) for Beamex USB devices
    //-------------------------------------------------------------------------
    public enum BeamexUsbPids : uint
	{
		MC2PE_NORMAL_PID	= 0x0200,
		MC2PE_BOOTBLOCK_PID	= 0x0201,
		MC2MF_NORMAL_PID	= 0x0202,
		MC2MF_BOOTBLOCK_PID	= 0x0203,
        MC4PE_NORMAL_PID    = 0x0400,
        MC4PE_BOOTBLOCK_PID = 0x0401,
        MC4MF_NORMAL_PID    = 0x0402,
        MC4MF_BOOTBLOCK_PID = 0x0403,
        MC2_IS_NORMAL_PID   = 0x0212,
        MC2_IS_BOOTBLOCK_PID= 0x0213,
        MC6_NORMAL_PID      = 0x0600,
		UNKNOWN_PID			= 0xFFFFFFFF,
	}



    public class Enumerator
	{

        //---------------------------------------------------------------------
        // Find the USB PID in the given device interface path.
        //---------------------------------------------------------------------
        public static BeamexUsbPids FindUsbPidInDeviceInterfacePath(string device_interface_path)
		{
			try
			{
                if (device_interface_path.Length < 9)
                {
                    return BeamexUsbPids.UNKNOWN_PID;
                }

                int pos = device_interface_path.IndexOf("&pid_");

                if (pos < 0)
                {
                    return BeamexUsbPids.UNKNOWN_PID;
                }

                string sub_string = device_interface_path.Substring(pos + 5, 4);

                return (BeamexUsbPids)uint.Parse(sub_string, System.Globalization.NumberStyles.HexNumber);
			}
			catch
			{
                return BeamexUsbPids.UNKNOWN_PID;
			}
		}


        //---------------------------------------------------------------------
        // Count number of Beamex USB devices that are currently connected.
        // Only the devices listed in 'pids' are counted.
        //---------------------------------------------------------------------
        public static int CountBeamexUsbDevices(BeamexUsbPids[] pids)
		{
            return GetBeamexUsbDeviceNames(pids).Count;
		}


        //---------------------------------------------------------------------
        // Get the names (device interface paths) of Beamex USB devices that are 
        // currently connected. Only the devices listed in 'pids' are added to
        // the returned string array.
        //---------------------------------------------------------------------
        public static StringCollection GetBeamexUsbDeviceNames(BeamexUsbPids[] pids)
		{

            #if TRACE
            Log.WriteLine("Enumerator.GetBeamexUsbDeviceNames(" + pids.Length + " pids) enter function (" + (IntPtr.Size * 8) + " bit env)" , Log.Level.DEBUG);
            #endif

#if TRACE
            try
			{
#endif
				StringCollection  names = new StringCollection();
                List<Win.Guid>    guids = new List<Win.Guid>();

                // Find unique GUID's to scan
                foreach (BeamexUsbPids p in pids)
                {
                    Win.Guid guid = GetBeamexUsbDeviceGuid(p);
                    bool found = false;
                    foreach (Win.Guid g in guids)
                    {
                        if (g == guid)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        guids.Add(guid);
                    }
                }

                #if TRACE
                Log.WriteLine("Enumerator.GetBeamexUsbDeviceGuid() found " + guids.Count + " guids", Log.Level.DEBUG);
                int i = 0;
                #endif

                // Loop through each GUID
                foreach (Win.Guid guid in guids)
                {
                    Win.DeviceInterfaceData interface_data;
                    Win.HDEVINFO hardware_info;

                    interface_data.Flags = 0;
                    interface_data.Reserved = UIntPtr.Zero;
                    interface_data.Size = 0;
                    interface_data.InterfaceClassGuid = guid;

                    #if TRACE
                    Log.WriteLine("Win.GetHardwareDeviceInfo(guid #" + i + ") enter", Log.Level.DEBUG);
                    ++i;
                    #endif
                    hardware_info = Win.GetHardwareDeviceInfo(guid);
                    if (hardware_info == Win.HDEVINFO.INVALID_HANDLE_VALUE)
                    {
                        #if TRACE
                        Log.WriteLine("Win.GetHardwareDeviceInfo() returned nothing", Log.Level.DEBUG);
                        #endif
                        continue;
                    }

                    // Loop through each USB device found in this GUID
                    // Loop max 1024 times, so we won't get stuck in here...
                    for (uint index = 0; index < 1024; index++)
                    {
                        interface_data.Size = (uint)Marshal.SizeOf(interface_data);

                        #if TRACE
                        Log.WriteLine("Win.EnumDeviceInterfaces() enter", Log.Level.DEBUG);
                        #endif
                        if (Win.EnumDeviceInterfaces(hardware_info, guid, index, ref interface_data) != Win.ReturnCodes.ERROR_SUCCESS)
                        {
                            #if TRACE
                            Log.WriteLine("Win.EnumDeviceInterfaces() failed, ending loop", Log.Level.DEBUG);
                            #endif
                            break;
                        }

                        #if TRACE
                        Log.WriteLine("Win.GetDeviceNamePath() enter", Log.Level.DEBUG);
                        #endif
                        string path = Win.GetDeviceNamePath(hardware_info, interface_data);
                        if ((path == null) || (path.Length < 1))
                        {
                            #if TRACE
                            Log.WriteLine("Win.GetDeviceNamePath() returned nothing", Log.Level.DEBUG);
                            #endif
                            continue;
                        }
                        #if TRACE
                        Log.WriteLine("Win.GetDeviceNamePath() returned " + path, Log.Level.DEBUG);
                        #endif

                        BeamexUsbPids local_pid = FindUsbPidInDeviceInterfacePath(path);
                        foreach (BeamexUsbPids p in pids)
                        {
                            if (p == local_pid)
                            {
                                // Filter out multiple entrys of the same type (can this really happen?)
                                bool found = false;
                                foreach (string n in names)
                                {
                                    if (n == path)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (found)
                                {
                                    #if TRACE
                                    Log.WriteLine("Win.GetBeamexUsbDeviceNames() path already stored, skipping", Log.Level.DEBUG);
                                    #endif
                                }
                                else
                                {
                                    names.Add(path);
                                }
                                break;
                            }
                        }
                    }

                    #if TRACE
                    Log.WriteLine("Win.DestroyDeviceInfoList() enter", Log.Level.DEBUG);
                    #endif
                    Win.DestroyDeviceInfoList(hardware_info);
                    #if TRACE
                    Log.WriteLine("Win.DestroyDeviceInfoList() returned", Log.Level.DEBUG);
                    #endif
                }

                #if TRACE
                Log.WriteLine("Enumerator.GetBeamexUsbDeviceNames() leave function. " + names.Count + " items found", Log.Level.DEBUG);
                #endif

                return names;

#if TRACE
			}
			catch (Exception e)
			{
                Log.WriteLine("Enumerator.GetBeamexUsbDeviceNames() exception: " + e.ToString(), Log.Level.ERROR);
				throw;
			}
#endif

        }


        //---------------------------------------------------------------------
        // Get the Beamex USB driver GUID (global unique identifier) 
        // corresponding to the given Beamex USB PID.
        //---------------------------------------------------------------------
        private static Win.Guid GetBeamexUsbDeviceGuid(BeamexUsbPids pid)
        {
            // MC2/MC4 driver guid {209C2DAD-D2B3-4ba8-ACD0-9574A43CCDA0} 
            Win.Guid Mc2Mc4DriverGuid = new Win.Guid(0x209c2dad, 0xd2b3, 0x4ba8, 0xac, 0xd0, 0x95, 0x74, 0xa4, 0x3c, 0xcd, 0xa0);

            // MC6 driver guid {17C9138D-DF81-4EE2-BF9E-04154F8C374E}
            Win.Guid Mc6DriverGuid = new Win.Guid(0x17C9138D, 0xDF81, 0x4EE2, 0xBF, 0x9E, 0x04, 0x15, 0x4F, 0x8C, 0x37, 0x4E);

            switch (pid)
            {
                default:
                    return Mc2Mc4DriverGuid;

                case BeamexUsbPids.MC6_NORMAL_PID:
                    return Mc6DriverGuid;
            }
        }

	}
}
