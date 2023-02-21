//---------- Copyright (C) 2009 by Oy Beamex Ab, Pietarsaari Finland ----------
//
// FILE:     MC6UsbDevice.cs
//
// AUTHOR:   MGB 14.10.2009
//
// ABSTRACT: Interface class towards the MC6 USB device. The class derives from
//           the UsbIo base and implements only Read()/Write() which are specific 
//           for MC6. The other functions in IUsbDevice are inherited from UsbIo.
//
//-----------------------------------------------------------------------------

using System;


namespace AnalogDevice.MC6
{

    public class MC6UsbDevice : UsbIo, IUsbDevice
    {

        //---------------------------------------------------------------------
        // Implement MC6 specific Read() for interface IUsbDevice 
        //---------------------------------------------------------------------
        public Win.ReturnCodes Read(out int msg_id, out int msg_status, out byte[] payload, int timeout_ms)
        {

#if TRACE
            try
            {

#endif
                const int HEADER_SIZE = 8;
                const int MAX_MESSAGE_SIZE = 256 * 1024 * 1024;

                Win.ReturnCodes ret_code;
                byte[] buf;
                int bytes_to_read;
                int bytes_read;
                int total_message_length;
                int payload_pos;
                int buf_pos;
                int total_bytes_read;

                // Set default return values
                msg_id = 0;
                msg_status = 0;
                payload = new byte [0];

                // Create temp read buffer
                buf = new byte[GetReadPipeTransferSize()];

                do
                {
                    // Read USB packet which should contain message header { uint length; ushort id, status; }
                    bytes_to_read = GetReadPipePacketSize();
                    if ((ret_code = ReadRawData(buf, 0, bytes_to_read, out bytes_read, timeout_ms)) != Win.ReturnCodes.ERROR_SUCCESS)
                    {
                        #if TRACE
                        Log.WriteLine("MC6UsbDevice.Read() ReadRawData() failed", Log.Level.ERROR);
                        #endif
                        return ret_code;
                    }

                    // Loop because we will (depending on the previous message length) read empty USB packets here
                } 
                while (bytes_read == 0);

                // Check that header fits in the data we got
                if (bytes_read < HEADER_SIZE)
                {
                    #if TRACE
                    Log.WriteLine("MC6UsbDevice.Read() received invalid message header", Log.Level.ERROR);
                    #endif
                    return Win.ReturnCodes.ERROR_INVALID_DATA;
                }

                // Get header parameters
                total_message_length = buf[0] + (buf[1] << 8) + (buf[2] << 16) + (buf[3] << 24);
                msg_id = buf[4] | (buf[5] << 8);
                msg_status = buf[6] | (buf[7] << 8);

                // Validate header parameters
                if ((total_message_length < HEADER_SIZE) || (total_message_length > MAX_MESSAGE_SIZE))
                {
                    #if TRACE
                    Log.WriteLine("MC6UsbDevice.Read() received invalid message header", Log.Level.ERROR);
                    #endif
                    return Win.ReturnCodes.ERROR_INVALID_DATA;
                }

                // Check that MC6 has terminated a short message transfer properly
                if (bytes_read > total_message_length)
                {
                    #if TRACE
                    Log.WriteLine("MC6UsbDevice.Read() short message not terminated correctly", Log.Level.ERROR);
                    #endif
                    return Win.ReturnCodes.ERROR_INVALID_DATA;
                }

                // Allocate output payload buffer
                payload = new byte[total_message_length - HEADER_SIZE];

                // Continue by reading the payload (if present).
                // Note that some of the payload bytes are already in buf[]
                payload_pos = 0;
                buf_pos = HEADER_SIZE;
                total_bytes_read = 0;

                for (;;) 
                {
                    // Copy data from source buf[] to destination payload[]
                    for (; buf_pos < bytes_read; buf_pos++, payload_pos++)
                    {
                        payload[payload_pos] = buf[buf_pos];
                    }

                    // Have we received the whole message?
                    total_bytes_read += bytes_read;
                    if (total_bytes_read >= total_message_length)
                    {
                        // Yes, we are done!
                        break;
                    }

                    // Read more payload (in multiples of USB packet size)
                    bytes_to_read = (((total_message_length - total_bytes_read) + (GetReadPipePacketSize() - 1)) / GetReadPipePacketSize()) * GetReadPipePacketSize();
                    // and no more than buf[] length at a time
                    if (bytes_to_read > buf.Length)
                    {
                        bytes_to_read = buf.Length;
                    }
                    if ((ret_code = ReadRawData(buf, 0, bytes_to_read, out bytes_read, timeout_ms)) != Win.ReturnCodes.ERROR_SUCCESS)
                    {
                        #if TRACE
                        Log.WriteLine("MC6UsbDevice.Read() ReadRawData() failed", Log.Level.ERROR);
                        #endif
                        return ret_code;
                    }
                    buf_pos = 0;

                    // Check that MC6 has terminated the message transfer properly
                    if (bytes_read > (total_message_length - total_bytes_read))
                    {
                        #if TRACE
                        Log.WriteLine("MC6UsbDevice.Read() message not terminated correctly", Log.Level.ERROR);
                        #endif
                        return Win.ReturnCodes.ERROR_INVALID_DATA;
                    }
                }

                #if TRACE
                Log.WriteLine("MC6UsbDevice.Read: " +
                    " id=" + msg_id.ToString() +
                    " status=" + msg_status.ToString() +
                    " payload_bytes=" + ((payload == null) ? 0 : payload.Length).ToString(), Log.Level.DEBUG);
                #endif

                // Message was read!
                return Win.ReturnCodes.ERROR_SUCCESS;

#if TRACE
            }
            catch (Exception e)
            {
                Log.WriteLine("MC6UsbDevice.Read() exception: " + e.ToString(), Log.Level.ERROR);
                throw;
            }
#endif

        }


        //---------------------------------------------------------------------
        // Implement MC6 specific Write() for interface IUsbDevice 
        //---------------------------------------------------------------------
        public Win.ReturnCodes Write(int msg_id, byte[] payload, int timeout_ms)
        {

#if TRACE
            try
            {
#endif
                const int HEADER_SIZE = 6;

                Win.ReturnCodes ret_code;
                int payload_length;
                byte[] message;

                #if TRACE
                Log.WriteLine("MC6UsbDevice.Write:" +
                    " id=" + msg_id.ToString() +
                    " payload_bytes=" + ((payload == null) ? 0 : payload.Length).ToString(), Log.Level.DEBUG);
                #endif

                // Allocate message buffer for header and payload
                payload_length = (payload == null) ? 0 : payload.Length;
                message = new byte[HEADER_SIZE + payload_length];

                // Build message header { uint length; ushort id; }
                message[0] = (byte) message.Length;
                message[1] = (byte)(message.Length >> 8);
                message[2] = (byte)(message.Length >> 16);
                message[3] = (byte)(message.Length >> 24);
                message[4] = (byte)msg_id;
                message[5] = (byte)(msg_id >> 8);

                // Copy payload to message buffer
                if (payload_length != 0)
                {
                    payload.CopyTo(message, HEADER_SIZE);
                }

                // Write message to MC6 using the USBIO driver
                ret_code = WriteRawData(message, 0, message.Length, timeout_ms);
                if (ret_code != Win.ReturnCodes.ERROR_SUCCESS)
                {
                    #if TRACE
                    Log.WriteLine("MC6UsbDevice.Write() WriteRawData() failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                    #endif
                    return ret_code;
                }

                // If message length is a multiple of the USB FIFO size, we must signal the 
                // end-of-transfer to MC6 by sending an empty packet also
                if ((message.Length % GetWritePipePacketSize()) == 0)
                {
                    ret_code = WriteRawData(message, 0, 0, timeout_ms);
                    if (ret_code != Win.ReturnCodes.ERROR_SUCCESS)
                    {
                        #if TRACE
                        Log.WriteLine("MC6UsbDevice.Write() WriteRawData(empty packet) failed: " + Win.GetSystemErrorCodeAsString(ret_code), Log.Level.ERROR);
                        #endif
                        return ret_code;
                    }
                }

                // Message was written!
                return Win.ReturnCodes.ERROR_SUCCESS;

#if TRACE
            }
            catch (Exception e)
            {
                Log.WriteLine("MC6UsbDevice.Write() exception: " + e.ToString(), Log.Level.ERROR);
                throw;
            }
#endif

        }

    }

}

