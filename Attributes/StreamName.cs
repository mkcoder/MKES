using System;

namespace MKES.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StreamInfo : Attribute
    {
        public string Name { get; set; }

        public StreamInfo(string name)
        {
            Name = name;
        }
    }
}
