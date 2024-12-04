﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Services.Interfaces;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace SOS.Harvest.Services
{
    public class GBIFResult
    {
        public int Count { get; set; }
        public bool EndOfRecords { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public IEnumerable<DwcObservationVerbatim>? Results { get; set; }
    }
    public class StringConverter : System.Text.Json.Serialization.JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            if (reader.TokenType == JsonTokenType.Number)
            {
                var utf8Bytes = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                var stringified = Encoding.UTF8.GetString(utf8Bytes);
                return stringified;
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }

            throw new System.Text.Json.JsonException();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

    }
    public class iNaturalistObservationService : IiNaturalistObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly iNaturalistServiceConfiguration _iNaturalistServiceConfiguration;
        private readonly ILogger<iNaturalistObservationService> _logger;

        private async Task<IEnumerable<DwcObservationVerbatim>> GetAsync(DateTime fromDate, DateTime toDate,
            byte attempt)
        {
            try
            {
                var observations = new List<DwcObservationVerbatim>();
                var endOfChunk = false;
                var currentOffset = 0;
                var chunkSize = _iNaturalistServiceConfiguration.MaxReturnedChangesInOnePage;
                while (!endOfChunk)
                {
                    var gbifChunk = await _httpClientService.GetFileStreamAsync(
                        new Uri($"{_iNaturalistServiceConfiguration.BaseAddress}/v1/occurrence/search?" +
                                $"country=SE" +
                                $"&datasetKey={_iNaturalistServiceConfiguration.DatasetKey}" +
                                $"&eventDate={fromDate.ToString("yyyy-MM-dd")},{toDate.ToString("yyyy-MM-dd")}" +
                                $"&offset=" + currentOffset +
                                $"&limit=" + chunkSize),
                        new Dictionary<string, string>(new[]
                            {
                                new KeyValuePair<string, string>("Accept", "application/json"),
                            }
                            )
                        );
                    var serializerOptions = new JsonSerializerOptions()
                    {
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = null,
                        WriteIndented = true,
                        PropertyNameCaseInsensitive = true
                    };
                    serializerOptions.Converters.Add(new StringConverter());
                    var s = new StreamReader(gbifChunk);
                    var json = await s.ReadToEndAsync();
                    var results = JsonSerializer.Deserialize<GBIFResult>(json, serializerOptions);

                    if (results?.Results == null)
                    {
                        endOfChunk = true;
                        continue;
                    }
                    observations.AddRange(results.Results);
                    if (results.EndOfRecords)
                    {
                        endOfChunk = true;
                    }
                    currentOffset += chunkSize;
                }
                return observations;

            }
            catch (Exception e)
            {
                if (attempt < 5)
                {
                    _logger.LogWarning($"Failed to get data from iNaturalist ({fromDate.ToString("yyyy-MM-dd")}-{toDate.ToString("yyyy-MM-dd")}), attempt: {attempt}", e);
                    Thread.Sleep(attempt * 5000);
                    return await GetAsync(fromDate, toDate, ++attempt);
                }

                _logger.LogError(e, "Failed to get data from {@dataProvider} " + $"({fromDate.ToString("yyyy-MM-dd")}-{toDate.ToString("yyyy-MM-dd")})", "iNaturalist");
                throw;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="iNaturalistServiceConfiguration"></param>
        /// <param name="logger"></param>
        public iNaturalistObservationService(
            IHttpClientService httpClientService,
            iNaturalistServiceConfiguration iNaturalistServiceConfiguration,
            ILogger<iNaturalistObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _iNaturalistServiceConfiguration = iNaturalistServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(iNaturalistServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DwcObservationVerbatim>> GetAsync(DateTime fromDate, DateTime toDate)
        {
            return await GetAsync(fromDate, toDate, 1);
        }
    }
}