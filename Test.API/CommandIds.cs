using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.API
{
    public enum CommandIds : byte
    {
        Shutdown = 1,
        SetMessage = 2,
        GetMessage = 3,
        AddNumbers = 4
    }
}
