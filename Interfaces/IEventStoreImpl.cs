using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using MKES.Model;

namespace MKES.Interfaces
{
    public interface IEventStore : IDisposable
    {
        Task Connect();
        Task<ResolvedEvent[]> ReadAllEventsForAGivenStream(string streamName);
        Task WriteEventToStream(Event @event);
    }
}