using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Autofac;
using SOS.Core.IoC.Modules;

namespace SOS.Core.IoC
{
    public static class BootstrapContainer
    {
        public static ContainerBuilder Boostrap()
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyModules(typeof(CoreModule).Assembly, Assembly.GetEntryAssembly());

            return builder;
        }
    }
}
