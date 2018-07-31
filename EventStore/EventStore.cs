using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using MKES.Interfaces;
using MKES.Model;
using MKES.Attributes;
using Newtonsoft.Json;

namespace MKES.EventStore
{
    public class EventStoreImpl : IEventStore
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private bool _connected = false;

        public EventStoreImpl()
        {
            _eventStoreConnection = EventStoreConnection.Create(ConnectionSettings.Default, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113));
        }

        public async Task Connect()
        {
            _eventStoreConnection.Connected += EventStoreConnectionOnConnected;
            _eventStoreConnection.ConnectAsync().Wait();
        }

        private void EventStoreConnectionOnConnected(object sender, ClientConnectionEventArgs e)
        {
            Console.WriteLine($"EventStore has been connected. {_eventStoreConnection.ConnectionName}");
            _connected = true;
        }

        public async Task<ResolvedEvent[]> ReadAllEventsForAGivenStream(string streamName)
        {
            var task = await _eventStoreConnection.ReadStreamEventsForwardAsync(streamName, 0, 4096, false);
            return task.Events;
        }

        public async Task WriteEventToStream(Event @event)
        {
            var metaData = GetMetaDataFromEvent(@event);
            var info = GetStreamInfoFromAttribute(@event);
            byte[] data = Encoding.UTF8.GetBytes(EventModel.GetEventModelFromEvent(@event).ToJson());
            EventData eventData = new EventData(@event.AggregateId, @event.GetType().Name, true, data, metaData);
            await _eventStoreConnection.AppendToStreamAsync(info.Name, ExpectedVersion.Any, eventData);
        }

        private StreamInfo GetStreamInfoFromAttribute(Event @event)
        {
            var infoAttr = @event.GetType().GetCustomAttributes(typeof(StreamInfo), false);
            StreamInfo info = (StreamInfo) infoAttr[0];
            return info;
        }

        private byte[] GetMetaDataFromEvent(Event @event)
        {
            byte[] metaData = Encoding.UTF8.GetBytes("{}");
            var metaAttr = @event.GetType().GetCustomAttributes(typeof(Metadata), false);
            if (metaAttr.Length > 0)
            {
                var intf = @event.GetType().GetInterfaces().FirstOrDefault(s => s.FullName.Contains("IMetaEvent"));
                var mi = intf.GetMethods()[0];
                var result = mi.Invoke(@event, new object[] { });
                metaData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
            }

            return metaData;
        }

        public void Dispose()
        {
            _eventStoreConnection?.Dispose();
        }
    }
}