using System;
using System.Collections.Generic;
using System.Text;

namespace DMT.Models
{
    public interface IController
    {
        bool Initialize(string filename);
        bool Open();
        bool Close();
        bool Active();
    }
}
