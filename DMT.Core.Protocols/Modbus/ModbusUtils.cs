using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;


namespace DMT.Core.Protocols
{
    public enum EVENT_MODBUS
    {
        MODBUS_TCP_INIT = 2000,
        MODBUS_TCP_WRITE = 2001,
        MODBUS_TCP_READ = 2003,
    }

    public enum ModbusErrorCode
    {
        Ok = 0,
        InitError = 1,
        ReadError = 2,
        WriteError = 3
    }

    public enum ModbusDataType
    {
        Coil = 0,
        Input = 1,
        HoldingRegister = 2,
        InputRegister = 3,
    }


    public enum ChannelType
    {
        AI = 0,
        AO = 1,
        DI = 2,
        DO = 3,
    }

    public class ModbusItem
    {
        public string Section { get; set; }
        public string Caption { get; set; }
        public ChannelType ChannelType { get; set; }
        public short BaseAddress { get; set; }
        public string Name { get; set; }
        public ushort Offset { get; set; }
        public ushort Length { get; set; }
        public ushort[] Datas { get; set; }
        public string DataValue { get;set;}

        public Boolean Enable { get; set; }

        public ushort StartAddress
        {
            get
            {
                return (ushort)(this.BaseAddress + this.Offset);
            }
        }


        public ModbusItem()
        { 
        
        }


        public ModbusItem(short baseIndex,ushort offset, ushort length, ChannelType type)
        {
            this.Length = length;
            this.Offset = offset;
            this.Enable = true;
            this.ChannelType = type;
            this.Initialize(baseIndex);
        }
        
        public ModbusItem(string section, string caption,string name, short baseIndex, ushort offset, ushort length, ChannelType type)
        {
            this.Section = section;
            this.Caption = caption;
            this.Name = name;
            this.Length = length;
            this.Offset = offset;
            this.Enable = false;
            this.ChannelType = type;
            this.Initialize(baseIndex);
        }
        public void Initialize(short baseIndex)
        {
            this.Datas = new ushort[this.Length];
            this.BaseAddress = baseIndex;
        }

        public void Clear()
        {
            this.DataValue = "";
            this.Enable = false;
            for (int i = 0; i < this.Length; i++)
            {
                this.Datas[i] = 0;
            }
        }

        private string offsetKey
        {
            get
            {
                return this.Name + ".offset";
            }
        }

        private string lengthKey
        {
            get
            {
                return this.Name + ".Length";
            }
        }

        public void LoadFromFile(string fileName)
        {
            this.Offset = (ushort)IniFiles.GetIntValue(fileName, this.Section, this.offsetKey, this.Offset);
            this.Length = (ushort)IniFiles.GetIntValue(fileName, this.Section, this.lengthKey, this.Length);

            string[] list = IniFiles.GetAllSectionNames(fileName);
            if (!list.Contains(this.Name))
            {
                this.SaveToFile(fileName);
            }

        }

        public void SaveToFile(string fileName)
        {
            IniFiles.WriteIntValue(fileName, this.Section, this.offsetKey, this.Offset);
            IniFiles.WriteIntValue(fileName, this.Section, this.lengthKey, this.Length);
        }





    }

}
