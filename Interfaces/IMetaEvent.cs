using System;
using System.Collections.Generic;
using System.Text;

namespace MKES.Interfaces
{
    interface IMetaEvent<T> where T : new()
    {
        T GetMetaData();
    }
}
