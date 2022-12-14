using Microsoft.Extensions.Logging;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers
{
    public class DevOpsManager : IDevOpsManager
    {
        private readonly IDevOpsService _devOpsService;
        private readonly DevOpsConfiguration _devOpsConfiguration;
        private readonly ILogger<DevOpsManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="devOpsService"></param>
        /// <param name="devOpsConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DevOpsManager(
             IDevOpsService devOpsService,
             DevOpsConfiguration devOpsConfiguration,
             ILogger<DevOpsManager> logger
        )
        {
            _devOpsService = devOpsService ?? throw new ArgumentNullException(nameof(devOpsService));
            _devOpsConfiguration = devOpsConfiguration ??
                                             throw new ArgumentNullException(nameof(devOpsConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<string> GetBuildInfoAsync()
        {
            var buildInfo = string.Empty;
            if (!_devOpsConfiguration?.Releases?.Any() ?? true)
            {
                return buildInfo;
            }
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // Just for debug 
            if (environment.Equals("local", StringComparison.CurrentCultureIgnoreCase))
            {
                environment = "dev";
            }

            foreach (var releaseDefinition in _devOpsConfiguration.Releases)
            {
                var response = await _devOpsService.GetReleasesAsync(releaseDefinition.Id);

                if (response.Count == 0 )
                {
                    continue;
                }
                var releases = response.Value;
                foreach (var release in releases)
                {
                    var releaseDetails = await _devOpsService.GetReleaseAsync(release.Id);

                    if (!releaseDetails?.Environments?.Any() ?? true)
                    {
                        continue;
                    }

                    var currentEnvironment = releaseDetails.Environments.FirstOrDefault(e => 
                        e.Name.Equals(environment, StringComparison.CurrentCultureIgnoreCase)
                        && e.CreatedOn.HasValue
                    );
                    if (currentEnvironment != null)
                    {
                        if (!string.IsNullOrEmpty(buildInfo))
                        {
                            buildInfo += "\n";
                        }
                        buildInfo += $"{releaseDefinition.Name}: {currentEnvironment.CreatedOn.Value.ToShortDateString()}";
                        break;
                    }
                }
            }

           
            return buildInfo;
        }
    }
}
