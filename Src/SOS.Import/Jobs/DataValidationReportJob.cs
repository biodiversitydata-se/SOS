using System;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Jobs.Import;
using Newtonsoft.Json;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Json;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Create data validation report job for a specific data provider.
    /// </summary>
    public class DataValidationReportJob : IDataValidationReportJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IDataValidationReportManager _dataValidationReportManager;
        private readonly IReportManager _reportManager;
        private readonly DwcaConfiguration _dwcaConfiguration;

        public DataValidationReportJob(
            IDataProviderManager dataProviderManager, 
            IDataValidationReportManager dataValidationReportManager, 
            IReportManager reportManager,
            DwcaConfiguration dwcaConfiguration)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _dataValidationReportManager = dataValidationReportManager ?? throw new ArgumentNullException(nameof(dataValidationReportManager));
            _reportManager = reportManager ?? throw new ArgumentNullException(nameof(reportManager));
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
        }

        public async Task<Report> RunAsync(
            string reportId,
            string createdBy,
            string dataProviderIdentifier,
            int maxNrObservationsToRead,
            int nrValidObservationsInReport,
            int nrInvalidObservationsInReport,
            IJobCancellationToken cancellationToken)
        {
            var dataProvider =
                await _dataProviderManager.GetDataProviderByIdentifier(dataProviderIdentifier);
            if (dataProvider == null) throw new Exception($"No data provider with Identifier={dataProviderIdentifier} found");

            var dataValidationSummary = await _dataValidationReportManager.CreateDataValidationReport(
                dataProvider,
                maxNrObservationsToRead,
                nrValidObservationsInReport,
                nrInvalidObservationsInReport);

            // Create compact JSON file
            var compactJsonSettings = CreateCompactJsonSerializerSettings();
            var compactJson = JsonConvert.SerializeObject(dataValidationSummary, Formatting.Indented, compactJsonSettings);
            var compactJsonFile = Encoding.UTF8.GetBytes(compactJson);
            
            // Create verbose JSON file
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
                Name = dataProvider.Names.Translate("en-GB"),
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

        private JsonSerializerSettings CreateCompactJsonSerializerSettings()
        {
            // Exclude some properties.
            var jsonResolver = new IgnorableSerializerContractResolver { SetStringPropertyDefaultsToEmptyString = true }
                .Ignore<Observation>(obs => obs.Location.Point)
                .Ignore<Observation>(obs => obs.Location.PointWithBuffer)
                .Ignore<Observation>(obs => obs.IsInEconomicZoneOfSweden)
                .Ignore<Observation>(obs => obs.Defects)
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
                .Ignore<Observation>(obs => obs.IsInEconomicZoneOfSweden)
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