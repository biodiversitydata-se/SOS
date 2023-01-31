using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.DataStewardship.Api.IntegrationTests.Extensions
{
    internal static class ServiceProviderExtensions
    {
        public static ServiceProvider RegisterServices(params ServiceCollection[] serviceCollections)
        {
            var serviceCollection = new ServiceCollection();
            foreach (var collection in serviceCollections)
            {
                serviceCollection.Add(collection);
            }

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
