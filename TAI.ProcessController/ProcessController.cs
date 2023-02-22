using System;
using DMT.Core.Protocols;
using DMT.Core.Models;

namespace TAI.Manager
{
    /// <summary>
    /// 测试流程控制
    /// </summary>
    public class ProcessController: Controller, IController
    {
        public ModbusTCPClient Channel { get; set; }



        public ProcessController()
        { 
            
        }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public bool Open()
        {
            throw new NotImplementedException();
        }

        public bool Close()
        {
            throw new NotImplementedException();
        }

        public bool Active()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {

            return;
        }
    }
}
