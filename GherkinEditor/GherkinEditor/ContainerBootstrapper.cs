using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;
using System.Windows.Controls;

namespace Gherkin
{
    static class ContainerBootstrapper
    {
        public static void Resiger(IUnityContainer container)
        {
            container
                .AddNewExtension<Util.ContainerExtension4Util>()
                .RegisterType<View.MainWindow>(new ContainerControlledLifetimeManager());
        }
    }
}
