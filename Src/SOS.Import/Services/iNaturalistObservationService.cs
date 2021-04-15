using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Services.Interfaces;

namespace SOS.Import.Services
{
    public class GBIFResult
    {
        public int Count { get; set; }
        public bool EndOfRecords { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public IEnumerable<DwcObservationVerbatim> Results { get; set; }
    }
    public class StringConverter : System.Text.Json.Serialization.JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            if (reader.TokenType == JsonTokenType.Number)
            {
                var utf8Bytes = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                var stringified =  Encoding.UTF8.GetString(utf8Bytes);
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

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="speciesObservationChangeServiceClient"></param>
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
            try
            {
                var observations = new List<DwcObservationVerbatim>();
                bool endOfChunk = false;
                int currentOffset = 0;
                int chunkSize = _iNaturalistServiceConfiguration.MaxReturnedChangesInOnePage;
                while (!endOfChunk) { 
                    var gbifChunk = await _httpClientService.GetFileStreamAsync(
                        new Uri($"{_iNaturalistServiceConfiguration.BaseAddress}/v1/occurrence/search?" +
                                $"country=SE" +
                                $"&datasetKey={_iNaturalistServiceConfiguration.DatasetKey}" +
                                $"&eventDate={ fromDate.ToString("yyyy-MM-dd") },{ toDate.ToString("yyyy-MM-dd") }" +
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
                        IgnoreNullValues = true,
                        PropertyNamingPolicy = null,
                        WriteIndented = true,
                        PropertyNameCaseInsensitive = true
                    };
                    serializerOptions.Converters.Add(new StringConverter());
                    var s = new StreamReader(gbifChunk);
                    var json = await s.ReadToEndAsync();
                    var results =   JsonSerializer.Deserialize<GBIFResult>(json, serializerOptions);
                    observations.AddRange(results.Results);
                    if(results.EndOfRecords)
                    {
                        endOfChunk = true;
                    }
                    currentOffset += chunkSize;
                }
                return observations;

            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get data from iNaturalist", e);
                return null;
            }
        }
    }
}