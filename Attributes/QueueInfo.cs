using System;
using System.Collections.Generic;
using System.Text;

namespace MKES.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueueInfo : Attribute
    {
        public QueueInfo(string name, string routingKey)
        {
            Name = name;
            RoutingKey = routingKey;
        }

        public string Name { get; set; }
        public string RoutingKey { get; set; }
    }
}
