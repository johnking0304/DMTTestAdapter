//---------- Copyright (C) 2009 by Oy Beamex Ab, Pietarsaari Finland ----------
//
// FILE:     UsbIo.cs
//
// AUTHOR:   MGB 9.10.2009
//
// ABSTRACT: UsbIo is an interface class towards the USBIO generic USB driver 
//           from Thesycon. The class contains USBIO specific constants and 
//           structures, as well as open/close and low level read/write towards 
//           the USB pipes.
//
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using System.Diagnostics;


namespace BeamexDotNetUsbLowLib
{

    public class UsbIo : IDisposable
    {
        // Static mutex for exclusive access to m_OpenDevices
        private static Mutex m_OpenDevicesMutex = new Mutex();

        // Static table with USB devices that currently is open
        private static Hashtable m_OpenDevices = new Hashtable();


        // Device interface path passed to Open()
        private string m_DeviceInterfacePath = "";

        // Main handle to USBIO driver
        private Win.HANDLE m_hDriver = Win.HANDLE.INVALID_HANDLE_VALUE;

        // Handle to pipe carrying data from USB device
        private Win.HANDLE m_hReadPipe = Win.HANDLE.INVALID_HANDLE_VALUE;

        // Handle to pipe carrying data to USB device
        private Win.HANDLE m_hWritePipe = Win.HANDLE.INVALID_HANDLE_VALUE;

        // USB read pipe packet size (a.k.a. FIFO size), updated when pipe is opened
        private int m_MaximumReadPacketSize = 64;

        // USB write pipe packet size (a.k.a. FIFO size), updated when pipe is opened
        private int m_MaximumWritePacketSize = 64;

        // USB driver buffer size for reads, updated when pipe is opened
        private int m_MaximumReadTransferSize = 4096;

        // USB driver buffer size for writes, updated when pipe is opened
        private int m_MaximumWriteTransferSize = 4096;

        // Event for overlapped reading from read pipe
        private Win.HANDLE m_hReadEvent = Win.HANDLE.INVALID_HANDLE_VALUE;

        // Event for overlapped writing to write pipe
        private Win.HANDLE m_hWriteEvent = Win.HANDLE.INVALID_HANDLE_VALUE;

        // Mutex for ReadRawData() and Open()/Close() access
        private Mutex m_ReadDataMutex = new Mutex();

        // Mutex for WriteRawData() and Open()/Close() access
        private Mutex m_WriteDataMutex = new Mutex();


        //---------------------------------------------------------------------
        // Open USBIO device whith the given device interface path.
        //---------------------------------------------------------------------
        public Win.ReturnCodes Open(string device_interface_path)
        {
            Win.ReturnCodes ret_code;
            string pipe_device_interface_path;

            // Validate input parameter
            if (device_interface_path == null)
            {
                #if TRACE
                Log.WriteLine("UsbIo.Open() null argument", Log.Level.ERROR);
                #endif
                return Win.ReturnCodes.ERROR_INVALID_PARAMETER;
            }

            // Close first, if already open
            if (IsOpen())
            {
                #if TRACE
                Log.WriteLine("UsbIo.Open() instance already open, closing first", Log.Level.ERROR);
                #endif
                Close();
            }

            #if TRACE
            Log.WriteLine("UsbIo.Open(" + device_interface_path + ")", Log.Level.INFO);
            #endif

            // Get read data mutexes to prevent reading while opening and configuring
            // Also, at the same time we prevent close/open if some other thread
            // currently is reading or writing...
            if (!m_ReadDataMutex.WaitOne(0, false))
            {
                #if TRACE
                Log.WriteLine("UsbIo.Open() failed. read in progress, device is busy", Log.Level.ERROR);
                #endif
                return Win.ReturnCodes.ERROR_BUSY;
            }
            if (!m_WriteDataMutex.WaitOne(0, false))
            {
                #if TRACE
                Log.WriteLine("UsbIo.Open() failed. write in progress, device is busy", Log.Level.ERROR);
                #endif
                return Win.ReturnCodes.ERROR_BUSY;
            }

            // Check if the same USB device is already open in some other UsbIo object instance
            m_OpenDevicesMutex.WaitOne();
            if (m_OpenDevices.ContainsKey(device_interface_path))
            {
                #if TRACE
                Log.WriteLine("UsbIo.Open() error, this USB device is already open", Log.Level.ERROR);
                #endif
                m_OpenDevicesMutex.ReleaseMutex();
                m_WriteDataMutex.ReleaseMutex();
                m_ReadDataMutex.ReleaseMutex();
                return Win.ReturnCodes.ERROR_SERVICE_ALREADY_RUNNING;
            }
            // Not open, add it to the table
            m_OpenDevices.Add(device_interface_path, 0);
            m_DeviceInterfacePath = device_interface_path;
            m_OpenDevicesMutex.ReleaseMutex();

            bool configure_first_attempt = true;
    try_again:
            // Open main handle to USBIO driver
            m_hDriver = Win.CreateFile(device_interface_path, false);
            if (m_hDriver == Win.HANDLE.INVALID_HANDLE_VALUE)
            {
                ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                #if TRACE
                Log.WriteLine("UsbIo.Open() initial driver handle open failed", Log.Level.ERROR);
                #endif
                CleanupClose();
                m_WriteDataMutex.ReleaseMutex();
                m_ReadDataMutex.ReleaseMutex();
                return ret_code;
            }

            // Configure USB device so we can reset it
            if ((ret_code = ConfigureUsbDevice()) != Win.ReturnCodes.ERROR_SUCCESS)
            {
                #if TRACE
                Log.WriteLine("UsbIo.Open() initial configuration failed", Log.Level.ERROR);
                #endif
                CleanupClose();
                // Note! This is a workaround for MC2/MC4. MC2/MC4 gets configured already when the USB
                // cable is plugged in --> our initial attempt to configure it will fail here.
                // CleanupClose() will unconfigure it (if no other application owns it), and the next
                // attempt to configure it will succeed.
                if (configure_first_attempt && (ret_code == (Win.ReturnCodes)USBIO_ERR_ALREADY_CONFIGURED))
                {
                    #if TRACE
                    Log.WriteLine("UsbIo.Open() closed handle and trying again", Log.Level.ERROR);
                    #endif
                    configure_first_attempt = false;
                    goto try_again;
                }
                m_WriteDataMutex.ReleaseMutex();
                m_ReadDataMutex.ReleaseMutex();
                return ret_code;
            }

            // Reset USB device to get a really clean start
            if ((ret_code = ResetUsbDevice()) != Win.ReturnCodes.ERROR_SUCCESS)
            {
                #if TRACE
                Log.WriteLine("UsbIo.Open() USB reset failed", Log.Level.ERROR);
                #endif
                CleanupClose();
                m_WriteDataMutex.ReleaseMutex();
                m_ReadDataMutex.ReleaseMutex();
                return ret_code;
            }

            // Not sure why this delay is needed, but it makes the startup more reliable.
            // Maybe some tasks are finnished in the background and closing the handle too soon
            // after a reset (which generates USB messages) is not good. 
            Thread.Sleep(50);

            // Close USB driver handle
            Win.CloseHandle(m_hDriver);
            m_hDriver = Win.HANDLE.INVALID_HANDLE_VALUE;

            // As above, not sure why this delay is needed, but it makes the startup more 
            // reliable...
            Thread.Sleep(50);

            // Open main handle to USBIO driver
            m_hDriver = Win.CreateFile(device_interface_path, false);
            if (m_hDriver == Win.HANDLE.INVALID_HANDLE_VALUE)
            {
                ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                #if TRACE
                Log.WriteLine("UsbIo.Open() reopen after USB reset failed", Log.Level.ERROR);
                #endif
                CleanupClose();
                m_WriteDataMutex.ReleaseMutex();
                m_ReadDataMutex.ReleaseMutex();
                return ret_code;
            }

            // Configure USB device
            if ((ret_code = ConfigureUsbDevice()) != Win.ReturnCodes.ERROR_SUCCESS)
            {
                #if TRACE
                Log.WriteLine("UsbIo.Open() configuration failed", Log.Level.ERROR);
                #endif
                CleanupClose();
                m_WriteDataMutex.ReleaseMutex();
                m_ReadDataMutex.ReleaseMutex();
                return ret_code;
            }

            // Open read pipe
            pipe_device_interface_path = device_interface_path + "\\1";
            m_hReadPipe = Win.CreateFile(pipe_device_interface_path, true);
            if (m_hReadPipe == Win.HANDLE.INVALID_HANDLE_VALUE)
            {
                ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                #if TRACE
                Log.WriteLine("UsbIo.Open(" + pipe_device_interface_path + ") failed", Log.Level.ERROR);
                #endif
                CleanupClose();
                m_WriteDataMutex.ReleaseMutex();
                m_ReadDataMutex.ReleaseMutex();
                return ret_code;
            }

            // Open write pipe
            pipe_device_interface_path = device_interface_path + "\\0";
            m_hWritePipe = Win.CreateFile(pipe_device_interface_path, true);
            if (m_hWritePipe == Win.HANDLE.INVALID_HANDLE_VALUE)
            {
                ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                #if TRACE
                Log.WriteLine("UsbIo.Open(" + pipe_device_interface_path + ") failed", Log.Level.ERROR);
                #endif
                CleanupClose();
                m_WriteDataMutex.ReleaseMutex();
                m_ReadDataMutex.ReleaseMutex();
                return ret_code;
            }

            // Configre USB device read and write pipes
            if ((ret_code = ConfigureUsbDevicePipes()) != Win.ReturnCodes.ERROR_SUCCESS)
            {
                #if TRACE
                Log.WriteLine("UsbIo.Open() failed", Log.Level.ERROR);
                #endif
                CleanupClose();
                m_WriteDataMutex.ReleaseMutex();
                m_ReadDataMutex.ReleaseMutex();
                return ret_code;
            }
            
            // Create read and write event objects
            unsafe
            {
                m_hReadEvent = Win.CreateEvent(null, false, false, null);
                if (m_hReadEvent == Win.HANDLE.INVALID_HANDLE_VALUE)
                {
                    ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                    #if TRACE
                    Log.WriteLine("UsbIo.Open() CreateEvent() failed", Log.Level.ERROR);
                    #endif
                    CleanupClose();
                    m_WriteDataMutex.ReleaseMutex();
                    m_ReadDataMutex.ReleaseMutex();
                    return ret_code;
                }

                m_hWriteEvent = Win.CreateEvent(null, false, false, null);
                if (m_hWriteEvent == Win.HANDLE.INVALID_HANDLE_VALUE)
                {
                    ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                    #if TRACE
                    Log.WriteLine("UsbIo.Open() CreateEvent() failed", Log.Level.ERROR);
                    #endif
                    CleanupClose();
                    m_WriteDataMutex.ReleaseMutex();
                    m_ReadDataMutex.ReleaseMutex();
                    return ret_code;
                }
            }

            // Don't allow data transfers right after configuration as this gives problems
            // at least for MC4.
            Thread.Sleep(50);

            #if TRACE
            Log.WriteLine("UsbIo.Open() sucess (read_packet_size=" + m_MaximumReadPacketSize + ", write_packet_size=" + m_MaximumWritePacketSize + 
                          ", read_transfer_size=" + m_MaximumReadTransferSize + ", write_transfer_size=" + m_MaximumWriteTransferSize + ")", Log.Level.INFO);
            #endif

            // Success!
            m_WriteDataMutex.ReleaseMutex();
            m_ReadDataMutex.ReleaseMutex();
            return Win.ReturnCodes.ERROR_SUCCESS;
        }


        //---------------------------------------------------------------------
        // Configure the USB device
        //---------------------------------------------------------------------
        private Win.ReturnCodes ConfigureUsbDevice()
        {
            Win.ReturnCodes ret_code;
            USBIO_SET_CONFIGURATION set_cfg;
            uint bytes_returned;

            // Configure the device
            set_cfg.ConfigurationIndex = 0;                         // use the configuration descriptor with index 0
            set_cfg.NbOfInterfaces = 1;                             // device has 1 interface
            set_cfg.InterfaceList0.InterfaceIndex = 0;              // first interface is 0
            set_cfg.InterfaceList0.AlternateSettingIndex = 0;       // alternate setting for first interface is 0
            set_cfg.InterfaceList0.MaximumTransferSize = 64 * 1024; // maximum buffer size for read/write operation in bytes
            unsafe
            {
                if (!Win.DeviceIoControl(m_hDriver,
                            IOCTL_USBIO_SET_CONFIGURATION,
                            &set_cfg, (uint)Marshal.SizeOf(set_cfg),
                            null, 0,
                            &bytes_returned,
                            null))
                {
                    ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                    #if TRACE
                    Log.WriteLine("UsbIo.ConfigureUsbDevice() IOCTL_USBIO_SET_CONFIGURATION failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                    #endif
                    return ret_code;
                }
            }

            // Success!
            return Win.ReturnCodes.ERROR_SUCCESS;
        }


        //---------------------------------------------------------------------
        // Reset USB device
        //---------------------------------------------------------------------
        private Win.ReturnCodes ResetUsbDevice()
        {
            Win.ReturnCodes ret_code;
            uint bytes_returned;

            unsafe
            {
                if (!Win.DeviceIoControl(m_hDriver,
                            IOCTL_USBIO_RESET_DEVICE,
                            null, 0,
                            null, 0,
                            &bytes_returned,
                            null))
                {
                    ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                    #if TRACE
                    Log.WriteLine("UsbIo.ResetDevice() IOCTL_USBIO_RESET_DEVICE failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                    #endif
                    return ret_code;
                }
            }

            // Success!
            return Win.ReturnCodes.ERROR_SUCCESS;
        }


        //---------------------------------------------------------------------
        // Configure the USB device pipes
        //---------------------------------------------------------------------
        private Win.ReturnCodes ConfigureUsbDevicePipes()
        {
            Win.ReturnCodes ret_code;
            USBIO_BIND_PIPE bind_pipe;
            uint bytes_returned;

            // Set endpoint for read pipe
            bind_pipe.EndpointAddress = 0x82;
            unsafe
            {
                if ((ret_code = Win.DeviceIoControlSync(m_hReadPipe,
                                        IOCTL_USBIO_BIND_PIPE,
                                        &bind_pipe, (uint)Marshal.SizeOf(bind_pipe),
                                        null, 0,
                                        &bytes_returned)) != Win.ReturnCodes.ERROR_SUCCESS)
                {
                    #if TRACE
                    Log.WriteLine("UsbIo.ConfigureUsbDevicePipes() IOCTL_USBIO_BIND_PIPE 0x82 failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                    #endif
                    return ret_code;
                }
            }

            // Get pipe packet and transfer size
            ret_code = GetDeviceMaximumTransferAndPacketSize(m_hReadPipe, bind_pipe.EndpointAddress, out m_MaximumReadPacketSize, out m_MaximumReadTransferSize);
            if (ret_code != Win.ReturnCodes.ERROR_SUCCESS)
            {
                #if TRACE
                Log.WriteLine("UsbIo.ConfigureUsbDevicePipes() failed", Log.Level.ERROR);
                #endif
                return ret_code;
            }

            // Set endpoint for write pipe
            bind_pipe.EndpointAddress = 0x01;
            unsafe
            {
                if ((ret_code = Win.DeviceIoControlSync(m_hWritePipe,
                                        IOCTL_USBIO_BIND_PIPE,
                                        &bind_pipe, (uint)Marshal.SizeOf(bind_pipe),
                                        null, 0,
                                        &bytes_returned)) != Win.ReturnCodes.ERROR_SUCCESS)
                {
                    #if TRACE
                    Log.WriteLine("UsbIo.ConfigureUsbDevicePipes() IOCTL_USBIO_BIND_PIPE 0x01 failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                    #endif
                    return ret_code;
                }
            }

            // Get pipe packet and transfer size
            ret_code = GetDeviceMaximumTransferAndPacketSize(m_hWritePipe, bind_pipe.EndpointAddress, out m_MaximumWritePacketSize, out m_MaximumWriteTransferSize);
            if (ret_code != Win.ReturnCodes.ERROR_SUCCESS)
            {
                #if TRACE
                Log.WriteLine("UsbIo.ConfigureUsbDevicePipes() failed", Log.Level.ERROR);
                #endif
                return ret_code;
            }

            // Device pipes successfully configured
            return Win.ReturnCodes.ERROR_SUCCESS;
        }


        //---------------------------------------------------------------------
        // Ask the driver what the maximum packet and transfer size is for
        // the given pipe
        //---------------------------------------------------------------------
        private Win.ReturnCodes GetDeviceMaximumTransferAndPacketSize(Win.HANDLE hPipe, byte ep, out int maximum_packet_size, out int maximum_transfer_size)
        {
            Win.ReturnCodes ret_code;
            USBIO_CONFIGURATION_INFO cfg;
            uint bytes_returned;

            // Set defaults 
            maximum_packet_size = 64;
            maximum_transfer_size = 4096;

            // Ask the pipe what the transfer and packet sizes are
            unsafe
            {
                if ((ret_code = Win.DeviceIoControlSync(hPipe,
                                        IOCTL_USBIO_GET_CONFIGURATION_INFO,
                                        null, 0,
                                        &cfg, (uint)Marshal.SizeOf(cfg),
                                        &bytes_returned)) != Win.ReturnCodes.ERROR_SUCCESS)
                {
                    #if TRACE
                    Log.WriteLine("UsbIo.GetDeviceMaximumTransferAndPacketSize() IOCTL_USBIO_GET_CONFIGURATION_INFO failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                    #endif
                    return ret_code;
                }
            }
            if ((cfg.NbOfPipes >= 1) && (cfg.PipeInfo0.EndpointAddress == ep))
            {
                maximum_packet_size = cfg.PipeInfo0.MaximumPacketSize;
                maximum_transfer_size = (int)cfg.PipeInfo0.MaximumTransferSize;
            }
            else if ((cfg.NbOfPipes >= 2) && (cfg.PipeInfo1.EndpointAddress == ep))
            {
                maximum_packet_size = cfg.PipeInfo1.MaximumPacketSize;
                maximum_transfer_size = (int)cfg.PipeInfo1.MaximumTransferSize;
            }
            else if ((cfg.NbOfPipes >= 3) && (cfg.PipeInfo2.EndpointAddress == ep))
            {
                maximum_packet_size = cfg.PipeInfo2.MaximumPacketSize;
                maximum_transfer_size = (int)cfg.PipeInfo2.MaximumTransferSize;
            }

            // Windows 7 64-bit returns rather large transfer buffers; 4 MByte. --> limit to max 64 kByte
            if (maximum_transfer_size > (64 * 1024))
            {
                #if TRACE
                Log.WriteLine("UsbIo.GetDeviceMaximumTransferAndPacketSize() large transfer size " + maximum_transfer_size/1024 + " kByte. Limiting to 64 kByte", Log.Level.ERROR);
                #endif
                maximum_transfer_size = 64 * 1024;
            }

            // Success!
            return Win.ReturnCodes.ERROR_SUCCESS;
        }


        //---------------------------------------------------------------------
        // Ask the driver for the read (Rx) counters
        //---------------------------------------------------------------------
        public Win.ReturnCodes GetReadCounters(out ulong bytes_transferred, out uint requests_succeeded, out uint requests_failed, bool reset_counters)
        {
            Win.ReturnCodes ret_code;

            // Grab read data mutex
            m_ReadDataMutex.WaitOne();

            // Get statistics from read pipe
            ret_code = GetPipeStatistics(m_hReadPipe, out bytes_transferred, out requests_succeeded, out requests_failed, reset_counters);

            // Release read data mutex
            m_ReadDataMutex.ReleaseMutex();
            return ret_code;
        }


        //---------------------------------------------------------------------
        // Ask the driver for the write (Tx) counters
        //---------------------------------------------------------------------
        public Win.ReturnCodes GetWriteCounters(out ulong bytes_transferred, out uint requests_succeeded, out uint requests_failed, bool reset_counters)
        {
            Win.ReturnCodes ret_code;

            // Grab write data mutex
            m_WriteDataMutex.WaitOne();

            // Get statistics from write pipe
            ret_code = GetPipeStatistics(m_hWritePipe, out bytes_transferred, out requests_succeeded, out requests_failed, reset_counters);

            // release write data mutex
            m_WriteDataMutex.ReleaseMutex();
            return ret_code;
        }


        //---------------------------------------------------------------------
        // Ask the driver for the pipe statistics
        //---------------------------------------------------------------------
        private Win.ReturnCodes GetPipeStatistics(Win.HANDLE hPipe, out ulong bytes_transferred, out uint requests_succeeded, out uint requests_failed, bool reset_counters)
        {
            Win.ReturnCodes ret_code;
            USBIO_QUERY_PIPE_STATISTICS query;
            USBIO_PIPE_STATISTICS result;
            uint bytes_returned;

            // Set defaults 
            bytes_transferred = 0;
            requests_succeeded = 0;
            requests_failed = 0;

            // Reset statistics counters?
            if (reset_counters)
            {
                query.Flags = USBIO_QPS_FLAG_RESET_ALL_COUNTERS;
            }
            else
            {
                query.Flags = 0;
            }

            // Ask the pipe for the statistics
            unsafe
            {
                if ((ret_code = Win.DeviceIoControlSync(hPipe,
                                        IOCTL_USBIO_QUERY_PIPE_STATISTICS,
                                        &query, (uint)Marshal.SizeOf(query),
                                        &result, (uint)Marshal.SizeOf(result),
                                        &bytes_returned)) != Win.ReturnCodes.ERROR_SUCCESS)
                {
                    #if TRACE
                    Log.WriteLine("UsbIo.GetPipeStatistics() IOCTL_USBIO_QUERY_PIPE_STATISTICS failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                    #endif
                    return ret_code;
                }
            }
            bytes_transferred = result.BytesTransferred_L | (result.BytesTransferred_H << 32);
            requests_succeeded = result.RequestsSucceeded;
            requests_failed = result.RequestsFailed;

            // Success!
            return Win.ReturnCodes.ERROR_SUCCESS;
        }


        //---------------------------------------------------------------------
        // Check if USBIO driver device is open
        //---------------------------------------------------------------------
        public bool IsOpen()
        {
            return m_hDriver != Win.HANDLE.INVALID_HANDLE_VALUE;
        }


        //---------------------------------------------------------------------
        // Get name of USB device, same as passed to Open()
        //---------------------------------------------------------------------
        public string GetName()
        {
            return m_DeviceInterfacePath;
        }


        //---------------------------------------------------------------------
        // Close USBIO driver device
        //
        // Note! Before calling close, make sure that some other thread is not 
        //       currently waiting inside Read() or Write(). Close() will fail 
        //       with ERROR_BUSY if this is the case!
        //---------------------------------------------------------------------
        public Win.ReturnCodes Close()
        {
            // Get read data mutexes to prevent reading while closing
            // Also, at the same time we prevent closing if some other thread
            // currently is reading or writing...
            if (!m_ReadDataMutex.WaitOne(0, false))
            {
                #if TRACE
                Log.WriteLine("UsbIo.Close() failed. read in progress, device is busy", Log.Level.ERROR);
                #endif
                return Win.ReturnCodes.ERROR_BUSY;
            }
            if (!m_WriteDataMutex.WaitOne(0, false))
            {
                #if TRACE
                Log.WriteLine("UsbIo.Close() failed. write in progress, device is busy", Log.Level.ERROR);
                #endif
                return Win.ReturnCodes.ERROR_BUSY;
            }

            #if TRACE
            Log.WriteLine("UsbIo.Close(" + m_DeviceInterfacePath + ")", Log.Level.INFO);
            #endif

            CleanupClose();

            m_WriteDataMutex.ReleaseMutex();
            m_ReadDataMutex.ReleaseMutex();
            return Win.ReturnCodes.ERROR_SUCCESS;
        }


        //---------------------------------------------------------------------
        // Close any Windows native object handles and release other native
        // resources.
        //---------------------------------------------------------------------
        private void CleanupClose()
        {
            // Close read event handle, if open
            if (m_hWriteEvent != Win.HANDLE.INVALID_HANDLE_VALUE)
            {
                Win.CloseHandle(m_hWriteEvent);
                m_hWriteEvent = Win.HANDLE.INVALID_HANDLE_VALUE;
            }

            // Close read event handle, if open
            if (m_hReadEvent != Win.HANDLE.INVALID_HANDLE_VALUE)
            {
                Win.CloseHandle(m_hReadEvent);
                m_hReadEvent = Win.HANDLE.INVALID_HANDLE_VALUE;
            }

            // Close write pipe handle, if open
            if (m_hWritePipe != Win.HANDLE.INVALID_HANDLE_VALUE)
            {
                Win.CloseHandle(m_hWritePipe);
                m_hWritePipe = Win.HANDLE.INVALID_HANDLE_VALUE;
            }

            // Close read pipe handle, if open
            if (m_hReadPipe != Win.HANDLE.INVALID_HANDLE_VALUE)
            {
                Win.CloseHandle(m_hReadPipe);
                m_hReadPipe = Win.HANDLE.INVALID_HANDLE_VALUE;
            }

            // Close main driver handle, if open
            if (m_hDriver != Win.HANDLE.INVALID_HANDLE_VALUE)
            {
                Win.CloseHandle(m_hDriver);
                m_hDriver = Win.HANDLE.INVALID_HANDLE_VALUE;
            }

            // Remove USB device from table of open devices
            m_OpenDevicesMutex.WaitOne();
            m_OpenDevices.Remove(m_DeviceInterfacePath);
            m_OpenDevicesMutex.ReleaseMutex();
        }


        //---------------------------------------------------------------------
        // Get the USB read pipe packet size a.k.a. FIFO size. The function 
        // returns meaningful information when the device is open.
        //---------------------------------------------------------------------
        public int GetReadPipePacketSize()
        {
            return m_MaximumReadPacketSize;
        }


        //---------------------------------------------------------------------
        // Get the USB write pipe packet size a.k.a. FIFO size. The function 
        // returns meaningful information when the device is open.
        //---------------------------------------------------------------------
        public int GetWritePipePacketSize()
        {
            return m_MaximumWritePacketSize;
        }


        //---------------------------------------------------------------------
        // Get the USB driver buffer size for read. The function 
        // returns meaningful information when the device is open.
        //---------------------------------------------------------------------
        public int GetReadPipeTransferSize()
        {
            return m_MaximumReadTransferSize;
        }


        //---------------------------------------------------------------------
        // Get the USB driver buffer size for writes. The function 
        // returns meaningful information when the device is open.
        //---------------------------------------------------------------------
        public int GetWritePipeTransferSize()
        {
            return m_MaximumWriteTransferSize;
        }


        //---------------------------------------------------------------------
        // Read raw data from the connected USB device. The function does not
        // return until all data has been read, or a short or empty packet is 
        // is received, or a timeout or error occure.
        // The destination buf[] should be a multiple of the USB FIFO packet
        // size!
        // In case of timeout, or a short or empty packet was received, the 
        // returned 'bytes_read' might be less than 'bytes'.
        // If the amount of data is large, it is read as several smaller blocks 
        // not to cause broblems with the USB driver read buffer.
        //---------------------------------------------------------------------
        public Win.ReturnCodes ReadRawData(byte[] buf, int buf_offset, int bytes, out int bytes_read, int timeout_ms)
        {
            Win.ReturnCodes ret_code;
            Win.OVERLAPPED ovlp;
            bool read_result;
            uint wait_result;
            int bytes_to_read;
            uint bytes_transferred;

            // Default output value, no data read
            bytes_read = 0;

            // Sanity check input parameters
            if ((buf == null) || (buf_offset < 0) || (bytes < 0) ||
                ((buf_offset + bytes) > buf.Length) ||
                (timeout_ms < 0))
            {
                #if TRACE
                Log.WriteLine("UsbIo.ReadRawData() bad input parameter", Log.Level.ERROR);
                #endif
                return Win.ReturnCodes.ERROR_INVALID_PARAMETER;
            }

            // Grab read data mutex
            m_ReadDataMutex.WaitOne();

            try
            {
                // We must be connected to read data
                if (!IsOpen())
                {
                    #if TRACE
                    Log.WriteLine("UsbIo.ReadRawData() device not connected", Log.Level.ERROR);
                    #endif
                    m_ReadDataMutex.ReleaseMutex();
                    return Win.ReturnCodes.ERROR_DEVICE_NOT_CONNECTED;
                }

                // Loop and wait until all data is read, timeout or error occure
                for (; ; )
                {
                    // Setup for an overlapped read
                    ovlp.Internal = 0;
                    ovlp.InternalHigh = 0;
                    ovlp.Offset = 0;
                    ovlp.OffsetHigh = 0;
                    ovlp.hEvent = m_hReadEvent;

                    // Limit current read to m_MaximumTransferSize
                    bytes_to_read = bytes;
                    if (bytes_to_read > m_MaximumReadTransferSize)
                    {
                        bytes_to_read = m_MaximumReadTransferSize;
                    }

                    unsafe
                    {
                        // Note! Pin the destination data buffer as the read is asynchronous.
                        // The overlapped struct is on the stack so it's also fixed during the asynchronous read.
                        fixed (byte* p_buf = &buf[buf_offset])
                        {
                            // Start the asynchronous read
                            bytes_transferred = 0;
                            read_result = Win.ReadFile(m_hReadPipe, (void*)p_buf, (uint)bytes_to_read, ref bytes_transferred, ref ovlp);
                            ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();

                            // ReadFile() problem or asyncronous completion
                            if (!read_result)
                            {
                                if (ret_code != Win.ReturnCodes.ERROR_IO_PENDING)
                                {
                                    #if TRACE
                                    Log.WriteLine("UsbIo.ReadRawData() ReadFile() failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                                    #endif
                                    Win.CancelIo(m_hReadPipe);
                                    m_ReadDataMutex.ReleaseMutex();
                                    return ret_code;
                                }

                                // Wait for asyncronous read to complete
                                if ((wait_result = Win.WaitForSingleObject(m_hReadEvent, (uint)timeout_ms)) == Win.WAIT_FAILED)
                                {
                                    // Wait failed
                                    ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                                    #if TRACE
                                    Log.WriteLine("UsbIo.ReadRawData() WaitForSingleObject() failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                                    #endif
                                    Win.CancelIo(m_hReadPipe);
                                    m_ReadDataMutex.ReleaseMutex();
                                    return ret_code;
                                }

                                // Timeout waiting for data?
                                if (wait_result == Win.WAIT_TIMEOUT)
                                {
                                    #if TRACE
                                    Log.WriteLine("UsbIo.ReadRawData() WaitForSingleObject() timeout (timeout_ms=" + timeout_ms + ")", Log.Level.ERROR);
                                    #endif
                                    Win.CancelIo(m_hReadPipe);
                                    m_ReadDataMutex.ReleaseMutex();
                                    return Win.ReturnCodes.WAIT_TIMEOUT;
                                }

                                // Get number of bytes read
                                bytes_transferred = 0;
                                if (!Win.GetOverlappedResult(m_hReadPipe, ref ovlp, ref bytes_transferred, false))
                                {
                                    // GetOverlappedResult failed
                                    ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                                    #if TRACE
                                    Log.WriteLine("UsbIo.ReadRawData() GetOverlappedResult() failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                                    #endif
                                    Win.CancelIo(m_hReadPipe);
                                    m_ReadDataMutex.ReleaseMutex();
                                    return ret_code;
                                }
                            }
                        }
                    }

                    // Check that bytes_transferred makes sense
                    if (bytes_transferred > bytes_to_read)
                    {
                        #if TRACE
                        Log.WriteLine("UsbIo.ReadRawData() inconsistent read", Log.Level.ERROR);
                        #endif
                        m_ReadDataMutex.ReleaseMutex();
                        return Win.ReturnCodes.ERROR_GEN_FAILURE;
                    }

                    // Advance in destination buffer
                    buf_offset += (int)bytes_transferred;
                    bytes -= (int)bytes_transferred;
                    bytes_read += (int)bytes_transferred;

                    // All bytes read, or short or empty packet received?
                    if ((bytes <= 0) || (bytes_transferred == 0) || ((bytes_transferred % GetReadPipePacketSize()) != 0))
                    {
                        // Yes! We are done
                        break;
                    }

                }

                // Data read successfully without error or timeout
                m_ReadDataMutex.ReleaseMutex();
                return Win.ReturnCodes.ERROR_SUCCESS;
            }
            catch (Exception)
            {
                // In case of exception, make sure mutex is released
                m_ReadDataMutex.ReleaseMutex();
                throw;
            }
         }


        //---------------------------------------------------------------------
        // Write raw data to the connected USB device. The function does not
        // return until all data has been written, or a timeout or error occure.
        // If the amount if data is large, it is written as several smaller blocks 
        // not to overflow the USB driver write buffer.
        //---------------------------------------------------------------------
        public Win.ReturnCodes WriteRawData(byte[] buf, int buf_offset, int bytes, int timeout_ms)
        {
            Win.ReturnCodes ret_code;
            Win.OVERLAPPED ovlp;
            bool write_result;
            uint wait_result;
            int bytes_to_write;
            uint bytes_transferred;

            // Sanity check input parameters
            if ((buf == null) || (buf_offset < 0) || (bytes < 0) ||
                ((buf_offset + bytes) > buf.Length) ||
                (timeout_ms < 0))
            {
                #if TRACE
                Log.WriteLine("UsbIo.WriteRawData() bad input parameter", Log.Level.ERROR);
                #endif
                return Win.ReturnCodes.ERROR_INVALID_PARAMETER;
            }

            // Grab write data mutex
            m_WriteDataMutex.WaitOne();

            try
            {

                // We must be connected to write data
                if (!IsOpen())
                {
                    #if TRACE
                    Log.WriteLine("UsbIo.WriteRawData() device not connected", Log.Level.ERROR);
                    #endif
                    m_WriteDataMutex.ReleaseMutex();
                    return Win.ReturnCodes.ERROR_DEVICE_NOT_CONNECTED;
                }

                // Loop and wait until all data is written, timeout or error occure
                for (;;)
                {
                    // Setup for an overlapped write
                    ovlp.Internal     = 0;
                    ovlp.InternalHigh = 0;
                    ovlp.Offset       = 0;
                    ovlp.OffsetHigh   = 0;
                    ovlp.hEvent       = m_hWriteEvent;

                    // Limit current write to m_MaximumTransferSize
                    bytes_to_write = bytes;
                    if (bytes_to_write > m_MaximumWriteTransferSize)
                    {
                        bytes_to_write = m_MaximumWriteTransferSize;
                    }

                    unsafe
                    {
                        // Note! Pin the source data buffer as the write is asynchronous.
                        // The overlapped struct is on the stack so it's also fixed during the asynchronous write.
                        fixed (byte * p_buf = &buf[buf_offset])
                        {
                            // Start the asynchronous write
                            bytes_transferred = 0;
                            write_result = Win.WriteFile(m_hWritePipe, (void*)p_buf, (uint)bytes_to_write, ref bytes_transferred, ref ovlp);
                            ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();

                            // WriteFile() problems or asynchronous completion
                            if (!write_result)
                            {
                                if (ret_code != Win.ReturnCodes.ERROR_IO_PENDING)
                                {
                                    // Write failed
                                    #if TRACE
                                    Log.WriteLine("UsbIo.WriteRawData() WriteFile() failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                                    #endif
                                    Win.CancelIo(m_hWritePipe);
                                    m_WriteDataMutex.ReleaseMutex();
                                    return ret_code;
                                }

                                // Wait for asyncronous write to complete
                                if ((wait_result = Win.WaitForSingleObject(m_hWriteEvent, (uint)timeout_ms)) == Win.WAIT_FAILED)
                                {
                                    // Wait failed
                                    ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                                    #if TRACE
                                    Log.WriteLine("UsbIo.WriteRawData() WaitForSingleObject() failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                                    #endif
                                    Win.CancelIo(m_hWritePipe);
                                    m_WriteDataMutex.ReleaseMutex();
                                    return ret_code;
                                }

                                // Timeout waiting for data to be transmitted?
                                if (wait_result == Win.WAIT_TIMEOUT)
                                {
                                    #if TRACE
                                    Log.WriteLine("UsbIo.WriteRawData() WaitForSingleObject() timeout (timeout_ms=" + timeout_ms + ")", Log.Level.ERROR);
                                    #endif
                                    Win.CancelIo(m_hWritePipe);
                                    m_WriteDataMutex.ReleaseMutex();
                                    return Win.ReturnCodes.WAIT_TIMEOUT;
                                }

                                // Get number of bytes written
                                bytes_transferred = 0;
                                if (!Win.GetOverlappedResult(m_hWritePipe, ref ovlp, ref bytes_transferred, false))
                                {
                                    // GetOverlappedResult failed
                                    ret_code = (Win.ReturnCodes)Marshal.GetLastWin32Error();
                                    #if TRACE
                                    Log.WriteLine("UsbIo.WriteRawData() GetOverlappedResult() failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                                    #endif
                                    Win.CancelIo(m_hWritePipe);
                                    m_WriteDataMutex.ReleaseMutex();
                                    return ret_code;
                                }
                            }
                        }
                    }

                    // Check that bytes_transferred makes sense
                    if (bytes_transferred > bytes_to_write)
                    {
                        #if TRACE
                        Log.WriteLine("UsbIo.WriteRawData() inconsistent write", Log.Level.ERROR);
                        #endif
                        m_WriteDataMutex.ReleaseMutex();
                        return Win.ReturnCodes.ERROR_GEN_FAILURE;
                    }

                    // Advance in source buffer
                    buf_offset += (int)bytes_transferred;
                    bytes -= (int)bytes_transferred;

                    // All bytes written?
                    if (bytes <= 0)
                    {
                        // Yes! We are done
                        break;
                    }

                }

                // Data written successfully without error or timeout
                m_WriteDataMutex.ReleaseMutex();
                return Win.ReturnCodes.ERROR_SUCCESS;

            }
            catch (Exception)
            {
                // In case of exception, make sure mutex is released
                m_WriteDataMutex.ReleaseMutex();
                throw;
            }
        }


        //---------------------------------------------------------------------
        // Implement IDisposable
        //---------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).
                // ***
            }
            // Free unmanaged objects.
            if (IsOpen())
            {
                Close();
            }
            // Set large fields to null.
        }


        //---------------------------------------------------------------------
        // Destructor
        //---------------------------------------------------------------------
        ~UsbIo()
		{
			Dispose(false);
		}



        //---------------------------------------------------------------------
        //
        // The following code is USBIO driver specific structures and constants
        //
        //---------------------------------------------------------------------


        // USBIO_PIPE_CONFIGURATION_INFO structure from usbio_i.h
        [StructLayout(LayoutKind.Sequential)]
        internal struct USBIO_PIPE_CONFIGURATION_INFO
        {
            public uint PipeType;  // type
            public uint MaximumTransferSize;// maximum Read/Write buffer size 
            public ushort MaximumPacketSize;  // FIFO size of the endpoint  
            public byte EndpointAddress;    // including direction bit (bit 7)
            public byte Interval;           // in ms, for interrupt pipe
            public byte InterfaceNumber;    // interface that the EP belongs to
            public byte reserved1;   // reserved field, set to zero
            public byte reserved2;   // reserved field, set to zero
            public byte reserved3;   // reserved field, set to zero
        }

        // USBIO_INTERFACE_CONFIGURATION_INFO structure from usbio_i.h
        [StructLayout(LayoutKind.Sequential)]
        internal struct USBIO_INTERFACE_CONFIGURATION_INFO
        {
            public byte InterfaceNumber;
            public byte AlternateSetting;
            public byte Class;
            public byte SubClass;
            public byte Protocol;
            public byte NumberOfPipes;
            public byte reserved1;  // reserved field, set to zero
            public byte reserved2;  // reserved field, set to zero
        }

        // USBIO_CONFIGURATION_INFO structure from usbio_i.h
        [StructLayout(LayoutKind.Sequential)]
        internal struct USBIO_CONFIGURATION_INFO
        {
            public uint NbOfInterfaces;
            public uint NbOfPipes;
            public USBIO_INTERFACE_CONFIGURATION_INFO /* InterfaceInfo[32]; */
                InterfaceInfo0,
                InterfaceInfo1,
                InterfaceInfo2,
                InterfaceInfo3,
                InterfaceInfo4,
                InterfaceInfo5,
                InterfaceInfo6,
                InterfaceInfo7,
                InterfaceInfo8,
                InterfaceInfo9,
                InterfaceInfo10,
                InterfaceInfo11,
                InterfaceInfo12,
                InterfaceInfo13,
                InterfaceInfo14,
                InterfaceInfo15,
                InterfaceInfo16,
                InterfaceInfo17,
                InterfaceInfo18,
                InterfaceInfo19,
                InterfaceInfo20,
                InterfaceInfo21,
                InterfaceInfo22,
                InterfaceInfo23,
                InterfaceInfo24,
                InterfaceInfo25,
                InterfaceInfo26,
                InterfaceInfo27,
                InterfaceInfo28,
                InterfaceInfo29,
                InterfaceInfo30,
                InterfaceInfo31;
            public USBIO_PIPE_CONFIGURATION_INFO /* PipeInfo[32]; */
                PipeInfo0,
                PipeInfo1,
                PipeInfo2,
                PipeInfo3,
                PipeInfo4,
                PipeInfo5,
                PipeInfo6,
                PipeInfo7,
                PipeInfo8,
                PipeInfo9,
                PipeInfo10,
                PipeInfo11,
                PipeInfo12,
                PipeInfo13,
                PipeInfo14,
                PipeInfo15,
                PipeInfo16,
                PipeInfo17,
                PipeInfo18,
                PipeInfo19,
                PipeInfo20,
                PipeInfo21,
                PipeInfo22,
                PipeInfo23,
                PipeInfo24,
                PipeInfo25,
                PipeInfo26,
                PipeInfo27,
                PipeInfo28,
                PipeInfo29,
                PipeInfo30,
                PipeInfo31;
        }

        // USBIO_INTERFACE_SETTING structure from usbio_i.h
        [StructLayout(LayoutKind.Sequential)]
        internal struct USBIO_INTERFACE_SETTING
        {
            public ushort InterfaceIndex;
            public ushort AlternateSettingIndex;
            public uint MaximumTransferSize;
        }

        // USBIO_SET_CONFIGURATION structure from usbio_i.h
        [StructLayout(LayoutKind.Sequential)]
        internal struct USBIO_SET_CONFIGURATION
        {
            public ushort ConfigurationIndex;
            public ushort NbOfInterfaces;
            public USBIO_INTERFACE_SETTING /* InterfaceList[32]; */
                InterfaceList0,
                InterfaceList1,
                InterfaceList2,
                InterfaceList3,
                InterfaceList4,
                InterfaceList5,
                InterfaceList6,
                InterfaceList7,
                InterfaceList8,
                InterfaceList9,
                InterfaceList10,
                InterfaceList11,
                InterfaceList12,
                InterfaceList13,
                InterfaceList14,
                InterfaceList15,
                InterfaceList16,
                InterfaceList17,
                InterfaceList18,
                InterfaceList19,
                InterfaceList20,
                InterfaceList21,
                InterfaceList22,
                InterfaceList23,
                InterfaceList24,
                InterfaceList25,
                InterfaceList26,
                InterfaceList27,
                InterfaceList28,
                InterfaceList29,
                InterfaceList30,
                InterfaceList31;
        }

        // USBIO_BIND_PIPE structure from usbio_i.h
        [StructLayout(LayoutKind.Sequential)]
        internal struct USBIO_BIND_PIPE
        {
            public byte EndpointAddress; // including direction bit
        }

        // USBIO_QUERY_PIPE_STATISTICS structure from usbio_i.h
        [StructLayout(LayoutKind.Sequential)]
        internal struct USBIO_QUERY_PIPE_STATISTICS
        {
            public uint Flags; // 0 or any combination of USBIO_QPS_FLAG_XXX
        }
        internal const uint USBIO_QPS_FLAG_RESET_BYTES_TRANSFERRED   = 0x00000001;
        internal const uint USBIO_QPS_FLAG_RESET_REQUESTS_SUCCEEDED  = 0x00000002;
        internal const uint USBIO_QPS_FLAG_RESET_REQUESTS_FAILED     = 0x00000004;
        internal const uint USBIO_QPS_FLAG_RESET_ALL_COUNTERS        = USBIO_QPS_FLAG_RESET_BYTES_TRANSFERRED | 
                                                                       USBIO_QPS_FLAG_RESET_REQUESTS_SUCCEEDED |
                                                                       USBIO_QPS_FLAG_RESET_REQUESTS_FAILED;

        // USBIO_PIPE_STATISTICS structure from usbio_i.h
        [StructLayout(LayoutKind.Sequential)]
        internal struct USBIO_PIPE_STATISTICS 
        {
          public uint ActualAveragingInterval;  // in ms, 0 = statistics off
          public uint AverageRate;              // in bytes/s
          public uint BytesTransferred_L; // total number of bytes transferred, lower 32 bits
          public uint BytesTransferred_H; // total number of bytes transferred, upper 32 bits
          public uint RequestsSucceeded;  // total number of I/O requests succeeded
          public uint RequestsFailed;     // total number of I/O requests failed
          public uint reserved1;          // reserved for future use
          public uint reserved2;          // reserved for future use
        }

        // Constants from WinIoCtl.h
        internal const uint METHOD_BUFFERED = 0;
        internal const uint FILE_ANY_ACCESS = 0;

        // Constants from usbio_i.h
        internal const uint FILE_DEVICE_USBIO = 0x8094;
        internal const uint _USBIO_IOCTL_BASE = 0x800;

        // Constants from usbio_i.h & WinIoCtl.h
        internal const uint IOCTL_USBIO_SET_CONFIGURATION      = (FILE_DEVICE_USBIO << 16) | (FILE_ANY_ACCESS << 14) | ((_USBIO_IOCTL_BASE +  9) << 2) | METHOD_BUFFERED;
        internal const uint IOCTL_USBIO_GET_CONFIGURATION_INFO = (FILE_DEVICE_USBIO << 16) | (FILE_ANY_ACCESS << 14) | ((_USBIO_IOCTL_BASE + 20) << 2) | METHOD_BUFFERED;
        internal const uint IOCTL_USBIO_RESET_DEVICE           = (FILE_DEVICE_USBIO << 16) | (FILE_ANY_ACCESS << 14) | ((_USBIO_IOCTL_BASE + 21) << 2) | METHOD_BUFFERED;
        internal const uint IOCTL_USBIO_BIND_PIPE              = (FILE_DEVICE_USBIO << 16) | (FILE_ANY_ACCESS << 14) | ((_USBIO_IOCTL_BASE + 30) << 2) | METHOD_BUFFERED;
        internal const uint IOCTL_USBIO_UNBIND_PIPE            = (FILE_DEVICE_USBIO << 16) | (FILE_ANY_ACCESS << 14) | ((_USBIO_IOCTL_BASE + 31) << 2) | METHOD_BUFFERED;
        internal const uint IOCTL_USBIO_RESET_PIPE             = (FILE_DEVICE_USBIO << 16) | (FILE_ANY_ACCESS << 14) | ((_USBIO_IOCTL_BASE + 32) << 2) | METHOD_BUFFERED;
        internal const uint IOCTL_USBIO_ABORT_PIPE             = (FILE_DEVICE_USBIO << 16) | (FILE_ANY_ACCESS << 14) | ((_USBIO_IOCTL_BASE + 33) << 2) | METHOD_BUFFERED;
        internal const uint IOCTL_USBIO_SETUP_PIPE_STATISTICS  = (FILE_DEVICE_USBIO << 16) | (FILE_ANY_ACCESS << 14) | ((_USBIO_IOCTL_BASE + 37) << 2) | METHOD_BUFFERED;
        internal const uint IOCTL_USBIO_QUERY_PIPE_STATISTICS  = (FILE_DEVICE_USBIO << 16) | (FILE_ANY_ACCESS << 14) | ((_USBIO_IOCTL_BASE + 38) << 2) | METHOD_BUFFERED;


        // Constants from usbioerr.h
        internal const uint USBIO_ERR_SUCCESS                          = 0x00000000;
        internal const uint USBIO_ERR_CRC                              = 0xE0000001;
        internal const uint USBIO_ERR_BTSTUFF                          = 0xE0000002;
        internal const uint USBIO_ERR_DATA_TOGGLE_MISMATCH             = 0xE0000003;
        internal const uint USBIO_ERR_STALL_PID                        = 0xE0000004;
        internal const uint USBIO_ERR_DEV_NOT_RESPONDING               = 0xE0000005;
        internal const uint USBIO_ERR_PID_CHECK_FAILURE                = 0xE0000006;
        internal const uint USBIO_ERR_UNEXPECTED_PID                   = 0xE0000007;
        internal const uint USBIO_ERR_DATA_OVERRUN                     = 0xE0000008;
        internal const uint USBIO_ERR_DATA_UNDERRUN                    = 0xE0000009;
        internal const uint USBIO_ERR_RESERVED1                        = 0xE000000A;
        internal const uint USBIO_ERR_RESERVED2                        = 0xE000000B;
        internal const uint USBIO_ERR_BUFFER_OVERRUN                   = 0xE000000C;
        internal const uint USBIO_ERR_BUFFER_UNDERRUN                  = 0xE000000D;
        internal const uint USBIO_ERR_NOT_ACCESSED                     = 0xE000000F;
        internal const uint USBIO_ERR_FIFO                             = 0xE0000010;
        internal const uint USBIO_ERR_XACT_ERROR                       = 0xE0000011;
        internal const uint USBIO_ERR_BABBLE_DETECTED                  = 0xE0000012;
        internal const uint USBIO_ERR_DATA_BUFFER_ERROR                = 0xE0000013;
        internal const uint USBIO_ERR_ENDPOINT_HALTED                  = 0xE0000030;
        internal const uint USBIO_ERR_NO_MEMORY                        = 0xE0000100;
        internal const uint USBIO_ERR_INVALID_URB_FUNCTION             = 0xE0000200;
        internal const uint USBIO_ERR_INVALID_PARAMETER                = 0xE0000300;
        internal const uint USBIO_ERR_ERROR_BUSY                       = 0xE0000400;
        internal const uint USBIO_ERR_REQUEST_FAILED                   = 0xE0000500;
        internal const uint USBIO_ERR_INVALID_PIPE_HANDLE              = 0xE0000600;
        internal const uint USBIO_ERR_NO_BANDWIDTH                     = 0xE0000700;
        internal const uint USBIO_ERR_INTERNAL_HC_ERROR                = 0xE0000800;
        internal const uint USBIO_ERR_ERROR_SHORT_TRANSFER             = 0xE0000900;
        internal const uint USBIO_ERR_BAD_START_FRAME                  = 0xE0000A00;
        internal const uint USBIO_ERR_ISOCH_REQUEST_FAILED             = 0xE0000B00;
        internal const uint USBIO_ERR_FRAME_CONTROL_OWNED              = 0xE0000C00;
        internal const uint USBIO_ERR_FRAME_CONTROL_NOT_OWNED          = 0xE0000D00;
        internal const uint USBIO_ERR_NOT_SUPPORTED                    = 0xE0000E00;
        internal const uint USBIO_ERR_INVALID_CONFIGURATION_DESCRIPTOR = 0xE0000F00;
        internal const uint USBIO_ERR_INSUFFICIENT_RESOURCES           = 0xE8001000;
        internal const uint USBIO_ERR_SET_CONFIG_FAILED                = 0xE0002000;
        internal const uint USBIO_ERR_USBD_BUFFER_TOO_SMALL            = 0xE0003000;
        internal const uint USBIO_ERR_USBD_INTERFACE_NOT_FOUND         = 0xE0004000;
        internal const uint USBIO_ERR_INVALID_PIPE_FLAGS               = 0xE0005000;
        internal const uint USBIO_ERR_USBD_TIMEOUT                     = 0xE0006000;
        internal const uint USBIO_ERR_DEVICE_GONE                      = 0xE0007000;
        internal const uint USBIO_ERR_STATUS_NOT_MAPPED                = 0xE0008000;
        internal const uint USBIO_ERR_CANCELED                         = 0xE0010000;
        internal const uint USBIO_ERR_ISO_NOT_ACCESSED_BY_HW           = 0xE0020000;
        internal const uint USBIO_ERR_ISO_TD_ERROR                     = 0xE0030000;
        internal const uint USBIO_ERR_ISO_NA_LATE_USBPORT              = 0xE0040000;
        internal const uint USBIO_ERR_ISO_NOT_ACCESSED_LATE            = 0xE0050000;
        internal const uint USBIO_ERR_FAILED                           = 0xE0001000;
        internal const uint USBIO_ERR_INVALID_INBUFFER                 = 0xE0001001;
        internal const uint USBIO_ERR_INVALID_OUTBUFFER                = 0xE0001002;
        internal const uint USBIO_ERR_OUT_OF_MEMORY                    = 0xE0001003;
        internal const uint USBIO_ERR_PENDING_REQUESTS                 = 0xE0001004;
        internal const uint USBIO_ERR_ALREADY_CONFIGURED               = 0xE0001005;
        internal const uint USBIO_ERR_NOT_CONFIGURED                   = 0xE0001006;
        internal const uint USBIO_ERR_OPEN_PIPES                       = 0xE0001007;
        internal const uint USBIO_ERR_ALREADY_BOUND                    = 0xE0001008;
        internal const uint USBIO_ERR_NOT_BOUND                        = 0xE0001009;
        internal const uint USBIO_ERR_DEVICE_NOT_PRESENT               = 0xE000100A;
        internal const uint USBIO_ERR_CONTROL_NOT_SUPPORTED            = 0xE000100B;
        internal const uint USBIO_ERR_TIMEOUT                          = 0xE000100C;
        internal const uint USBIO_ERR_INVALID_RECIPIENT                = 0xE000100D;
        internal const uint USBIO_ERR_INVALID_TYPE                     = 0xE000100E;
        internal const uint USBIO_ERR_INVALID_IOCTL                    = 0xE000100F;
        internal const uint USBIO_ERR_INVALID_DIRECTION                = 0xE0001010;
        internal const uint USBIO_ERR_TOO_MUCH_ISO_PACKETS             = 0xE0001011;
        internal const uint USBIO_ERR_POOL_EMPTY                       = 0xE0001012;
        internal const uint USBIO_ERR_PIPE_NOT_FOUND                   = 0xE0001013;
        internal const uint USBIO_ERR_INVALID_ISO_PACKET               = 0xE0001014;
        internal const uint USBIO_ERR_OUT_OF_ADDRESS_SPACE             = 0xE0001015;
        internal const uint USBIO_ERR_INTERFACE_NOT_FOUND              = 0xE0001016;
        internal const uint USBIO_ERR_INVALID_DEVICE_STATE             = 0xE0001017;
        internal const uint USBIO_ERR_INVALID_PARAM                    = 0xE0001018;
        internal const uint USBIO_ERR_DEMO_EXPIRED                     = 0xE0001019;
        internal const uint USBIO_ERR_INVALID_POWER_STATE              = 0xE000101A;
        internal const uint USBIO_ERR_POWER_DOWN                       = 0xE000101B;
        internal const uint USBIO_ERR_VERSION_MISMATCH                 = 0xE000101C;
        internal const uint USBIO_ERR_SET_CONFIGURATION_FAILED         = 0xE000101D;
        internal const uint USBIO_ERR_ADDITIONAL_EVENT_SIGNALLED       = 0xE000101E;
        internal const uint USBIO_ERR_INVALID_PROCESS                  = 0xE000101F;
        internal const uint USBIO_ERR_DEVICE_ACQUIRED                  = 0xE0001020;
        internal const uint USBIO_ERR_DEVICE_OPENED                    = 0xE0001021;
        internal const uint USBIO_ERR_VID_RESTRICTION                  = 0xE0001080;
        internal const uint USBIO_ERR_ISO_RESTRICTION                  = 0xE0001081;
        internal const uint USBIO_ERR_BULK_RESTRICTION                 = 0xE0001082;
        internal const uint USBIO_ERR_EP0_RESTRICTION                  = 0xE0001083;
        internal const uint USBIO_ERR_PIPE_RESTRICTION                 = 0xE0001084;
        internal const uint USBIO_ERR_PIPE_SIZE_RESTRICTION            = 0xE0001085;
        internal const uint USBIO_ERR_CONTROL_RESTRICTION              = 0xE0001086;
        internal const uint USBIO_ERR_INTERRUPT_RESTRICTION            = 0xE0001087;
        internal const uint USBIO_ERR_DEVICE_NOT_FOUND                 = 0xE0001100;
        internal const uint USBIO_ERR_DEVICE_NOT_OPEN                  = 0xE0001102;
        internal const uint USBIO_ERR_NO_SUCH_DEVICE_INSTANCE          = 0xE0001104;
        internal const uint USBIO_ERR_INVALID_FUNCTION_PARAM           = 0xE0001105;
        internal const uint USBIO_ERR_LOAD_SETUP_API_FAILED            = 0xE0001106;
        internal const uint USBIO_ERR_DEVICE_ALREADY_OPENED            = 0xE0001107;

        // Struct for mapping a text string to an USBIO error code
        internal struct ErrorCodeText
        {
            public uint   code;
            public string text;

            public ErrorCodeText(uint _code, string _text)
            {
                code = _code;
                text = _text;
            }
        }

        // Table for mapping USBIO error codes to text messages
        internal static ErrorCodeText[] m_ErrorCodeTexts = new ErrorCodeText[] 
        { 
            new ErrorCodeText(USBIO_ERR_SUCCESS                , "No error."),
            new ErrorCodeText(USBIO_ERR_CRC                    , "HC Error: Wrong CRC."),
            new ErrorCodeText(USBIO_ERR_BTSTUFF                , "HC Error: Wrong bit stuffing."),
            new ErrorCodeText(USBIO_ERR_DATA_TOGGLE_MISMATCH   , "HC Error: Data toggle mismatch."),
            new ErrorCodeText(USBIO_ERR_STALL_PID              , "HC Error: stall PID."),
            new ErrorCodeText(USBIO_ERR_DEV_NOT_RESPONDING     , "HC Error: Device not responding."),
            new ErrorCodeText(USBIO_ERR_PID_CHECK_FAILURE      , "HC Error: PID check failed."),
            new ErrorCodeText(USBIO_ERR_UNEXPECTED_PID         , "HC Error: Unexpected PID."),
            new ErrorCodeText(USBIO_ERR_DATA_OVERRUN           , "HC Error: Data Overrun."),
            new ErrorCodeText(USBIO_ERR_DATA_UNDERRUN          , "HC Error: Data Underrun."),
            new ErrorCodeText(USBIO_ERR_RESERVED1              , "HC Error: Reserved1."),
            new ErrorCodeText(USBIO_ERR_RESERVED2              , "HC Error: Reserved2."),
            new ErrorCodeText(USBIO_ERR_BUFFER_OVERRUN         , "HC Error: Buffer Overrun."),
            new ErrorCodeText(USBIO_ERR_BUFFER_UNDERRUN        , "HC Error: Buffer Underrun."),
            new ErrorCodeText(USBIO_ERR_NOT_ACCESSED           , "HC Error: Not accessed."),
            new ErrorCodeText(USBIO_ERR_FIFO                   , "HC Error: FIFO error."),
            new ErrorCodeText(USBIO_ERR_XACT_ERROR             , "HC Error: XACT error."),
            new ErrorCodeText(USBIO_ERR_BABBLE_DETECTED        , "HC Error: Babble detected."),
            new ErrorCodeText(USBIO_ERR_DATA_BUFFER_ERROR      , "HC Error: Data buffer error."),
            new ErrorCodeText(USBIO_ERR_ENDPOINT_HALTED        , "USBD Error: Endpoint halted."),
            new ErrorCodeText(USBIO_ERR_NO_MEMORY              , "USBD Error: No system memory."),
            new ErrorCodeText(USBIO_ERR_INVALID_URB_FUNCTION   , "USBD Error: Invalid URB function."),
            new ErrorCodeText(USBIO_ERR_INVALID_PARAMETER      , "USBD Error: Invalid parameter."),
            new ErrorCodeText(USBIO_ERR_ERROR_BUSY             , "USBD Error: Busy."),
            new ErrorCodeText(USBIO_ERR_REQUEST_FAILED         , "USBD Error: Request failed."),
            new ErrorCodeText(USBIO_ERR_INVALID_PIPE_HANDLE    , "USBD Error: Invalid pipe handle."),
            new ErrorCodeText(USBIO_ERR_NO_BANDWIDTH           , "USBD Error: No bandwidth available."),
            new ErrorCodeText(USBIO_ERR_INTERNAL_HC_ERROR      , "USBD Error: Internal HC error."),
            new ErrorCodeText(USBIO_ERR_ERROR_SHORT_TRANSFER   , "USBD Error: Short transfer."),
            new ErrorCodeText(USBIO_ERR_BAD_START_FRAME        , "USBD Error: Bad start frame."),
            new ErrorCodeText(USBIO_ERR_ISOCH_REQUEST_FAILED   , "USBD Error: Isochronous request failed."),
            new ErrorCodeText(USBIO_ERR_FRAME_CONTROL_OWNED    , "USBD Error: Frame control owned."),
            new ErrorCodeText(USBIO_ERR_FRAME_CONTROL_NOT_OWNED, "USBD Error: Frame control not owned."),
            new ErrorCodeText(USBIO_ERR_NOT_SUPPORTED          , "USBD Error: Not supported."),
            new ErrorCodeText(USBIO_ERR_INVALID_CONFIGURATION_DESCRIPTOR, "USBD Error: Invalid configuration descriptor."),
            new ErrorCodeText(USBIO_ERR_INSUFFICIENT_RESOURCES   , "USBD Error: Insufficient resources."),
            new ErrorCodeText(USBIO_ERR_SET_CONFIG_FAILED        , "USBD Error: Set configuration failed."),
            new ErrorCodeText(USBIO_ERR_USBD_BUFFER_TOO_SMALL    , "USBD Error: Buffer too small."),
            new ErrorCodeText(USBIO_ERR_USBD_INTERFACE_NOT_FOUND , "USBD Error: Interface not found."),
            new ErrorCodeText(USBIO_ERR_INVALID_PIPE_FLAGS       , "USBD Error: Invalid pipe flags."),
            new ErrorCodeText(USBIO_ERR_USBD_TIMEOUT             , "USBD Error: Timeout."),
            new ErrorCodeText(USBIO_ERR_DEVICE_GONE              , "USBD Error: Device gone."),
            new ErrorCodeText(USBIO_ERR_STATUS_NOT_MAPPED        , "USBD Error: Status not mapped."),
            new ErrorCodeText(USBIO_ERR_CANCELED               , "USBD Error: cancelled."),
            new ErrorCodeText(USBIO_ERR_ISO_NOT_ACCESSED_BY_HW , "USBD Error: ISO not accessed by hardware."),
            new ErrorCodeText(USBIO_ERR_ISO_TD_ERROR           , "USBD Error: ISO TD error."),
            new ErrorCodeText(USBIO_ERR_ISO_NA_LATE_USBPORT    , "USBD Error: ISO NA late USB port."),
            new ErrorCodeText(USBIO_ERR_ISO_NOT_ACCESSED_LATE  , "USBD Error: ISO not accessed, submitted too late."),
            new ErrorCodeText(USBIO_ERR_FAILED                 , "Operation failed."),
            new ErrorCodeText(USBIO_ERR_INVALID_INBUFFER       , "Input buffer too small."),
            new ErrorCodeText(USBIO_ERR_INVALID_OUTBUFFER      , "Output buffer too small."),
            new ErrorCodeText(USBIO_ERR_OUT_OF_MEMORY          , "Out of memory."),
            new ErrorCodeText(USBIO_ERR_PENDING_REQUESTS       , "There are pending requests."),
            new ErrorCodeText(USBIO_ERR_ALREADY_CONFIGURED     , "USB device is already configured."),
            new ErrorCodeText(USBIO_ERR_NOT_CONFIGURED         , "USB device is not configured."),
            new ErrorCodeText(USBIO_ERR_OPEN_PIPES             , "There are open pipes."),
            new ErrorCodeText(USBIO_ERR_ALREADY_BOUND          , "Either handle or pipe is already bound."),
            new ErrorCodeText(USBIO_ERR_NOT_BOUND              , "Handle is not bound to a pipe."),
            new ErrorCodeText(USBIO_ERR_DEVICE_NOT_PRESENT     , "Device is removed."),
            new ErrorCodeText(USBIO_ERR_CONTROL_NOT_SUPPORTED  , "Control code is not supported."),
            new ErrorCodeText(USBIO_ERR_TIMEOUT                , "The request has been timed out."),
            new ErrorCodeText(USBIO_ERR_INVALID_RECIPIENT      , "Invalid recipient."),
            new ErrorCodeText(USBIO_ERR_INVALID_TYPE           , "Invalid pipe type or invalid request type."),
            new ErrorCodeText(USBIO_ERR_INVALID_IOCTL          , "Invalid I/O control code."),
            new ErrorCodeText(USBIO_ERR_INVALID_DIRECTION      , "Invalid direction of read/write operation."),
            new ErrorCodeText(USBIO_ERR_TOO_MUCH_ISO_PACKETS   , "Too much ISO packets."),
            new ErrorCodeText(USBIO_ERR_POOL_EMPTY             , "Request pool empty."),
            new ErrorCodeText(USBIO_ERR_PIPE_NOT_FOUND         , "Pipe not found."),
            new ErrorCodeText(USBIO_ERR_INVALID_ISO_PACKET     , "Invalid ISO packet."),
            new ErrorCodeText(USBIO_ERR_OUT_OF_ADDRESS_SPACE   , "Out of address space. Not enough system resources."),
            new ErrorCodeText(USBIO_ERR_INTERFACE_NOT_FOUND    , "Interface not found."),
            new ErrorCodeText(USBIO_ERR_INVALID_DEVICE_STATE   , "Invalid device state (stopped or power down)."),
            new ErrorCodeText(USBIO_ERR_INVALID_PARAM          , "Invalid parameter."),
            new ErrorCodeText(USBIO_ERR_DEMO_EXPIRED           , "DEMO version has timed out. Reboot required!"),
            new ErrorCodeText(USBIO_ERR_INVALID_POWER_STATE    , "Power state not allowed. Set to D0 first."),
            new ErrorCodeText(USBIO_ERR_POWER_DOWN             , "Device powered down."),
            new ErrorCodeText(USBIO_ERR_VERSION_MISMATCH       , "API Version does not match."),
            new ErrorCodeText(USBIO_ERR_SET_CONFIGURATION_FAILED,"Set configuration failed."),
            new ErrorCodeText(USBIO_ERR_INVALID_PROCESS,         "Invalid process."),
            new ErrorCodeText(USBIO_ERR_DEVICE_ACQUIRED,         "The device is acquired by another process for exclusive use."),
            new ErrorCodeText(USBIO_ERR_DEVICE_OPENED,           "A different process has opened the device."),
            new ErrorCodeText(USBIO_ERR_VID_RESTRICTION,         "Light version restriction: Unsupported Vendor ID."),
            new ErrorCodeText(USBIO_ERR_ISO_RESTRICTION,         "Light version restriction: Iso pipes are not supported."),
            new ErrorCodeText(USBIO_ERR_BULK_RESTRICTION,        "Light version restriction: Bulk pipes are not supported."),
            new ErrorCodeText(USBIO_ERR_EP0_RESTRICTION,         "Light version restriction: EP0 requests are not fully supported."),
            new ErrorCodeText(USBIO_ERR_PIPE_RESTRICTION,        "Light version restriction: Too many pipes."),
            new ErrorCodeText(USBIO_ERR_PIPE_SIZE_RESTRICTION,   "Light version restriction: Maximum FIFO size exceeded."),
            new ErrorCodeText(USBIO_ERR_CONTROL_RESTRICTION,     "Light version restriction: Control pipes are not supported."),
            new ErrorCodeText(USBIO_ERR_INTERRUPT_RESTRICTION,   "Light version restriction: Interrupt pipes are not supported."),
            new ErrorCodeText(USBIO_ERR_DEVICE_NOT_FOUND       , "Device not found or acquired for exclusive use."),
            new ErrorCodeText(USBIO_ERR_DEVICE_NOT_OPEN        , "Device not open."),
            new ErrorCodeText(USBIO_ERR_NO_SUCH_DEVICE_INSTANCE, "No such device instance."),
            new ErrorCodeText(USBIO_ERR_INVALID_FUNCTION_PARAM,  "An invalid parameter was passed."),
            new ErrorCodeText(USBIO_ERR_DEVICE_ALREADY_OPENED,   "The handle is already opened.")
        };


    }


}
