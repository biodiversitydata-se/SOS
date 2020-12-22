using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Jobs.Import;
using Newtonsoft.Json;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Json;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Create data validation report job for a specific data provider.
    /// </summary>
    public class DataValidationReportJob : IDataValidationReportJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IDataValidationReportManager _dataValidationReportManager;
        private readonly DwcaConfiguration _dwcaConfiguration;

        public DataValidationReportJob(IDataProviderManager dataProviderManager, IDataValidationReportManager dataValidationReportManager, DwcaConfiguration dwcaConfiguration)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _dataValidationReportManager = dataValidationReportManager ?? throw new ArgumentNullException(nameof(dataValidationReportManager));
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
        }

        public async Task<string> RunAsync(
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

            string compactSavePath = Path.Combine(_dwcaConfiguration.ImportPath, $"Compact-DataValidationReport-{dataProvider.Name}.json");
            string verboseSavePath = Path.Combine(_dwcaConfiguration.ImportPath, $"Verbose-DataValidationReport-{dataProvider.Name}.json");

            // Serialize and save compact JSON file
            var compactJsonSettings = CreateCompactJsonSerializerSettings();
            var compactJson = JsonConvert.SerializeObject(dataValidationSummary, Formatting.Indented, compactJsonSettings);
            await File.WriteAllTextAsync(compactSavePath, compactJson);

            // Serialize and save verbose JSON file
            var verboseJsonSettings = CreateVerboseJsonSerializerSettings();
            var verboseJson = JsonConvert.SerializeObject(dataValidationSummary, Formatting.Indented, verboseJsonSettings);
            await File.WriteAllTextAsync(verboseSavePath, verboseJson);

            return compactSavePath;
        }

        private JsonSerializerSettings CreateCompactJsonSerializerSettings()
        {
            // Exclude some properties.
            var jsonResolver = new IgnorableSerializerContractResolver { SetStringPropertyDefaultsToEmptyString = true }
                .Ignore<Observation>(obs => obs.Location.Point)
                .Ignore<Observation>(obs => obs.Location.PointWithBuffer)
                .Ignore<Observation>(obs => obs.IsInEconomicZoneOfSweden)
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