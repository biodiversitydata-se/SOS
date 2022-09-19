namespace SOS.Blazor.Api.Modules
{
    public class DataProvidersModule : IModule
    {
        public void MapEndpoints(WebApplication application)
        {
            application.MapGet("dataproviders/dataproviderstatus", async (ISosClient sosClient) =>
            {
                var activeProcessInfo = await sosClient.GetProcessInfoAsync(true);
                var inactiveProcessInfo = await sosClient.GetProcessInfoAsync(false);
                var providers = await sosClient.GetDataProvidersAsync();
                return GetDataProviderStatuses(providers, activeProcessInfo, inactiveProcessInfo);
            });
        }

        private static IEnumerable<DataProviderStatus> GetDataProviderStatuses(DataProvider[] dataProviders, ProcessInfo activeProcessInfo, ProcessInfo inactiveProcessInfo)
        {
            var result = new List<DataProviderStatus>();
            foreach (var dataProvider in dataProviders)
            {
                var dataProviderStatus = new DataProviderStatus
                {
                    Id = dataProvider.Id,
                    Identifier = dataProvider.Identifier,
                    Name = dataProvider.Name,
                    Organization = dataProvider.Organization
                };
                var active = activeProcessInfo.ProvidersInfo.FirstOrDefault(m => m.DataProviderId == dataProvider.Id);
                var inactive = inactiveProcessInfo.ProvidersInfo.FirstOrDefault(m => m.DataProviderId == dataProvider.Id);
                if (active != null)
                {
                    dataProviderStatus.PublicObservationsActiveInstanceCount = active.PublicProcessCount.GetValueOrDefault();
                    dataProviderStatus.ProtectedObservationsActiveInstanceCount = active.ProtectedProcessCount.GetValueOrDefault();
                }
                if (inactive != null)
                {
                    dataProviderStatus.PublicObservationsInactiveInstanceCount = inactive.PublicProcessCount.GetValueOrDefault();
                    dataProviderStatus.ProtectedObservationsInactiveInstanceCount = inactive.ProtectedProcessCount.GetValueOrDefault();
                }

                result.Add(dataProviderStatus);
            }

            return result;
        }
    }
}
