using System;
using MKES.Interfaces;
using NUnit.Framework.Constraints;

namespace MKES.Model
{
    public abstract class Command : IMessage
    {
        public int Version { get; set; }
        public Guid AggregateId { get; set; }

        public Command()
        {
            AggregateId = Guid.NewGuid();
            Version = 1;
        }
    }
}