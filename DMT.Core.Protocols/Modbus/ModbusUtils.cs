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
        public string section { get; set; }
        public ChannelType channelType { get; set; }
        public short baseIndex { get; set; }
        public string name { get; set; }
        public ushort offset { get; set; }
        public ushort length { get; set; }
        public ushort[] datas { get; set; }
        public string dataValue { get;set;}

        public Boolean enable { get; set; }

        public ushort startIndex
        {
            get
            {
                return (ushort)(this.baseIndex + this.offset);
            }
        }


        public ModbusItem()
        { 
        
        }


        public ModbusItem(short baseIndex,ushort offset, ushort length, ChannelType type)
        {
            this.length = length;
            this.offset = offset;
            this.enable = true;
            this.channelType = type;
            this.Initialize(baseIndex);
        }
        public ModbusItem(string section, string name, short baseIndex, ushort offset, ushort length, ChannelType type)
        {
            this.section = section;
            this.name = name;
            this.length = length;
            this.offset = offset;
            this.enable = false;
            this.channelType = type;
            this.Initialize(baseIndex);
        }
        public void Initialize(short baseIndex)
        {
            this.datas = new ushort[this.length];
            this.baseIndex = baseIndex;
        }

        public void Clear()
        {
            this.dataValue = "";
            this.enable = false;
            for (int i = 0; i < this.length; i++)
            {
                this.datas[i] = 0;
            }
        }

        private string offsetKey
        {
            get
            {
                return this.name + ".offset";
            }
        }

        private string lengthKey
        {
            get
            {
                return this.name + ".Length";
            }
        }

        public void LoadFromFile(string fileName)
        {
            this.offset = (ushort)IniFiles.GetIntValue(fileName, this.section, this.offsetKey, this.offset);
            this.length = (ushort)IniFiles.GetIntValue(fileName, this.section, this.lengthKey, this.length);

            string[] list = IniFiles.GetAllSectionNames(fileName);
            if (!list.Contains(this.name))
            {
                this.SaveToFile(fileName);
            }

        }

        public void SaveToFile(string fileName)
        {
            IniFiles.WriteIntValue(fileName, this.section, this.offsetKey, this.offset);
            IniFiles.WriteIntValue(fileName, this.section, this.lengthKey, this.length);
        }





    }

}
