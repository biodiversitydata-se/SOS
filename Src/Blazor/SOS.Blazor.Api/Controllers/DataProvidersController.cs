using Microsoft.AspNetCore.Mvc;
using SOS.Blazor.Shared.Models;

namespace SOS.Blazor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataProvidersController : ControllerBase
    {
        private readonly ILogger<DataProvidersController> _logger;

        public DataProvidersController(ILogger<DataProvidersController> logger)
        {
            _logger = logger;
        }

        [HttpGet("DataProviderStatus")]
        public async Task<IEnumerable<DataProviderStatus>?> GetDataProviderStatus([FromQuery] ApiEnvironment apiEnvironment = ApiEnvironment.Dev)
        {
            var sosClient = new SosClient("https://sos-search-dev.artdata.slu.se/");
            ProcessInfo? activeProcessInfo = await sosClient.GetProcessInfo(true);
            ProcessInfo? inactiveProcessInfo = await sosClient.GetProcessInfo(false);
            var providers = await sosClient.GetDataProviders();
            var result = GetDataProviderStatuses(providers, activeProcessInfo, inactiveProcessInfo);
            return result.ToArray();
        }
        
        private List<DataProviderStatus> GetDataProviderStatuses(DataProvider[] dataProviders, ProcessInfo activeProcessInfo, ProcessInfo inactiveProcessInfo)
        {
            List<DataProviderStatus> result = new List<DataProviderStatus>();
            foreach (var dataProvider in dataProviders)
            {
                var dataProviderStatus = new DataProviderStatus();
                dataProviderStatus.Id = dataProvider.Id;
                dataProviderStatus.Identifier = dataProvider.Identifier;
                dataProviderStatus.Name = dataProvider.Name;
                dataProviderStatus.Organization = dataProvider.Organization;
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