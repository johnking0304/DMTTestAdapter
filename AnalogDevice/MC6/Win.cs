//---------- Copyright (C) 2009 by Oy Beamex Ab, Pietarsaari Finland ----------
//
// FILE:     Win.cs
//
// AUTHOR:   MGB 9.10.2009, based on older Bemaex code by MKA?
//
// ABSTRACT: Interface class towards the native Windows (Win32) API.
//
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Text;


namespace BeamexDotNetUsbLowLib
{

    public class Win
    {

        // Native Windows error codes
        public enum ReturnCodes : uint
        {
            ERROR_SUCCESS                 = 0,      // The operation completed successfully.
            ERROR_FILE_NOT_FOUND          = 2,      // The system cannot find the file specified.
            ERROR_ACCESS_DENIED           = 5,      // Access is denied.
            ERROR_INVALID_HANDLE          = 6,      // The handle is invalid.
            ERROR_INVALID_DATA            = 13,     // The data is invalid.
            ERROR_NOT_READY               = 21,     // The device is not ready.
            ERROR_WRITE_FAULT             = 29,     // The system cannot write to the specified device.
            ERROR_READ_FAULT              = 30,     // The system cannot read from the specified device.
            ERROR_GEN_FAILURE             = 31,     // A device attached to the system is not functioning.
            ERROR_SHARING_VIOLATION       = 32,     // The process cannot access the file because it is being used by another process.
            ERROR_ALREADY_ASSIGNED        = 85,     // The local device name is already in use.
            ERROR_INVALID_PARAMETER       = 87,     // The parameter is incorrect.
            ERROR_OPEN_FAILED             = 110,    // The system cannot open the device or file specified.
            ERROR_INSUFFICIENT_BUFFER     = 122,    // The data area passed to a system call is too small.
            ERROR_MOD_NOT_FOUND           = 126,    // The specified module could not be found.
            ERROR_BUSY                    = 170,    // The requested resource is in use.
            WAIT_TIMEOUT                  = 258,
            ERROR_NO_MORE_ITEMS           = 259,    // No more data is available.
            ERROR_IO_PENDING              = 997,    // Overlapped I/O operation is in progress.
            ERROR_SERVICE_ALREADY_RUNNING = 1056,   // An instance of the service is already running.
            ERROR_DEVICE_NOT_CONNECTED    = 1167,   // The device is not connected.
            ERROR_INVALID_USER_BUFFER     = 1784,   // The supplied user buffer is not valid for the requested operation.
            // This in not all windows system error codes, other values may exist!
        }


        //---------------------------------------------------------------------
        // Create file in non-overlapped or overlapped mode
        //---------------------------------------------------------------------
        internal static HANDLE CreateFile(string filename, bool overlapped)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(filename);

            unsafe
            {
                fixed (byte * p_byte = &bytes[0])
                {
                    return (HANDLE)CreateFile(
                                (sbyte *)p_byte, 
                                GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE,
                                0, 
                                OPEN_EXISTING, 
                                (overlapped == false) ? FILE_ATTRIBUTE_NORMAL : FILE_FLAG_OVERLAPPED, 
                                0);
                }
            }
        }


        //---------------------------------------------------------------------
        // Close handle to Windows native object
        //---------------------------------------------------------------------
        internal static void CloseHandle(HANDLE hObject)
        {
            CloseHandle((uint)hObject);
        }


        //---------------------------------------------------------------------
        // Convert system error code to text message
        //---------------------------------------------------------------------
        public static string GetSystemErrorCodeAsString(ReturnCodes code)
        {
            // First check if USBIO specific error
            for (int i = 0; i < UsbIo.m_ErrorCodeTexts.Length; i++)
            {
                if (UsbIo.m_ErrorCodeTexts[i].code == (uint)code)
                {
                    return UsbIo.m_ErrorCodeTexts[i].text;
                }
            }

            sbyte[] msg = new sbyte[4096];
            string msg_string = "";
            uint ret;

            unsafe
            {
                fixed (sbyte * p_byte = &msg[0])
                {
                    ret = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, 0, (uint)code, 0, p_byte, (uint)msg.Length, 0);
                    if (ret != 0)
                    {
                        msg_string = new string(p_byte);
                    }
                }
            }

            if (ret == 0)
            {
                return "GetSystemErrorCodeAsString() Failed, result=" +
                    ((ReturnCodes)Marshal.GetLastWin32Error()).ToString() + ", error code=" + code.ToString();
            }
            return msg_string.TrimEnd(new char[] { '\r', '\n' });
        }


        //---------------------------------------------------------------------
        // Get device information set for our driver
        //---------------------------------------------------------------------
        internal static HDEVINFO GetHardwareDeviceInfo(Guid guid)
        {
            return SetupDiGetClassDevs(ref guid, 0, 0, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
        }


        //---------------------------------------------------------------------
        // Delete a device information set and frees all associated memory
        //---------------------------------------------------------------------
        internal static void DestroyDeviceInfoList(HDEVINFO DeviceInfoSet)
        {
            if (DeviceInfoSet == HDEVINFO.INVALID_HANDLE_VALUE)
            {
                return;
            }
            SetupDiDestroyDeviceInfoList(DeviceInfoSet);
        }


        //---------------------------------------------------------------------
        // Enumerate the device interfaces that are contained in a device 
        // information set
        //---------------------------------------------------------------------
        internal static ReturnCodes EnumDeviceInterfaces(HDEVINFO devinfo, Guid guid, uint memberinfoindex, ref DeviceInterfaceData interfacedata)
        {
            if (!SetupDiEnumDeviceInterfaces(devinfo, 0, ref guid, memberinfoindex, ref interfacedata))
            {
                return (ReturnCodes)Marshal.GetLastWin32Error();
            }
            return ReturnCodes.ERROR_SUCCESS;
        }


        //---------------------------------------------------------------------
        // Get size of device interface path text string
        //---------------------------------------------------------------------
        internal static uint GetDeviceInterfaceDetailRequiredSize(HDEVINFO devinfo, DeviceInterfaceData interfacedata)
        {
            uint RequiredSize = 0;

            SetupDiGetDeviceInterfaceDetail(devinfo, ref interfacedata, 0, 0, ref RequiredSize, 0);

            return RequiredSize;
        }


        //---------------------------------------------------------------------
        // Get device interface path text string
        //---------------------------------------------------------------------
        internal static string GetDeviceNamePath(HDEVINFO devinfo, DeviceInterfaceData interfacedata)
        {
            DeviceInterfaceDetailData oDetail = new DeviceInterfaceDetailData();

            // 32/64-bit system size workaround
            if (IntPtr.Size == 8)
            {
                oDetail.Size = 8;
            }
            else
            {
                oDetail.Size = 5;
            }

            uint nRequiredSize = 0;

            // Error 0
            if (!SetupDiGetDeviceInterfaceDetail(devinfo, ref interfacedata, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero))
            {
                // Error 122 - ERROR_INSUFFICIENT_BUFFER (not a problem, just used to set nRequiredSize)
                if (SetupDiGetDeviceInterfaceDetail(devinfo, ref interfacedata, ref oDetail, nRequiredSize, ref nRequiredSize, IntPtr.Zero))
                {
                    return oDetail.DevicePath;
                }
                // Error 1784 - ERROR_INVALID_USER_BUFFER (unless size=5 on 32bit, size=8 on 64bit)
            }
            return "";
        }


        //---------------------------------------------------------------------
        // Syncronous DeviceIoControlSync() on a handle opened in overlapped mode
        //---------------------------------------------------------------------
        internal static unsafe ReturnCodes DeviceIoControlSync(HANDLE hDevice, uint dwIoControlCode, void* lpInBuffer, uint nInBufferSize,
                                                               void* lpOutBuffer, uint nOutBufferSize, uint* lpBytesReturned)
        {
            ReturnCodes ret_code;
            bool succ;
            OVERLAPPED ovlp;
            uint bytes_transferred = 0;

            ovlp.Internal = 0;
            ovlp.InternalHigh = 0;
            ovlp.Offset = 0;
            ovlp.OffsetHigh = 0;
            ovlp.hEvent = CreateEvent(null, false, false, null);

            // Start asyncronous ioctl
            succ = DeviceIoControl(hDevice, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize, lpBytesReturned, &ovlp);
            if (succ)
            {
                // ioctl completed successfully
                CloseHandle(ovlp.hEvent);
                return ReturnCodes.ERROR_SUCCESS;
            }

            // Get error code from DeviceIoControl()
            ret_code = (ReturnCodes)Marshal.GetLastWin32Error();

            // Other than ERROR_IO_PENDING is failure
            if (ret_code != ReturnCodes.ERROR_IO_PENDING)
            {
                CancelIo(hDevice);
                CloseHandle(ovlp.hEvent);
                return ret_code;
            }

            // the operation is pending, wait for completion
            ret_code = ReturnCodes.ERROR_SUCCESS;
            if (!(succ = GetOverlappedResult(hDevice, ref ovlp, ref bytes_transferred, true)))
            {
                // Failed, get error code from GetOverlappedResult()
                ret_code = (ReturnCodes)Marshal.GetLastWin32Error();
                CancelIo(hDevice);
            }

            CloseHandle(ovlp.hEvent);

            return ret_code;
        }


        [DllImportAttribute("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern unsafe uint CreateFile(sbyte * lpFileName, uint dwDesiredAccess, uint dwShareMode,
            int lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);

        [DllImportAttribute("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern unsafe bool CloseHandle(uint handle);

        [DllImportAttribute("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern unsafe bool ReadFile(HANDLE hFile, void * lpBuffer, uint nNumberOfBytesToRead,
            ref uint lpNumberOfBytesRead, ref OVERLAPPED lpOverlapped);

        [DllImportAttribute("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern unsafe bool WriteFile(HANDLE hFile, void * lpBuffer, uint nNumberOfBytesToWrite,
            ref uint lpNumberOfBytesWritten, ref OVERLAPPED lpOverlapped);

        [DllImportAttribute("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern unsafe HANDLE CreateEvent(void* lpEventAttributes, bool bManualReset, bool bInitialState, byte* lpName);

        [DllImportAttribute("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern unsafe uint WaitForSingleObject(HANDLE hHandle, uint dwMilliseconds);

        [DllImportAttribute("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern unsafe bool CancelIo(HANDLE hFile);

        [DllImportAttribute("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern unsafe bool GetOverlappedResult(HANDLE hFile, ref OVERLAPPED lpOverlapped, ref uint lpNumberOfBytesTransferred, bool bWait);

        [DllImportAttribute("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern unsafe uint FormatMessage(uint dwFlags, int lpSource, uint dwMessageId,
            uint dwLanguageId, sbyte * lpBuffer, uint nSize, int Arguments);

        [DllImportAttribute("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern unsafe bool DeviceIoControl(HANDLE hDevice, uint dwIoControlCode, void* lpInBuffer, uint nInBufferSize,
                                                         void * lpOutBuffer, uint nOutBufferSize, uint * lpBytesReturned, void * lpOverlapped);

        [DllImportAttribute("setupapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern unsafe HDEVINFO SetupDiGetClassDevs(ref Guid ClassGuid, uint Enumerator, uint hwndParent, uint Flags);

        [DllImportAttribute("setupapi.dll", SetLastError = true)]
        private static extern unsafe bool SetupDiDestroyDeviceInfoList(HDEVINFO DeviceInfoSet);

        [DllImportAttribute("setupapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern unsafe bool SetupDiEnumDeviceInterfaces(HDEVINFO DeviceInfoSet, int DeviceInfoData, ref Guid InterfaceClassGuid,
                                                                      uint MemberIndex, ref DeviceInterfaceData InterfaceData);

        [DllImportAttribute("setupapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern unsafe bool SetupDiGetDeviceInterfaceDetail(HDEVINFO DeviceInfoSet, ref DeviceInterfaceData DeviceInterfaceData,
            ref DeviceInterfaceDetailData detail, uint DeviceInterfaceDetailDataSize, ref uint RequiredSize, IntPtr DeviceInfoData);

        [DllImportAttribute("setupapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern unsafe bool SetupDiGetDeviceInterfaceDetail(HDEVINFO DeviceInfoSet, ref DeviceInterfaceData DeviceInterfaceData,
            IntPtr detail, uint DeviceInterfaceDetailDataSize, ref uint RequiredSize, IntPtr DeviceInfoData);

        [DllImportAttribute("setupapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern unsafe bool SetupDiGetDeviceInterfaceDetail(HDEVINFO DeviceInfoSet, ref DeviceInterfaceData DeviceInterfaceData,
            int Data, uint DeviceInterfaceDetailDataSize, ref uint RequiredSize, int DeviceInfoData);


		private const uint DIGCF_PRESENT					= 0x00000002;
		private const uint DIGCF_DEVICEINTERFACE			= 0x00000010;
		private const uint ENUMERATE_GUID					= DIGCF_PRESENT | DIGCF_DEVICEINTERFACE;
		private const uint FORMAT_MESSAGE_FROM_SYSTEM		= 0x00001000;
		private const uint GENERIC_READ						= 0x80000000;
		private const uint GENERIC_WRITE					= 0x40000000;
		private const uint FILE_SHARE_READ					= 0x00000001;
		private const uint FILE_SHARE_WRITE					= 0x00000002;
		private const uint OPEN_EXISTING					= 3;
		private const uint FILE_ATTRIBUTE_NORMAL			= 0x00000080;
		private const uint FILE_FLAG_OVERLAPPED				= 0x40000000;

        internal const uint WAIT_TIMEOUT = 0x00000102;
        internal const uint WAIT_FAILED  = 0xFFFFFFFF;


        // Handle to various native Windows objects
        internal enum HANDLE : uint
        {
            INVALID_HANDLE_VALUE = 0xFFFFFFFF,
            // Other values, handle is valid
        }

        // Handle to device info set
        internal enum HDEVINFO : uint
        {
            INVALID_HANDLE_VALUE = 0xFFFFFFFF,
            // Other values, handle is valid
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Guid
        {
            public uint Data1;
            public ushort Data2;
            public ushort Data3;
            public byte Byte1, Byte2, Byte3, Byte4, Byte5, Byte6, Byte7, Byte8;

            public Guid(uint d1, ushort d2, ushort d3, byte b1, byte b2, byte b3, byte b4, byte b5, byte b6, byte b7, byte b8)
            {
                this.Data1 = d1; this.Data2 = d2; this.Data3 = d3;
                this.Byte1 = b1; this.Byte2 = b2; this.Byte3 = b3; this.Byte4 = b4;
                this.Byte5 = b5; this.Byte6 = b6; this.Byte7 = b7; this.Byte8 = b8;
            }

            public static bool operator ==(Guid a, Guid b)
            {
                return (a.Data1 == b.Data1) && (a.Data2 == b.Data2) && (a.Data3 == b.Data3) &&
                       (a.Byte1 == b.Byte1) && (a.Byte2 == b.Byte2) && (a.Byte3 == b.Byte3) && (a.Byte4 == b.Byte4) &&
                       (a.Byte5 == b.Byte5) && (a.Byte6 == b.Byte6) && (a.Byte7 == b.Byte7) && (a.Byte8 == b.Byte8);
            }

            public static bool operator !=(Guid a, Guid b)
            {
                return !(a == b);
            }

            // Override the Object.Equals(object o) method:
            public override bool Equals(object o)
            {
                try
                {
                    return (bool)(this == (Guid)o);
                }
                catch
                {
                    return false;
                }
            }

            // Override the Object.GetHashCode() method:
            public override int GetHashCode()
            {
                return (int)(Data1 * Data2 * Data3);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DeviceInterfaceData
        {
            public uint Size;
            public Guid InterfaceClassGuid;
            public uint Flags;
            public UIntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DeviceInterfaceDetailData
        {
            public int Size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct OVERLAPPED
        {
            public uint Internal;       // ULONG_PTR
            public uint InternalHigh;   // ULONG_PTR
            public uint Offset;
            public uint OffsetHigh;
            public HANDLE hEvent;
        }

    }

}

