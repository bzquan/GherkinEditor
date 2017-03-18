using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;


namespace Gherkin.Util
{
    public class ContainerExtension4Util : UnityContainerExtension
    {
        protected override void Initialize()
        {
            base.Container
                .RegisterType<IAppSettings, AppSettings>(new ContainerControlledLifetimeManager());
        }
    }
}
