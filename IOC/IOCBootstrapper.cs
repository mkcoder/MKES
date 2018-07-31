using System.Reflection;
using Castle.Windsor;
using Castle.Windsor.Installer;

namespace MKES.IOC
{
    public static class IOCBootstrapper 
    {
        public static IWindsorContainer GetWindsorContainer()
        {
            return new WindsorContainer()
                .Install(FromAssembly.InThisApplication(Assembly.GetExecutingAssembly()));
        }
    }
}
