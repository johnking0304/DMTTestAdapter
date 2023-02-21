//---------- Copyright (C) 2009 by Oy Beamex Ab, Pietarsaari Finland ----------
//
// FILE:     MC4UsbDevice.cs
//
// AUTHOR:   MGB 14.10.2009, almost copy/pase from older MC2/MC4 code by MKA?
//
// ABSTRACT: Interface class towards the MC4 USB device. The class derives from
//           the UsbIo base and implements only Read()/Write() which is specific 
//           for MC4. The other functions in IUsbDevice are inherited from UsbIo.
//
//-----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace BeamexDotNetUsbLowLib
{

    public class MC4UsbDevice : UsbIo, IUsbDevice
    {

        //---------------------------------------------------------------------
        // Implement MC4 specific Read() for interface IUsbDevice 
        //---------------------------------------------------------------------
        public Win.ReturnCodes Read(out int msg_id, out int msg_status, out byte[] payload, int timeout_ms)
        {

#if TRACE
            try
            {
#endif

                Win.ReturnCodes return_value;
                MessageInHeader msg_header = new MessageInHeader();

                msg_id = 255;
                msg_status = 255;
                payload = new byte[0];

                byte[] recv = this.PacketRead(out return_value, timeout_ms);
                if (return_value != Win.ReturnCodes.ERROR_SUCCESS)
                {
                    #if TRACE
                    Log.WriteLine("MC4UsbDevice.Read() PacketRead() failed: " + Win.GetSystemErrorCodeAsString(return_value), Log.Level.ERROR);
                    #endif
                    return return_value;
                }

                msg_header.AssignThisNoPacket(ref recv);
                msg_id = msg_header.CommandId;
                msg_status = msg_header.Status;

                payload = new byte[recv.Length - MessageInHeader.SIZE];
                for (int i = 0; i < payload.Length; i++)
                    payload[i] = recv[i + MessageInHeader.SIZE];

                #if TRACE
                Log.WriteLine("MC4UsbDevice.Read: " +
                    " id=" + msg_id.ToString() +
                    " status=" + msg_status.ToString() +
                    " payload_bytes=" + ((payload == null) ? 0 : payload.Length).ToString(), Log.Level.DEBUG);
                #endif

                return Win.ReturnCodes.ERROR_SUCCESS;

#if TRACE
            }
            catch (Exception e)
            {
                Log.WriteLine("MC4UsbDevice.Read() exception: " + e.ToString(), Log.Level.ERROR);
                throw;
            }
#endif

        }


        //---------------------------------------------------------------------
        // Implement MC4 specific Write() for interface IUsbDevice 
        //---------------------------------------------------------------------
        public Win.ReturnCodes Write(int msg_id, byte[] payload, int timeout_ms)
        {

#if TRACE
            try
            {
#endif
                #if TRACE
                Log.WriteLine("MC4UsbDevice.Write:" +
                    " id=" + msg_id.ToString() +
                    " payload_bytes=" + ((payload == null) ? 0 : payload.Length).ToString(), Log.Level.DEBUG);
                #endif

                int msg_size = payload == null ? 0 : payload.Length;
                byte[] out_data = new byte[32];
                MessageOutHeader Out = new MessageOutHeader((byte)msg_id, msg_size);
                MultiSerialPacketHeader Header = new MultiSerialPacketHeader(msg_size <= 26 ? (byte)(msg_size + 5) : (byte)31);

                Header.AssignData(ref out_data);
                Out.AssignData(ref out_data);
                for (int i = 0; (i < msg_size) && (i < 26); i++)
                    out_data[6 + i] = payload[i];

                Win.ReturnCodes status = base.WriteRawData(out_data, 0, out_data.Length, timeout_ms);
                if (status != Win.ReturnCodes.ERROR_SUCCESS)
                {
                    #if TRACE
                    Log.WriteLine("MC4UsbDevice.Write() WriteRawData() failed: " + Win.GetSystemErrorCodeAsString(status), Log.Level.ERROR);
                    #endif
                    return status;
                }

                for (int index = 26, increment; index < msg_size; index += increment)
                {
                    increment = (msg_size - index) >= 31 ? 31 : (msg_size - index);

                    Header.ValidBytes = (byte)increment;
                    Header.AssignData(ref out_data);

                    for (int i = 0; i < increment; i++)
                        out_data[1 + i] = payload[index + i];

                    status = base.WriteRawData(out_data, 0, out_data.Length, timeout_ms);
                    if (status != Win.ReturnCodes.ERROR_SUCCESS)
                    {
                        #if TRACE
                        Log.WriteLine("MC4UsbDevice.Write() WriteRawData() failed: " + Win.GetSystemErrorCodeAsString(status), Log.Level.ERROR);
                        #endif
                        return status;
                    }
                }

                return Win.ReturnCodes.ERROR_SUCCESS;

#if TRACE
            }
            catch (Exception e)
            {
                Log.WriteLine("MC4UsbDevice.Write() exception: " + e.ToString(), Log.Level.ERROR);
                throw;
            }
#endif

        }


        internal byte[] PacketRead(out Win.ReturnCodes return_value, int timeout)
        {

#if TRACE
            try
            {
#endif

                byte[] message = null;
                return_value = Win.ReturnCodes.ERROR_SUCCESS;
                MessageInHeader msg_header = new MessageInHeader();
                bool first_packet = true;
                MultiSerialPacketHeader packet_header = new MultiSerialPacketHeader(0);
                byte[] recv_packet = new byte[32];
                msg_header.SizeOfMessage = MessageInHeader.SIZE + MultiSerialPacketHeader.SIZE;

                for (int index = 0; index < msg_header.SizeOfMessage; index += packet_header.ValidBytes)
                {
                    int bytes_read = 0;
                    return_value = base.ReadRawData(recv_packet, 0, recv_packet.Length, out bytes_read, timeout);
                    this.FixUsbByteOrder(ref recv_packet);

                    if (return_value == Win.ReturnCodes.ERROR_READ_FAULT)
                    {
                        packet_header.ValidBytes = 0; // Low speed device causes this error, just ignore it and continue
                        continue;
                    }

                    if (return_value != Win.ReturnCodes.ERROR_SUCCESS)
                        return message;

                    if ((recv_packet[0] & 0xE0) != 0x60) // Port 3, upper three bits in first byte
                    {
                        packet_header.ValidBytes = 0; // MultiSerialPacketHeader.PortMumber is not normal MC2 Port number(3)  
                        continue;
                    }

                    packet_header.AssignThis(ref recv_packet);

                    if (first_packet)
                    {
                        msg_header.AssignThis(ref recv_packet);

                        if ((msg_header.SizeOfMessage < MessageInHeader.SIZE) || (msg_header.SizeOfMessage > 0x04000000))
                        {
                            if ((msg_header.SizeOfMessage == MessageInHeader.SIZE) && (msg_header.Status != 0))
                                break;

                            return_value = Win.ReturnCodes.ERROR_READ_FAULT;
                            return message;
                        }

                        message = new byte[msg_header.SizeOfMessage];
                        first_packet = false;
                    }

                    for (int i = index; (i - index) < packet_header.ValidBytes; i++)
                        message[i] = recv_packet[(i - index) + MultiSerialPacketHeader.SIZE];
                }

                return message;

#if TRACE
            }
            catch (Exception e)
            {
                Log.WriteLine("MC4UsbDevice.PacketRead() failed " + e.ToString(), Log.Level.ERROR);
                throw;
            }
#endif

        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct MultiSerialPacketHeader
        {
            public const byte SIZE = 1;

            private byte internal_value;

            public byte PortMumber
            {
                get
                {
                    return (byte)((this.internal_value & 0xE0) >> 5);
                }
                set
                {
                    this.internal_value = (byte)(((value & 0xE0) << 5) | (this.internal_value & 0x1F));
                }
            }
            public byte ValidBytes
            {
                get
                {
                    return (byte)(this.internal_value & 0x1F);
                }
                set
                {
                    this.internal_value = (byte)((value & 0x1F) | (this.internal_value & 0xE0));
                }
            }


            public MultiSerialPacketHeader(byte packet_bytes)
            {
                this.internal_value = (byte)((3 << 5) | (packet_bytes & 0x1F));
            }


            public void AssignData(ref byte[] data)
            {
                if ((data == null) || (data.Length < SIZE)) return;

                data[0] = this.internal_value;
            }

            public void AssignThis(ref byte[] data)
            {
                if ((data == null) || (data.Length < SIZE))
                    this.internal_value = 0;
                else
                    this.internal_value = data[0];
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MessageOutHeader
        {
            public const int SIZE = 5;

            public uint SizeOfMessage;
            public byte CommandId;

            public MessageOutHeader(byte id, int size)
            {
                this.SizeOfMessage = (uint)(size + SIZE);
                this.CommandId = id;
            }

            public void AssignData(ref byte[] data)
            {
                if ((data == null) || (data.Length < (SIZE + MultiSerialPacketHeader.SIZE))) return;
                unsafe
                {
                    fixed (byte* p_byte = &data[MultiSerialPacketHeader.SIZE])
                    {
                        *((MessageOutHeader*)p_byte) = this;
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MessageInHeader
        {
            public const int SIZE = 6;

            public uint SizeOfMessage;
            public byte CommandId;
            public byte Status;

            public void AssignThis(ref byte[] data)
            {
                if ((data == null) || (data.Length < (SIZE + MultiSerialPacketHeader.SIZE)))
                {
                    this.SizeOfMessage = 0;
                    this.CommandId = 0;
                    this.Status = 0;
                    return;
                }
                unsafe
                {
                    fixed (byte* p_byte = &data[MultiSerialPacketHeader.SIZE])
                    {
                        this = *((MessageInHeader*)p_byte);
                    }
                }
            }

            public void AssignThisNoPacket(ref byte[] data)
            {
                if ((data == null) || (data.Length < SIZE)) return;
                unsafe
                {
                    fixed (byte* p_byte = &data[0])
                    {
                        this = *((MessageInHeader*)p_byte);
                    }
                }
            }
        }


        private void FixUsbByteOrder(ref byte[] data)
        {

#if TRACE
            try
            {
#endif
                for (int t = 0; t < data.Length; t += 4)
                {
                    byte b = data[0 + t]; data[0 + t] = data[3 + t]; data[3 + t] = b;
                    b = data[1 + t]; data[1 + t] = data[2 + t]; data[2 + t] = b;
                }

#if TRACE
            }
            catch (Exception e)
            {
                Log.WriteLine("MC4UsbDevice.FixUsbByteOrder() failed: " + e.ToString(), Log.Level.ERROR);
                throw;
            }
#endif

            }

    }

}

