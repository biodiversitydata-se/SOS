using Elastic.Clients.Elasticsearch;
using Hangfire;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Models;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace SOS.Lib.IO.GeoJson
{
    public class GeoJsonFileWriter : FileWriterBase, IGeoJsonFileWriter
    {
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IFileService _fileService;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly IGeneralizationResolver _generalizationResolver;
        private readonly ILogger<GeoJsonFileWriter> _logger;
        private const int FastSearchLimit = 10000;

        private void EnsureCoordinatesAreRetrievedFromDb(OutputFilter outputFilter)
        {
            if (outputFilter?.Fields == null) return;
            if (outputFilter.Fields.Any(f => f.Equals("location", StringComparison.CurrentCultureIgnoreCase))) return;
            if (!outputFilter.Fields.Any(f => f.Equals("location.decimallatitude", StringComparison.CurrentCultureIgnoreCase)))
            {
                outputFilter.Fields.Add("location.decimalLatitude");
            }

            if (!outputFilter.Fields.Any(f => f.Equals("location.decimallongitude", StringComparison.CurrentCultureIgnoreCase)))
            {
                outputFilter.Fields.Add("location.decimalLongitude");
            }
        }

        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            var geoJsonConverterFactory = new NetTopologySuite.IO.Converters.GeoJsonConverterFactory();
            var attributesTableConverter = geoJsonConverterFactory.CreateConverter(typeof(AttributesTable), null);
            var geometryConverter = geoJsonConverterFactory.CreateConverter(typeof(Geometry), null);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            jsonSerializerOptions.Converters.Add(attributesTableConverter);
            jsonSerializerOptions.Converters.Add(geometryConverter);
            return jsonSerializerOptions;
        }

        private List<Observation> CastDynamicsToObservations(IEnumerable<dynamic> dynamicObjects)
        {
            if (dynamicObjects == null) return null;
            return System.Text.Json.JsonSerializer.Deserialize<List<Observation>>(System.Text.Json.JsonSerializer.Serialize(dynamicObjects),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private AttributesTable GetFeatureAttributesTable(
            FlatObservation flatObservation,
            IEnumerable<PropertyFieldDescription> propertyFields,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues)
        {
            var attributesTable = new AttributesTable();
            foreach (var propertyField in propertyFields)
            {
                var value = flatObservation.GetValue(propertyField);
                if (excludeNullValues && (value == null || string.IsNullOrEmpty(value.ToString()))) continue;
                var label = ObservationPropertyFieldDescriptionHelper.GetPropertyLabel(propertyField, propertyLabelType);
                attributesTable.Add(label, value);
            }

            return attributesTable;
        }

        private AttributesTable GetFeatureAttributesTable(IDictionary<string, object> record, ICollection<string> outputFields)
        {
            AttributesTable attributesTable = null;
            if (outputFields != null && outputFields.Any())
            {
                attributesTable = new AttributesTable();
                foreach (var pair in record.Where(m => outputFields.Contains(m.Key, StringComparer.InvariantCultureIgnoreCase)))
                {
                    attributesTable.Add(pair.Key, pair.Value);
                }
            }
            else
            {
                attributesTable = new AttributesTable(record);
            }

            return attributesTable;
        }

        private Geometry GetFeatureGeometryFromDictionary(IDictionary<string, object> record)
        {
            double? decimalLatitude = null;
            double? decimalLongitude = null;
            if (record.TryGetValue(nameof(Observation.Location).ToLower(), out var locationObject))
            {
                var locationDictionary = locationObject as IDictionary<string, object>;
                if (locationDictionary == null) return null;
                if (locationDictionary.TryGetValue("decimalLatitude", out var decimalLatitudeObject))
                {
                    decimalLatitude = decimalLatitudeObject as double?;
                }
                if (locationDictionary.TryGetValue("decimalLongitude", out var decimalLongitudeObject))
                {
                    decimalLongitude = decimalLongitudeObject as double?;
                }
            }

            if (decimalLatitude == null || decimalLongitude == null) return null;
            Geometry geometry = new Point(decimalLongitude.Value, decimalLatitude.Value);
            return geometry;
        }

        private string GetOccurrenceIdFromDictionary(IDictionary<string, object> record)
        {
            string occurrenceId = null;
            if (record.TryGetValue(nameof(Observation.Occurrence).ToLower(), out var occurrenceObject))
            {
                var occurrenceDictionary = occurrenceObject as IDictionary<string, object>;
                if (occurrenceDictionary == null) return null;
                if (occurrenceDictionary.TryGetValue("occurrenceId", out var occurrenceIdObject))
                {
                    occurrenceId = occurrenceIdObject.ToString();
                }
            }

            return occurrenceId;
        }

        private Geometry GetFeatureGeometry(FlatObservation flatObservation)
        {
            double? decimalLatitude = flatObservation.LocationDecimalLatitude;
            double? decimalLongitude = flatObservation.LocationDecimalLongitude;
            if (decimalLatitude == null || decimalLongitude == null) return null;
            Geometry geometry = new Point(decimalLongitude.Value, decimalLatitude.Value);
            return geometry;
        }

        private Geometry GetFeatureGeometry(IDictionary<string, object> record)
        {
            double? decimalLatitude = null;
            double? decimalLongitude = null;

            if (record.TryGetValue("Location.DecimalLatitude", out var decimalLatitudeObject))
            {
                decimalLatitude = (double)decimalLatitudeObject;
            }

            if (record.TryGetValue("Location.DecimalLongitude", out var decimalLongitudeObject))
            {
                decimalLongitude = (double)decimalLongitudeObject;
            }

            if (decimalLatitude == null || decimalLongitude == null) return null;
            Geometry geometry = new Point(decimalLongitude.Value, decimalLatitude.Value);
            return geometry;
        }

        private async Task WriteFeature(
            IEnumerable<PropertyFieldDescription> propertyFields,
            IDictionary<string, object> record,
            bool excludeNullValues,
            Utf8JsonWriter jsonWriter,
            JsonSerializerOptions jsonSerializerOptions)
        {
            Geometry geometry = GetFeatureGeometryFromDictionary(record);
            if (geometry == null) return;
            AttributesTable attributesTable = new AttributesTable(record);
            string id = GetOccurrenceIdFromDictionary(record);
            await WriteFeature(geometry, attributesTable, id, jsonWriter, jsonSerializerOptions);
        }

        private async Task WriteFeature(
            IEnumerable<PropertyFieldDescription> propertyFields,
            FlatObservation flatObservation,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            Utf8JsonWriter utf8JsonWriter,
            JsonSerializerOptions jsonSerializerOptions)
        {
            Geometry geometry = GetFeatureGeometry(flatObservation);
            if (geometry == null) return;
            AttributesTable attributesTable = GetFeatureAttributesTable(flatObservation, propertyFields, propertyLabelType, excludeNullValues);
            string id = flatObservation.OccurrenceId;
            await WriteFeature(geometry, attributesTable, id, utf8JsonWriter, jsonSerializerOptions);
        }

        private async Task WriteFeature(
            Geometry geometry,
            AttributesTable attributesTable,
            string id,
            Utf8JsonWriter jsonWriter,
            JsonSerializerOptions jsonSerializerOptions)
        {
            await Task.Run(() =>
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("type", "Feature");
                if (!string.IsNullOrEmpty(id))
                {
                    jsonWriter.WriteString("id", id);
                }
                jsonWriter.WritePropertyName("geometry");

                JsonSerializer.Serialize(jsonWriter, geometry, jsonSerializerOptions);
                jsonWriter.WritePropertyName("properties");
                JsonSerializer.Serialize(jsonWriter, attributesTable, jsonSerializerOptions);
                jsonWriter.WriteEndObject();
            });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="generalizationResolver"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public GeoJsonFileWriter(IProcessedObservationCoreRepository processedObservationRepository,
            IFileService fileService,
            IVocabularyValueResolver vocabularyValueResolver,
            IGeneralizationResolver generalizationResolver,
            ILogger<GeoJsonFileWriter> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                                    throw new ArgumentNullException(
                                                        nameof(processedObservationRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _vocabularyValueResolver = vocabularyValueResolver ??
                                       throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _generalizationResolver = generalizationResolver ?? throw new ArgumentNullException(nameof(generalizationResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<FileExportResult> CreateFileAync(SearchFilter filter,
            string exportPath,
            string fileName,
            string culture,
            bool flatOut,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            bool gzip,
            IJobCancellationToken cancellationToken)
        {            
            var temporaryZipExportFolderPath = Path.Combine(exportPath, "zip");

            try
            {                
                int expectedNoOfObservations = (int)await _processedObservationRepository.GetMatchCountAsync(filter);
                bool useFastSearch = expectedNoOfObservations <= FastSearchLimit;
                _fileService.CreateDirectory(temporaryZipExportFolderPath);
                var observationsFilePath = Path.Combine(temporaryZipExportFolderPath, $"{fileName}.geojson");
                await using var fileStream = File.Create(observationsFilePath, 1048576);
                int nrObservations = await WriteGeoJsonFile(filter, culture, flatOut, propertyLabelType, excludeNullValues, expectedNoOfObservations, useFastSearch, fileStream, cancellationToken);

                if (gzip)
                {
                    await StoreFilterAsync(temporaryZipExportFolderPath, filter);
                    var zipFilePath = Path.Join(exportPath, $"{fileName}.zip");
                    _fileService.CompressDirectory(temporaryZipExportFolderPath, zipFilePath);
                    return new FileExportResult
                    {
                        FilePath = zipFilePath,
                        NrObservations = nrObservations
                    };
                }
                else
                {
                    var geojsonFilePath = Path.Combine(exportPath, $"{fileName}.geojson");
                    _fileService.MoveFile(observationsFilePath, geojsonFilePath);
                    return new FileExportResult
                    {
                        FilePath = geojsonFilePath,
                        NrObservations = nrObservations
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create GeoJson File.");
                throw;
            }
            finally
            {
                _fileService.DeleteDirectory(temporaryZipExportFolderPath);
            }
        }

        public async Task<(Stream stream, string filename)> CreateFileInMemoryAsZipStreamAsync(
           SearchFilter filter,
           string culture,
           bool flatOut,
           PropertyLabelType propertyLabelType,
           bool excludeNullValues,
           IJobCancellationToken cancellationToken)
        {            
            var memoryStream = new MemoryStream();

            try
            {                
                int expectedNoOfObservations = (int)await _processedObservationRepository.GetMatchCountAsync(filter);
                bool useFastSearch = expectedNoOfObservations <= FastSearchLimit;
                string fileName = $"Observations {DateTime.Now.ToString("yyyy-MM-dd HH.mm")} SOS export";
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var geoJsonFileEntry = zipArchive.CreateEntry($"{fileName}.geojson", useFastSearch ? CompressionLevel.Fastest : CompressionLevel.Optimal);
                    using (var geoJsonFileZipStream = geoJsonFileEntry.Open())
                    {
                        int nrObservations = await WriteGeoJsonFile(filter, culture, flatOut, propertyLabelType, excludeNullValues, expectedNoOfObservations, useFastSearch, geoJsonFileZipStream, cancellationToken);                        
                    }

                    var jsonEntry = zipArchive.CreateEntry("filter.json");
                    using (var filterFileZipStream = jsonEntry.Open())
                    using (var writer = new StreamWriter(filterFileZipStream, Encoding.UTF8))
                    {
                        writer.Write(filter.GetFilterAsJson());
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return (memoryStream, fileName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create GeoJSON file.");
                if (memoryStream != null) memoryStream.Dispose();
                throw;
            }
        }

        private async Task<int> WriteGeoJsonFile(
            SearchFilter filter, 
            string culture, 
            bool flatOut, 
            PropertyLabelType propertyLabelType, 
            bool excludeNullValues,
            int expectedNoOfObservations,
            bool useFastSearch,
            Stream stream, 
            IJobCancellationToken cancellationToken)
        {            
            var nrObservations = 0;
            var propertyFields =
                ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter.Output?.Fields, true);
            EnsureCoordinatesAreRetrievedFromDb(filter.Output);
            JsonSerializerOptions jsonSerializerOptions = CreateJsonSerializerOptions();

            var jsonWriterOptions = new JsonWriterOptions()
            {
                //To be able to display å,ä,ö e.t.c. properly we need to add the range Latin1Supplement to the list of characters which should not be displayed as UTF8 encoded values (\uxxxx).
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement)
            };
            await using var jsonWriter = new Utf8JsonWriter(stream, jsonWriterOptions);
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("type", "FeatureCollection");
            jsonWriter.WriteString("crs", "EPSG:4326");
            jsonWriter.WritePropertyName("features");
            jsonWriter.WriteStartArray();

            PagedResult<dynamic> fastSearchResult = null;
            SearchAfterResult<dynamic, IReadOnlyCollection<FieldValue>> searchResult = null;
            if (useFastSearch)
            {
                fastSearchResult = await _processedObservationRepository.GetChunkAsync(filter, 0, 10000);
            }
            else
            {
                searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<dynamic>(filter);
            }

            while ((useFastSearch && (fastSearchResult?.Records?.Any() ?? false)) || (!useFastSearch && (searchResult?.Records?.Any() ?? false)))
            {
                cancellationToken?.ThrowIfCancellationRequested();

                if (flatOut)
                {
                    Observation[] processedObservations = null;

                    if (useFastSearch)
                    {
                        processedObservations = fastSearchResult.Records.ToObservationsArray();          
                    }
                    else
                    {
                        processedObservations = searchResult.Records.ToObservationsArray();
                    }
                    
                    nrObservations += processedObservations.Length;
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture, true);
                    await _generalizationResolver.ResolveGeneralizedObservationsAsync(filter, processedObservations);

                    foreach (var observation in processedObservations)
                    {
                        var flatObservation = new FlatObservation(observation);
                        await WriteFeature(propertyFields, flatObservation, propertyLabelType, excludeNullValues, jsonWriter, jsonSerializerOptions);
                    }

                    processedObservations = null;
                }
                else
                {
                    IEnumerable<IDictionary<string, object>> processedRecords;
                    if (useFastSearch)
                    {
                        processedRecords = fastSearchResult.Records.Cast<IDictionary<string, object>>();
                    }
                    else
                    {
                        processedRecords = searchResult.Records.Cast<IDictionary<string, object>>();
                    }

                    nrObservations += processedRecords.Count();
                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedRecords, culture, true);
                    await _generalizationResolver.ResolveGeneralizedObservationsAsync(filter, processedRecords);
                    LocalDateTimeConverterHelper.ConvertToLocalTime(processedRecords);
                    foreach (var record in processedRecords)
                    {
                        await WriteFeature(propertyFields, record, excludeNullValues, jsonWriter, jsonSerializerOptions);
                    }
                }

                if (useFastSearch) break;

                // Get next batch of observations.        
                searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<dynamic>(filter, searchResult.PointInTimeId, searchResult.SearchAfter == null ? null : [searchResult.SearchAfter.ToFieldValue()]);           
            }

            searchResult = null;
            fastSearchResult = null;
            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndObject();
            await jsonWriter.FlushAsync();
            await jsonWriter.DisposeAsync();
            stream.Close();

            // If less tha 99% of expected observations where fetched, something is wrong
            if (nrObservations < expectedNoOfObservations * 0.99)
            {
                throw new Exception($"GeoJSON export expected {expectedNoOfObservations} but only got {nrObservations}");
            }

            return nrObservations;
        }
    }
}