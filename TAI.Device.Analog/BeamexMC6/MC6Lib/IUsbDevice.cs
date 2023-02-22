//---------- Copyright (C) 2009 by Oy Beamex Ab, Pietarsaari Finland ----------
//
// FILE:     IUsbDevice.cs
//
// AUTHOR:   MGB 16.10.2009
//
// ABSTRACT: Abstract USB device interface for opening/closing and reading/writing 
//           messages.
//
//-----------------------------------------------------------------------------

using System;


namespace AnalogDevice.MC6
{

    public interface IUsbDevice : IDisposable
    {

        //---------------------------------------------------------------------
        // Open USB device with the given device interface path.
        // The device interface path can be found using the Enumerator class
        // functions.
        //---------------------------------------------------------------------
        Win.ReturnCodes Open(string device_interface_path);


        //---------------------------------------------------------------------
        // Check if USB device is currently open
        //---------------------------------------------------------------------
        bool IsOpen();


        //---------------------------------------------------------------------
        // Get name (same as passed to Open()) of USB device
        //---------------------------------------------------------------------
        string GetName();


        //---------------------------------------------------------------------
        // Read message from USB device
        //---------------------------------------------------------------------
        Win.ReturnCodes Read(out int msg_id, out int msg_status, out byte[] payload, int timeout_ms);


        //---------------------------------------------------------------------
        // Write message to USB device
        //---------------------------------------------------------------------
        Win.ReturnCodes Write(int msg_id, byte[] payload, int timeout_ms);


        //---------------------------------------------------------------------
        // Get the read counters (transfer statistics) from USB device
        //---------------------------------------------------------------------
        Win.ReturnCodes GetReadCounters(out ulong bytes_transferred, out uint requests_succeeded, out uint requests_failed, bool reset_counters);


        //---------------------------------------------------------------------
        // Get the write counters (transfer statistics) from USB device
        //---------------------------------------------------------------------
        Win.ReturnCodes GetWriteCounters(out ulong bytes_transferred, out uint requests_succeeded, out uint requests_failed, bool reset_counters);


        //---------------------------------------------------------------------
        // Close USB device
        //
        // Note! Before calling close, make sure that some other thread is not 
        //       currently waiting inside Read() or Write(). Close() will fail 
        //       with ERROR_BUSY if this is the case!
        //---------------------------------------------------------------------
        Win.ReturnCodes Close();

    }

}

