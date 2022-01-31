using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Json;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Create Dwc-A data validation report job.
    /// </summary>
    public class CreateDwcaDataValidationReportJob : ICreateDwcaDataValidationReportJob
    {
        private readonly IDwcaDataValidationReportManager _dwcaDataValidationReportManager;
        private readonly DwcaConfiguration _dwcaConfiguration;
        private readonly IReportManager _reportManager;
        private readonly ILogger<CreateDwcaDataValidationReportJob> _logger;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="dwcaDataValidationReportManager"></param>
        /// <param name="dwcaConfiguration"></param>
        /// <param name="reportManager"></param>
        /// <param name="logger"></param>
        public CreateDwcaDataValidationReportJob(
            IDwcaDataValidationReportManager dwcaDataValidationReportManager,
            DwcaConfiguration dwcaConfiguration,
            IReportManager reportManager,
            ILogger<CreateDwcaDataValidationReportJob> logger)
        {
            _dwcaDataValidationReportManager = dwcaDataValidationReportManager ?? throw new ArgumentNullException(nameof(dwcaDataValidationReportManager));
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
            _reportManager = reportManager ?? throw new ArgumentNullException(nameof(reportManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Report> RunAsync(
            string reportId, 
            string createdBy, 
            string archivePath,
            int maxNrObservationsToRead,
            int nrValidObservationsInReport,
            int nrInvalidObservationsInReport,
            int nrTaxaInTaxonStatistics,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start DwC-A Test Import Job");
                using var archiveReader = new ArchiveReader(archivePath, _dwcaConfiguration.ImportPath);
                var dataValidationSummary = await _dwcaDataValidationReportManager.CreateDataValidationSummary(
                    archiveReader,
                    maxNrObservationsToRead,
                    nrValidObservationsInReport,
                    nrInvalidObservationsInReport,
                    nrTaxaInTaxonStatistics);
                
                // Serialize and save compact JSON file
                var compactJsonSettings = CreateCompactJsonSerializerSettings();
                var compactJson = JsonConvert.SerializeObject(dataValidationSummary, Formatting.Indented, compactJsonSettings);
                var compactJsonFile = Encoding.UTF8.GetBytes(compactJson);

                // Serialize and save verbose JSON file
                var verboseJsonSettings = CreateVerboseJsonSerializerSettings();
                var verboseJson = JsonConvert.SerializeObject(dataValidationSummary, Formatting.Indented, verboseJsonSettings);
                var verboseJsonFile = Encoding.UTF8.GetBytes(verboseJson);

                var zipFile = ZipFileHelper.CreateZipFile(new[]
                {
                    (Filename: "Validation Report [Compact].json", Bytes: compactJsonFile),
                    (Filename: "Validation Report [Verbose].json", Bytes: verboseJsonFile)
                });

                var report = new Report(reportId)
                {
                    Type = ReportType.DataValidationReport,
                    Name = Path.GetFileNameWithoutExtension(archivePath),
                    FileExtension = "zip",
                    CreatedBy = createdBy ?? "",
                    FileSizeInKb = zipFile.Length / 1024
                };

                // Export to file system
                //string zipExportPath = Path.Combine(_dwcaConfiguration.ImportPath, $"{reportId}.zip");
                //await File.WriteAllBytesAsync(zipExportPath, zipFile);
                await _reportManager.AddReportAsync(report, zipFile);
                return report;
            }
            finally
            {
                if (File.Exists(archivePath)) File.Delete(archivePath);
            }
        }

        private JsonSerializerSettings CreateCompactJsonSerializerSettings()
        {
            // Exclude some properties.
            var jsonResolver = new IgnorableSerializerContractResolver { SetStringPropertyDefaultsToEmptyString = true }
                .Ignore<Observation>(obs => obs.Location.Point)
                .Ignore<Observation>(obs => obs.Location.PointWithBuffer)
                .Ignore<Observation>(obs => obs.Location.IsInEconomicZoneOfSweden)
                .Ignore<DwcObservationVerbatim>(obs => obs.RecordId)
                .Ignore<DwcObservationVerbatim>(obs => obs.Id)
                .Ignore<DwcObservationVerbatim>(obs => obs.DataProviderId)
                .Ignore<DwcObservationVerbatim>(obs => obs.DataProviderIdentifier)
                .KeepTypeWithDefaultValue(typeof(VocabularyValue));

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            return jsonSettings;
        }

        private JsonSerializerSettings CreateVerboseJsonSerializerSettings()
        {
            // Exclude some properties.
            var jsonResolver = new IgnorableSerializerContractResolver()
                .Ignore<Observation>(obs => obs.Location.Point)
                .Ignore<Observation>(obs => obs.Location.PointWithBuffer)
                .Ignore<Observation>(obs => obs.Location.IsInEconomicZoneOfSweden)
                .Ignore<DwcObservationVerbatim>(obs => obs.RecordId)
                .Ignore<DwcObservationVerbatim>(obs => obs.Id)
                .Ignore<DwcObservationVerbatim>(obs => obs.DataProviderId)
                .Ignore<DwcObservationVerbatim>(obs => obs.DataProviderIdentifier)
                .KeepTypeWithDefaultValue(typeof(VocabularyValue));

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            return jsonSettings;
        }
    }
}