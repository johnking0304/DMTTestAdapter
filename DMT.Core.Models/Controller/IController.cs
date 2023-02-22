using System;
using System.Collections.Generic;
using System.Text;

namespace DMT.Core.Models
{
    public interface IController
    {
        bool Initialize();
        bool Open();
        bool Close();
        bool Active();
        void Start();

    }
}
