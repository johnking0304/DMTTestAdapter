//---------- Copyright (C) 2009 by Oy Beamex Ab, Pietarsaari Finland ----------
//
// FILE:     UsbDeviceFactory.cs
//
// AUTHOR:   MGB 14.10.2009
//
// ABSTRACT: Factory for creating the appropriate USB device object for a 
//           given Beamex USB device.
//
//-----------------------------------------------------------------------------

namespace TAI.Device.MC6
{

    public class UsbDeviceFactory
    {

        //---------------------------------------------------------------------
        // Create UsbDevice object instance based on device interface path.
        // null is returned if the function fails.
        //
        // Note! The instance is only created. The caller must manually open 
        //       it with Open().
        //---------------------------------------------------------------------
        public static IUsbDevice CreateInstance(string device_interface_path)
        {
            BeamexUsbPids pid = Enumerator.FindUsbPidInDeviceInterfacePath(device_interface_path);
            return CreateInstance(pid);
        }

        //---------------------------------------------------------------------
        // Create UsbDevice object instance based on USB PID.
        // null is returned if the function fails.
        //
        // Note! The instance is only created. The caller must manually open 
        //       it with Open().
        //---------------------------------------------------------------------
        public static IUsbDevice CreateInstance(BeamexUsbPids pid)
        {
            IUsbDevice dev = null;

            switch (pid)
            {
                case BeamexUsbPids.MC2PE_NORMAL_PID:
                case BeamexUsbPids.MC2PE_BOOTBLOCK_PID:
                case BeamexUsbPids.MC2MF_NORMAL_PID:
                case BeamexUsbPids.MC2MF_BOOTBLOCK_PID:
                case BeamexUsbPids.MC4PE_NORMAL_PID:
                case BeamexUsbPids.MC4PE_BOOTBLOCK_PID:
                case BeamexUsbPids.MC4MF_NORMAL_PID:
                case BeamexUsbPids.MC4MF_BOOTBLOCK_PID:
                case BeamexUsbPids.MC2_IS_NORMAL_PID:
                case BeamexUsbPids.MC2_IS_BOOTBLOCK_PID:
                    dev = new MC4UsbDevice();
                    break;

                case BeamexUsbPids.MC6_NORMAL_PID:
                    dev = new MC6UsbDevice();
                    break;
            }

            return dev;
        }

    }

}

