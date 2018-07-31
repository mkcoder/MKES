using System;
using System.Collections.Generic;
using System.Text;

namespace MKES.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class Metadata : Attribute
    {
    }
}
