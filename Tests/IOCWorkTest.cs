using System;
using System.Collections.Generic;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using MKES.IOC;
using NUnit.Framework;

namespace MKES.Tests
{
    internal class IOCWorkTest
    {
        private IWindsorContainer _container = IOCBootstrapper.GetWindsorContainer();

        interface IA
        {
            string GetString();
        }

        public class A : IA
        {
            public string GetString()
            {
                return "Hello";
            }
        }

        [Test]
        public void Test()
        {
            _container.Register(Component.For<IA>().ImplementedBy<A>());
            var a = _container.Resolve<IA>();
            Console.WriteLine(a.GetString());
        }
    }
}
