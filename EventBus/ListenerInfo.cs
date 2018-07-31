using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MKES.Attributes;
using NUnit.Compatibility;

namespace MKES.EventBus
{
    public class ListenerInfo
    {
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }

        public static ListenerInfo FromCommand(Type type)
        {
            var attr = type.GetCustomAttribute(typeof(CommandInfo));
            if(attr == null) throw new Exception("Type must have an CommandInfo attribute");
            var q = (CommandInfo) attr;
            return new ListenerInfo() {QueueName = q.Name, RoutingKey = q.RoutingKey};
        }
    }
}
