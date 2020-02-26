using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Factories.Interfaces;
using SOS.Export.Jobs.Interfaces;
using SOS.Lib.Models.Email;
using SOS.Lib.Models.Search;
using SOS.Lib.Services.Interfaces;

namespace SOS.Export.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class DOIJob : IDOIJob
    {
        private readonly ISightingFactory _sightingFactory;
        private readonly IEmailService _emailService;
        private readonly ILogger<DOIJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sightingFactory"></param>
        /// <param name="emailService"></param>
        /// <param name="logger"></param>
        public DOIJob(ISightingFactory sightingFactory, IEmailService emailService, ILogger<DOIJob> logger)
        {
            _sightingFactory = sightingFactory ?? throw new ArgumentNullException(nameof(sightingFactory));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(AdvancedFilter filter, string fileName, IJobCancellationToken cancellationToken)
        {
            try
            {
                _emailService.Send(new EmailMessage{ Content = "Hej hopp i lingon...", Subject = "Hello world", To = new []{ "mats.lindgren@slu.se" }});

                _logger.LogDebug("Start DOI job");
                var success = await _sightingFactory.CreateDOIAsync(filter, fileName, cancellationToken);
                _logger.LogDebug($"End DOI job. Success: {success}");
                return success;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("DOI job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DOI job failed");
                return false;
            }
        }
    }
}
