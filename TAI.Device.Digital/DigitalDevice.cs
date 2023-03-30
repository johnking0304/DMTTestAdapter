using System;
using DMT.Core.Models;
using DMT.Core.Protocols;
using DMT.Core.Utils;


namespace TAI.Device
{

    public class DigitalDevice : OperatorController, IController
    {
        public ModbusTCPClient Channel { get; set; }

        public DigitalOperator DigitalOperator { get; set; }


        public DigitalDevice() : base()
        {
            this.Caption = "DigitalModelPLC";
            this.Channel = new ModbusTCPClient(this.Caption);
            
            this.StatusMessage.Name = this.Caption;

        }

        public bool Active()
        {
            return (!this.Channel.HasError);
        }

        public bool Close()
        {
            return true;
        }


        public override void LoadFromFile(string fileName)
        {
            this.Channel.Initialize(fileName);
            this.DigitalOperator = new DigitalOperator(this.Channel.BaseIndex);
            this.DigitalOperator.LoadFromFile(fileName);
        }

        public bool Initialize()
        {
            this.SelectPIChannel(0);
            this.SelectPWChannel(0);
            this.SetPluseOutputMode(PluseMode.None);
            return true;
        }

        public bool Open()
        {
            return this.Channel.ConnectToTCPServer();
        }

        public void Start()
        {   
            this.StartThread();
        }



        public bool SetValue(int channelId, bool value)
        {
            byte[] bytes =ByteUtils.UshortsToBytes(new ushort[1] { (ushort)0 });
            if (channelId >= 1 && channelId <= 8)
            {


                byte data = 0;
                bytes[0] = ByteUtils.SetBitValue(data, (byte)(channelId - 1), value);
                ushort[] ushorts = ByteUtils.BytesToUshorts(bytes);
                this.DigitalOperator.OutputChannels.Datas[0] = ushorts[0];
                this.DigitalOperator.OutputChannels.Datas[1] = 0;
            }else if (channelId >= 9 && channelId <= 16)
            {
                byte data = 0;
                bytes[1] = ByteUtils.SetBitValue(data, (byte)(channelId - 9), value);
                ushort[] ushorts = ByteUtils.BytesToUshorts(bytes);
                this.DigitalOperator.OutputChannels.Datas[0] = ushorts[0];
                this.DigitalOperator.OutputChannels.Datas[1] = 0;
            }else if (channelId >= 17 && channelId <= 24)
            {
                byte data = 0;
                bytes[0] = ByteUtils.SetBitValue(data, (byte)(channelId - 17), value);
                ushort[] ushorts = ByteUtils.BytesToUshorts(bytes);
                this.DigitalOperator.OutputChannels.Datas[0] = 0;
                this.DigitalOperator.OutputChannels.Datas[1] = ushorts[0];
            }
          

            this.Channel.WriteMultipleRegisters(this.DigitalOperator.OutputChannels.StartAddress, this.DigitalOperator.OutputChannels.Datas);
            return !this.Channel.HasError;
        }

        public bool SetValues(int channelIdMask, uint valueMask)
        {
            return true;
        }

        public bool GetValues(int channelIdMask, ref uint valueMask)
        {
            return true;
        }

        public bool SelectPWChannel(int channelId)
        {
            this.DigitalOperator.PWChannelSelectId.Datas[0] = (ushort)channelId;

            this.Channel.WriteMultipleRegisters(this.DigitalOperator.PWChannelSelectId.StartAddress, this.DigitalOperator.PWChannelSelectId.Datas);
            return !this.Channel.HasError;
        }

        public bool SetPluseOutputMode(PluseMode mode)
        {
            this.DigitalOperator.PluseOutputMode.Datas[0] = (ushort)mode;

            this.Channel.WriteMultipleRegisters(this.DigitalOperator.PluseOutputMode.StartAddress, this.DigitalOperator.PluseOutputMode.Datas);
            return !this.Channel.HasError;
        }


        public bool SelectPIChannel(int channelId)
        {
            this.DigitalOperator.PIChannelSelectId.Datas[0] = (ushort)channelId;

            this.Channel.WriteMultipleRegisters(this.DigitalOperator.PIChannelSelectId.StartAddress, this.DigitalOperator.PIChannelSelectId.Datas);
            return !this.Channel.HasError;
        }


        public float GetVoltage()
        {
            ushort[] data = this.Channel.ReadHoldingRegisters(this.DigitalOperator.MeasureVoltage.StartAddress, this.DigitalOperator.MeasureVoltage.Length);
            if (!this.Channel.HasError)
            {
                return ByteUtils.GetSingle(data, 0);
            }
            return (float)0.0;
        }

        public bool GetValue(int channelId,ref bool result)
        {
            ushort[] data = this.Channel.ReadHoldingRegisters(this.DigitalOperator.InputChannels.StartAddress, this.DigitalOperator.InputChannels.Length);
            if (!this.Channel.HasError)
            {

                result = (data[0] >> (channelId - 1) & 1) == 1;
                return true;
                               
            }
            return false;
        }

        public override void ProcessEvent()
        {
            if (this.Active())
            {

            }
            else
            {
                //通道读写错误，重连判断及动作
                this.Channel.ReConnectTCPServer();
            }
        }

    }
}

