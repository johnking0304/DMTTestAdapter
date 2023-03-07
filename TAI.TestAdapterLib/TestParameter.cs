using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMTTestAdapter
{


    public enum SystemCode
    {
        Ok = 0,
        Fault = 1,

    }


    public enum OperateCommand
    { 
        None =0,
        Initialize =1,
        StartTest =2,
        StopTest =3,
        StartStationTest =4,
        StopStationTest =5,
    }





}
