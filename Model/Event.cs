using System;
using System.Collections.Generic;
using System.Text;
using EventStore.ClientAPI;
using MKES.Interfaces;

namespace MKES.Model
{
    public abstract class Event : IMessage
    {
        public int Version { get; set; }
        public Guid AggregateId { get; set; }

        public Event()
        {
            AggregateId = Guid.NewGuid();
        }
    }
}
