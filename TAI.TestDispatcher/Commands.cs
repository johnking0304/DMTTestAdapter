using DMT.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace TAI.TestDispatcher
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ADLHeader
    {
        public byte HDRType;
        public byte CMDType;
        public  ushort SequenceNum;
        public ushort Length;
        public ushort Port;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
        public byte[] IpAddress ;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.U2)]
        public ushort[] Reserved;
        public ushort IpCheckSum;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 24, ArraySubType = UnmanagedType.U1)]
        public byte[] HostName;       
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BlockHeader
    {
        public ushort MemoryType;
        public ushort ByteCount;
        public uint   StartAddress;
    };



    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ADLData
    {
        public uint ConnectionID;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 200, ArraySubType = UnmanagedType.U1)]
        public  byte[]  CommandString;
    };







    public class CalibrateCommand
    {
        public const int HDRTYPE = 0x06;
        public const int WRITEBLOCK = 0x05;
        public const int READBLOCK = 0x04;
        public ADLHeader ADLHeader  =new ADLHeader();
        public BlockHeader BlockHeader;
        public ADLData ADLData;
        public string CommandContent { get; set; }

        public BaseCalibrator Calibrator { get; set; }

        public CalibrateCommand(BaseCalibrator calibrator)
        {
            this.Calibrator = calibrator;
            this.ADLData.ConnectionID = 1000;
            this.SetADLHeader();
            this.SetBlockHeader();
            this.SetADLData();
            this.CommandContent = "";
        }

        private void SetADLData()
        {

            this.ADLData.CommandString = new byte[200];
        }
     
            
        
        
        private void SetADLHeader()
        {
            this.ADLHeader.HDRType = HDRTYPE;
            this.ADLHeader.CMDType = READBLOCK;
            this.ADLHeader.SequenceNum = 0;
            this.ADLHeader.Port = (ushort)this.Calibrator.Controller.UDPService.Port;
            this.ADLHeader.IpAddress = new byte[4] { 0xc0, 0xa8, 0x01, byte.Parse(this.Calibrator.ActiveCardModule.IPAddress) };


        }
        private void SetBlockHeader()
        {
            this.BlockHeader.MemoryType = 0x22;
            this.BlockHeader.ByteCount = 0x29;
            this.BlockHeader.StartAddress = 0x00;
        }

        public virtual string Pack()
        {
            return this.CommandContent;
        }
        public virtual byte[]  Package()
        {
            string command = this.Pack();
            byte[] buffer = Encoding.ASCII.GetBytes(command);
            Array.Copy(buffer, this.ADLData.CommandString, buffer.Length);

            int adlLength = Marshal.SizeOf(typeof(ADLHeader));
            int blockLength = Marshal.SizeOf(typeof(BlockHeader));
            int dataLength = Marshal.SizeOf(typeof(ADLData));

            this.Calibrator.SequenceNumber += 1;
            this.ADLHeader.SequenceNum = this.Calibrator.SequenceNumber;
            this.ADLHeader.Length = (ushort)(blockLength + dataLength);  

            int LAST_PACKET_LEN = adlLength + blockLength + dataLength;
            byte[] SendBuffer = new byte[LAST_PACKET_LEN];

            byte[] data =ByteUtils.StructToBytes(this.ADLHeader, typeof(ADLHeader));
                       
            Array.Copy(data,0, SendBuffer,0, data.Length);

            data = ByteUtils.StructToBytes(this.BlockHeader, typeof(BlockHeader));

            Array.Copy(data, 0, SendBuffer, adlLength, data.Length);

            data = ByteUtils.StructToBytes(this.ADLData, typeof(ADLData));

            Array.Copy(data, 0, SendBuffer, adlLength+ blockLength, data.Length);

            return SendBuffer;
        }

    }


    public class ConnectCommand: CalibrateCommand
    {
        public ConnectCommand(BaseCalibrator calibrator) : base(calibrator)
        {
            this.ADLHeader.CMDType = WRITEBLOCK;
            this.CommandContent = "CONNECT";
        }


    }



    public class CloseCommand : CalibrateCommand
    {
        public CloseCommand(BaseCalibrator calibrator) : base(calibrator)
        {
            this.ADLHeader.CMDType = WRITEBLOCK;
            this.CommandContent = "CLOSE";
        }
    }



    public class SetCalibrateValueCommand : CalibrateCommand
    {
        
        public SetCalibrateValueCommand(BaseCalibrator calibrator,string setContent) : base(calibrator)
        {
            this.CommandContent = setContent;
        }

    }

    public class SaveCalibrateValueCommand : CalibrateCommand
    {
        public SaveCalibrateValueCommand(BaseCalibrator calibrator, string setContent) : base(calibrator)
        {
            this.CommandContent = setContent;
        }

    }


    public class ReadCalibrateValueCommand : CalibrateCommand
    {
        public ReadCalibrateValueCommand(BaseCalibrator calibrator) : base(calibrator)
        {
            this.ADLHeader.CMDType = READBLOCK;
        }


        public override byte[] Package()
        {

            int adlLength = Marshal.SizeOf(typeof(ADLHeader));
            int blockLength = Marshal.SizeOf(typeof(BlockHeader));

            this.Calibrator.SequenceNumber += 1;
            this.ADLHeader.SequenceNum = this.Calibrator.SequenceNumber;
            this.ADLHeader.Length = (ushort)blockLength;

            int LAST_PACKET_LEN = adlLength + blockLength;
            byte[] SendBuffer = new byte[LAST_PACKET_LEN];

            byte[] data = ByteUtils.StructToBytes(this.ADLHeader, typeof(ADLHeader));

            Array.Copy(data, 0, SendBuffer, 0, data.Length);

            data = ByteUtils.StructToBytes(this.BlockHeader, typeof(BlockHeader));

            Array.Copy(data, 0, SendBuffer, adlLength, data.Length);

            return SendBuffer;

        }
    }

}
