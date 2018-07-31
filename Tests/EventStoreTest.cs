using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using MKES.Attributes;
using MKES.Model;
using MKES.EventStore;
using MKES.Interfaces;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MKES.EventStore.Tests
{
    internal class TestEventMetaData
    {
        public int Version { get; set; }
    }

    [StreamInfo("TestStream")]
    [Metadata]
    internal class TestEvent : Event, IMetaEvent<TestEventMetaData>
    {
        public string EventMessage { get; set; }
        public DateTime EventTime { get; set; }

        public TestEventMetaData GetMetaData()
        {
            return new TestEventMetaData(){Version = new Random().Next()};
        }

        public override string ToString()
        {
            return base.ToString() + $" EventMessage: {EventMessage}, EventTime: {EventTime}";
        }
    }

    [TestFixture]
    [SingleThreaded]
    internal class EventStoreTest
    {
        private EventStoreImpl _eventStore = new EventStoreImpl();

        [OneTimeSetUp]
        public async Task Setup()
        {
            await _eventStore.Connect();
        }

        [Test]
        public async Task WriteAnEventToTheEventStore()
        {
           Task.Run(async () =>
           {
               await _eventStore.WriteEventToStream(new TestEvent()
               {
                   AggregateId = Guid.NewGuid(),
                   Version = 1,
                   EventMessage = "This is a sample message",
                   EventTime = DateTime.Now
               });
           }).GetAwaiter().GetResult();
        }

        [Test]
        public async Task ReadAnEventToTheEventStore()
        {
            var result = Task.Run(async () =>
            {
                return await _eventStore.ReadAllEventsForAGivenStream("TestStream");
            }).GetAwaiter().GetResult();

            foreach (var resolvedEvent in result)
            {
                Console.WriteLine(JsonConvert.SerializeObject(resolvedEvent));
                Console.WriteLine($"Data: {Encoding.UTF8.GetString(resolvedEvent.Event.Data)}");
                Console.WriteLine($"Class data: {JsonConvert.DeserializeObject<TestEvent>(Encoding.UTF8.GetString(resolvedEvent.Event.Data))}");
                Console.WriteLine($"Metadata: {Encoding.UTF8.GetString(resolvedEvent.Event.Metadata)}");
            }
        }
    }
}
