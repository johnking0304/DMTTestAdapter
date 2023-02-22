using System;
using DMT.Core.Protocols;
using DMT.Core.Models;


namespace TAI.Manager
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
            throw new NotImplementedException();
        }


        public bool SwitchChannel(int channelId)
        {

            return true;
        }
    }
}
