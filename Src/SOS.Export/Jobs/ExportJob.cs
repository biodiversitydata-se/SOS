using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Factories.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.Email;
using SOS.Lib.Models.Search;
using SOS.Lib.Services.Interfaces;

namespace SOS.Export.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class ExportJob : IExportJob
    {
        private readonly ISightingFactory _sightingFactory;
        private readonly IEmailService _emailService;
        private readonly ILogger<ExportJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sightingFactory"></param>
        /// <param name="emailService"></param>
        /// <param name="logger"></param>
        public ExportJob(ISightingFactory sightingFactory, IEmailService emailService, ILogger<ExportJob> logger)
        {
            _sightingFactory = sightingFactory ?? throw new ArgumentNullException(nameof(sightingFactory));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(ExportFilter filter, string email, IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Start export job");
                var fileName = await _sightingFactory.ExportDWCAsync(filter, cancellationToken);
                var success = !string.IsNullOrEmpty(fileName);

                if (success && !string.IsNullOrEmpty(email))
                {
             //       _emailService.Send(new EmailMessage { Content = "Hej,</br>Din fil är nu skapad. Klicka på länken nedan för att hämta den...", Subject = "Exportfil skapad", To = new[] { email } });
                }

                _logger.LogDebug($"End DOI job. Success: {success}");
                
                return success ? true : throw new Exception("Export Job failed");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Export job failed");
                throw new Exception("Export Job failed");
            }
        }
    }
}
