using System;
using DMT.Protocols;
using DMT.Models;

namespace DMTTestAapter
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

        public bool Initialize(string filename)
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
    }
}
