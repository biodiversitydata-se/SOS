namespace SOS.DataStewardship.Api.IntegrationTests.Core.Extensions
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
