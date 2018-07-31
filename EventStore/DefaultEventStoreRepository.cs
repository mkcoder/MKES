using System.Collections.Generic;
using System.Threading.Tasks;
using MKES.Interfaces;
using MKES.Model;

namespace MKES.EventStore
{
    public class DefaultEventStoreRepository : IEventStoreRepository
    {
        private readonly IEventStore _eventStore;
        private readonly IEventBus _eventBus;

        public DefaultEventStoreRepository(IEventStore eventStore, IEventBus eventBus)
        {
            _eventStore = eventStore;
            _eventBus = eventBus;
        }

        public async Task Commit(List<Event> uncommitedChanges)
        {
            _eventStore.Connect();                        
            foreach (var @event in uncommitedChanges)
            {
                await _eventStore.WriteEventToStream(@event);
                _eventBus.Publish(@event);
            }
        }
    }
}
