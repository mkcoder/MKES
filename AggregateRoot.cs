using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MKES.Interfaces;
using MKES.Model;

namespace MKES
{
    public abstract class AggregateRoot
    {
        private readonly IEventStoreRepository _eventStoreRepository;
        private List<Event> _uncommitedChanges = new List<Event>();
        private Dictionary<Type, dynamic> _changes = new Dictionary<Type, dynamic>();

        public IReadOnlyCollection<Event> GetUncommitedChanges() => _uncommitedChanges.AsReadOnly();

        public AggregateRoot(IEventStoreRepository eventStoreRepository)
        {
            _eventStoreRepository = eventStoreRepository;
        }

        public async Task CommitChanges()
        {
            await _eventStoreRepository.Commit(_uncommitedChanges);
            _uncommitedChanges = new List<Event>();
        }

        protected void Register<TEvent>(Action<TEvent> aggregate) where TEvent : Event, new()
        {
            _changes.Add(typeof(TEvent), aggregate);
        }

        protected void ApplyChanges<T>(T @event) where T: Event
        {
            _uncommitedChanges.Add(@event);
            _changes[typeof(T)].Invoke(@event);
        }
    }
     
}
