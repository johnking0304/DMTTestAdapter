using System;
using DMT.Models;
using DMT.Protocols;

namespace DMTTestAapter
{

    public class DigitalDevice : Controller, IController
    {
        public ModbusTCPClient Channel { get; set; }

        public bool Active()
        {
            throw new NotImplementedException();
        }

        public bool Close()
        {
            throw new NotImplementedException();
        }

        public bool Initialize(string filename)
        {
            throw new NotImplementedException();
        }

        public bool Open()
        {
            throw new NotImplementedException();
        }
    }
}

