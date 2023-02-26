using System;
using DMT.Core.Models;
using DMT.Core.Protocols;

namespace TAI.Device
{

    public class DigitalDevice : Controller, IController
    {
        public ModbusTCPClient Channel { get; set; }


        public DigitalDevice() : base()
        { 
            
        
        
        }

        public bool Active()
        {
            throw new NotImplementedException();
        }

        public bool Close()
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public bool Open()
        {
            throw new NotImplementedException();
        }

        public void Start()
        { 
        
        }

        public bool SetValue(int channelId, bool value)
        {
            return true;
        }

        public bool GetValue(int channelId)
        {
            return true;
        }

    }
}

