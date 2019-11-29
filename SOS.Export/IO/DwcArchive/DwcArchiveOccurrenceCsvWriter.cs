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
using SOS.Export.Enums;
using SOS.Export.Helpers;
using SOS.Export.Mappings;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;

namespace SOS.Export.IO.DwcArchive
{
    public class DwcArchiveOccurrenceCsvWriter : Interfaces.IDwcArchiveOccurrenceCsvWriter
    {
        private readonly ILogger<DwcArchiveOccurrenceCsvWriter> _logger;

        public DwcArchiveOccurrenceCsvWriter(ILogger<DwcArchiveOccurrenceCsvWriter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> CreateOccurrenceCsvFileAsync(
            Stream stream,
            IEnumerable<FieldDescription> fieldDescriptions,
            IProcessedDarwinCoreRepository processedDarwinCoreRepository,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var skip = 0;
                const int take = 1000000;
                var darwinCoreMap = new DarwinCoreDynamicMap(fieldDescriptions);
                var processedDarwinCore = await processedDarwinCoreRepository.GetChunkAsync(skip, take);

                while (processedDarwinCore.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    var writeTasks = new[]
                    {
                        WriteOccurrenceCsvAsync(stream, processedDarwinCore, darwinCoreMap)
                    };

                    await Task.WhenAll(writeTasks);
                    skip += take;
                    processedDarwinCore = await processedDarwinCoreRepository.GetChunkAsync(skip, take);
                }

                return true;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation($"{nameof(CreateOccurrenceCsvFileAsync)} was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create occurrence CSV file.");
                return false;
            }
        }

        private async Task WriteOccurrenceCsvAsync<T>(Stream stream, IEnumerable<T> records, ClassMap<T> map)
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
