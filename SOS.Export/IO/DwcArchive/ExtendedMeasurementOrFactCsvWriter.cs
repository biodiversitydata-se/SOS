using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Extensions;
using SOS.Export.Mappings;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Export.IO.DwcArchive
{
    public class ExtendedMeasurementOrFactCsvWriter : Interfaces.IExtendedMeasurementOrFactCsvWriter
    {
        private readonly ILogger<ExtendedMeasurementOrFactCsvWriter> _logger;

        public ExtendedMeasurementOrFactCsvWriter(ILogger<ExtendedMeasurementOrFactCsvWriter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> CreateCsvFileAsync(
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedDarwinCoreRepository processedDarwinCoreRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var skip = 0;
                const int take = 100000;
                var map = new ExtendedMeasurementOrFactRowMap();
                IEnumerable<DarwinCoreProject> projectParameters = await processedDarwinCoreRepository.GetProjectParameters(skip, take);

                while (projectParameters.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    IEnumerable<ExtendedMeasurementOrFactRow> records = projectParameters.ToExtendedMeasurementOrFactRows();
                    await WriteEmofCsvAsync(stream, records, map);
                    skip += take;
                    projectParameters = await processedDarwinCoreRepository.GetProjectParameters(skip, take);
                }

                return true;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{nameof(CreateCsvFileAsync)} was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create ExtendedMeasurementOrFact CSV file.");
                return false;
            }
        }

        private async Task WriteEmofCsvAsync<T>(Stream stream, IEnumerable<ExtendedMeasurementOrFactRow> records, ClassMap<T> map)
        {
            if (!records?.Any() ?? true)
            {
                return;
            }

            await using var streamWriter = new StreamWriter(stream, null, -1, false);
            var csvConfig = new Configuration
            {
                HasHeaderRecord = true,
                Delimiter = "\t", // tab
                Encoding = System.Text.Encoding.UTF8,
                CultureInfo = CultureInfo.InvariantCulture
            };
            using var csv = new CsvWriter(streamWriter, csvConfig);

            csv.Configuration.RegisterClassMap(map);
            csv.WriteRecords(records);
        }
    }
}
