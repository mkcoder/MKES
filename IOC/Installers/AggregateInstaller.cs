using System;
using System.Collections.Generic;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using MKES.EventBus;
using MKES.EventStore;
using MKES.Interfaces;

namespace MKES.IOC.Installers
{
    public class AggregateInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IEventStoreRepository>().ImplementedBy<DefaultEventStoreRepository>().LifestyleTransient(),
                Component.For<IEventStore>().ImplementedBy<EventStoreImpl>().LifestyleTransient(),
                Component.For<IEventBus>().ImplementedBy<EventBusImpl>().LifestyleTransient()
            );
        }
    }
}
