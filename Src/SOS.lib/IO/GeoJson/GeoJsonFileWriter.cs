﻿using Hangfire;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Models;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly ILogger<GeoJsonFileWriter> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public GeoJsonFileWriter(IProcessedObservationCoreRepository processedObservationRepository,
            IFileService fileService,
            IVocabularyValueResolver vocabularyValueResolver,
            ILogger<GeoJsonFileWriter> logger)
        {
            _processedObservationRepository = processedObservationRepository ??
                                                    throw new ArgumentNullException(
                                                        nameof(processedObservationRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _vocabularyValueResolver = vocabularyValueResolver ??
                                       throw new ArgumentNullException(nameof(vocabularyValueResolver));
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
            string temporaryZipExportFolderPath = null;

            try
            {
                var nrObservations = 0;
                var expectedNoOfObservations = await _processedObservationRepository.GetMatchCountAsync(filter);
                var propertyFields =
                    ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter.Output?.Fields);
                EnsureCoordinatesAreRetrievedFromDb(filter.Output);
                JsonSerializerOptions jsonSerializerOptions = CreateJsonSerializerOptions();
                temporaryZipExportFolderPath = Path.Combine(exportPath, fileName);
                if (!Directory.Exists(temporaryZipExportFolderPath))
                {
                    Directory.CreateDirectory(temporaryZipExportFolderPath);
                }
                var observationsFilePath = Path.Combine(temporaryZipExportFolderPath, "Observations.geojson");
                await using var fileStream = File.Create(observationsFilePath, 1048576);
                var jsonWriterOptions = new JsonWriterOptions()
                {
                    //To be able to display å,ä,ö e.t.c. properly we need to add the range Latin1Supplement to the list of characters which should not be displayed as UTF8 encoded values (\uxxxx).
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement)
                };

                await using var jsonWriter = new Utf8JsonWriter(fileStream, jsonWriterOptions);
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("type", "FeatureCollection");
                jsonWriter.WriteString("crs", "EPSG:4326");
                jsonWriter.WritePropertyName("features");
                jsonWriter.WriteStartArray();

                var searchAfterResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<dynamic>(filter);

                while (searchAfterResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    nrObservations += searchAfterResult.Records.Count();
                    // Start fetching next batch of observations.
                    var searchAfterResultTask = _processedObservationRepository.GetObservationsBySearchAfterAsync<dynamic>(filter, searchAfterResult.PointInTimeId, searchAfterResult.SearchAfter);

                    if (flatOut)
                    {
                        var processedObservations = CastDynamicsToObservations(searchAfterResult.Records);

                        _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture, true);

                        foreach (var observation in processedObservations)
                        {
                            var flatObservation = new FlatObservation(observation);
                            await WriteFeature(propertyFields, flatObservation, propertyLabelType, excludeNullValues, jsonWriter, jsonSerializerOptions);
                        }
                    }
                    else
                    {
                        var processedRecords = searchAfterResult.Records.Cast<IDictionary<string, object>>();

                        _vocabularyValueResolver.ResolveVocabularyMappedValues(processedRecords, culture, true);

                        LocalDateTimeConverterHelper.ConvertToLocalTime(processedRecords);
                        foreach (var record in processedRecords)
                        {
                            await WriteFeature(propertyFields, record, excludeNullValues, jsonWriter, jsonSerializerOptions);
                        }
                    }

                    // Get next batch of observations.
                    searchAfterResult = await searchAfterResultTask;
                }

                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                await jsonWriter.FlushAsync();
                await jsonWriter.DisposeAsync();
                fileStream.Close();

                // If less tha 99% of expected observations where fetched, something is wrong
                if (nrObservations < expectedNoOfObservations * 0.99)
                {
                    throw new Exception($"Csv export expected {expectedNoOfObservations} but only got {nrObservations}");
                }

                if (gzip)
                {
                    await StoreFilterAsync(temporaryZipExportFolderPath, filter);
                    var zipFilePath = _fileService.CompressFolder(exportPath, fileName);
                    return new FileExportResult
                    {
                        FilePath = zipFilePath,
                        NrObservations = nrObservations
                    };
                }
                else
                {
                    var destinationFilePath = Path.Combine(exportPath, $"{fileName}.geojson");
                    File.Move(observationsFilePath, destinationFilePath);
                    return new FileExportResult
                    {
                        FilePath = destinationFilePath,
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
                _fileService.DeleteFolder(temporaryZipExportFolderPath);
            }
        }

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
            Observation observation,
            bool excludeNullValues,
            Utf8JsonWriter jsonWriter,
            JsonSerializerOptions jsonSerializerOptions)
        {
            Geometry geometry = GetFeatureGeometry(observation);
            if (geometry == null) return;
            AttributesTable attributesTable = GetFeatureAttributesTable(observation, propertyFields, excludeNullValues);
            string id = observation?.Occurrence?.OccurrenceId;
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
            IDictionary<string, object> record,
            ICollection<string> outputFields,
            Utf8JsonWriter utf8JsonWriter,
            JsonSerializerOptions jsonSerializerOptions)
        {
            Geometry geometry = GetFeatureGeometry(record);
            if (geometry == null) return;
            AttributesTable attributesTable = GetFeatureAttributesTable(record, outputFields);
            string id = null;
            if (record.TryGetValue("Occurrence.OccurrenceId", out object occurrenceId))
            {
                id = occurrenceId.ToString();
            }

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

        private Feature GetFeature(IDictionary<string, object> record, ICollection<string> outputFields) //, bool flattenProperties)
        {
            Geometry geometry = GetFeatureGeometry(record);
            if (geometry == null) return null;
            AttributesTable attributesTable = GetFeatureAttributesTable(record, outputFields);
            var feature = new Feature(geometry, attributesTable);
            return feature;
        }

        private AttributesTable GetFeatureAttributesTable(
            Observation observation,
            IEnumerable<PropertyFieldDescription> propertyFields,
            bool excludeNullValues)
        {
            var attributesTable = new AttributesTable();
            attributesTable.Add("Occurrence", observation.Occurrence);
            return attributesTable;
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

        private Geometry GetFeatureGeometry(Observation observation)
        {
            double? decimalLatitude = observation?.Location?.DecimalLatitude;
            double? decimalLongitude = observation?.Location?.DecimalLongitude;
            if (decimalLatitude == null || decimalLongitude == null) return null;
            Geometry geometry = new Point(decimalLongitude.Value, decimalLatitude.Value);
            return geometry;
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
    }
}