using System;
using DMT.Protocols;
using DMT.Models;


namespace DMTTestAdapter
{ 
     public class SwitchController : Controller, IController
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
