using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using MKES;
using MKES.Attributes;
using MKES.Interfaces;
using MKES.Model;
using MKES.EventStore;
using MKES.EventStore.Tests;
using NUnit.Framework;

namespace MKES.Tests
{
    internal class AggregateRootTest
    {
        public class AggregateRootTestClass : AggregateRoot
        {
            private string _message;
            private int _version;
            private Guid _aggregateId;

            public AggregateRootTestClass(IEventStoreRepository eventStoreRepository) : base(eventStoreRepository)
            {
                Register<ChangeTestEvent>(Apply);
                Register<ChangeVersionEvent>(Apply);
                Register<ChangeAggregateEvent>(Apply);
            }

            private void Apply(ChangeAggregateEvent @event)
            {
                _aggregateId = @event.AggregateId;
                Console.WriteLine($"Test: {_message}. Version: {_version}. AggregateId: {_aggregateId}");
            }

            private void Apply(ChangeTestEvent @event)
            {
                _message = @event.Test;
                Console.WriteLine($"Test: {_message}. Version: {_version}. AggregateId: {_aggregateId}");                
            }

            private void Apply(ChangeVersionEvent @event)
            {
                _version = @event.Version;
                Console.WriteLine($"Test: {_message}. Version: {_version}. AggregateId: {_aggregateId}");
            }

            public void ChangeMessage(string message)
            {
                _message = message;
                ApplyChanges(new ChangeTestEvent() {Test = _message});
            }

            public void ChangeVersion(int version)
            {
                _version = version;
                ApplyChanges(new ChangeVersionEvent() {  Version = version });
            }

            public void ChangeAggregate(Guid newGuid)
            {
                _aggregateId = newGuid;
                ApplyChanges(new ChangeAggregateEvent() {AggregateId = newGuid});
            }
        }

        [StreamInfo("TodoEventStream")]
        [QueueInfo(name: "Test-Queue", routingKey: "Todo-Event-Stream-Queue")]
        public class ChangeTestEvent : Event
        {
            public string Test { get; set; }
        }

        [StreamInfo("TodoEventStream")]
        [QueueInfo(name: "Test-Queue", routingKey: "Todo-Event-Stream-Queue")]
        public class ChangeVersionEvent : Event
        {
            public int Version { get; set; }
        }

        [StreamInfo("TodoEventStream")]
        [QueueInfo(name: "Test-Queue", routingKey: "Todo-Event-Stream-Queue")]
        public class ChangeAggregateEvent : Event
        {
        }

        private IWindsorContainer container = new WindsorContainer();
        private AggregateRootTestClass _sut;

        [SetUp]
        public void SetUp()
        {
            container.Install(FromAssembly.Containing<IOC.Installers.AggregateInstaller>());
            container.Register(
                Component.For<AggregateRootTestClass>()
            );
            _sut = container.Resolve<AggregateRootTestClass>();
        }

        [Test]
        public async Task BasicTest()
        {
            _sut.ChangeAggregate(Guid.NewGuid());
            _sut.ChangeMessage("testing");
            _sut.ChangeVersion(2);
            var changes = _sut.GetUncommitedChanges();
            Assert.IsNotNull(changes);
            Assert.IsNotNull(((ChangeAggregateEvent) changes.First()).AggregateId);
            Assert.AreEqual(changes.Count, 3);
            await _sut.CommitChanges();
        }
    }
}
