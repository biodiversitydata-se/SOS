using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Models;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using Location = NetTopologySuite.Geometries.Location;

namespace SOS.Lib.IO.GeoJson
{
    public class GeoJsonFileWriter : FileWriterBase, IGeoJsonFileWriter
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
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
        public GeoJsonFileWriter(IProcessedObservationRepository processedObservationRepository,
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
        public async Task<string> CreateFileAync(SearchFilter filter, 
            string exportPath,
            string fileName, 
            string culture, 
            bool flatOut,
            OutputFieldSet outputFieldSet, 
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            IJobCancellationToken cancellationToken)
        {
            string temporaryZipExportFolderPath = null;

            try
            {
                var propertyFields = ObservationPropertyFieldDescriptionHelper.FieldsByFieldSet[outputFieldSet];
                temporaryZipExportFolderPath = Path.Combine(exportPath, fileName);
                if (!Directory.Exists(temporaryZipExportFolderPath))
                {
                    Directory.CreateDirectory(temporaryZipExportFolderPath);
                }
                await using var fileStream = File.Create(Path.Combine(temporaryZipExportFolderPath, "Observations.geojson"));
                await using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                await streamWriter.WriteAsync("{\"type\":\"FeatureCollection\", \"crs\":\"EPSG:4326\", \"features\":[");
                //await streamWriter.WriteAsync("{\"type\":\"FeatureCollection\", \"features\":[");

                var scrollResult = await _processedObservationRepository.ScrollObservationsAsDynamicAsync(filter, null);

                JsonSerializer geoJsonSerializer;
                if (excludeNullValues)
                {
                    JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
                        { NullValueHandling = NullValueHandling.Ignore };
                    geoJsonSerializer = GeoJsonSerializer.CreateDefault(jsonSerializerSettings);
                }
                else
                {
                    geoJsonSerializer = GeoJsonSerializer.CreateDefault();
                }
                    
                bool firstFeature = true;

                while (scrollResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    if (flatOut)
                    {
                        var processedObservations = CastDynamicsToObservations(scrollResult.Records);
                        _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture, true);
                        foreach (var observation in processedObservations)
                        {
                            
                            var flatObservation = new FlatObservation(observation);
                            if (!firstFeature) await streamWriter.WriteAsync(",");
                            await WriteFeature(streamWriter, geoJsonSerializer, propertyFields, flatObservation, propertyLabelType, excludeNullValues);
                            
                            firstFeature = false;
                        }
                    }
                    else
                    {
                        var processedRecords = scrollResult.Records.Cast<IDictionary<string, object>>();
                        // todo - implement vocabulary resolver for dynamic type observation.
                        foreach (var record in processedRecords)
                        {
                            if (!firstFeature) await streamWriter.WriteAsync(",");
                            await WriteFeature(streamWriter, geoJsonSerializer, propertyFields, record, excludeNullValues);
                        
                            firstFeature = false;
                        }
                    }
                    
                    // Get next batch of observations.
                    scrollResult = await _processedObservationRepository.ScrollObservationsAsDynamicAsync(filter, scrollResult.ScrollId);
                }
                await streamWriter.WriteAsync("] }");
                streamWriter.Close();
                fileStream.Close();
                await StoreFilterAsync(temporaryZipExportFolderPath, filter);
                var zipFilePath = _fileService.CompressFolder(exportPath, fileName);
                return zipFilePath;
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

        private List<Observation> CastDynamicsToObservations(IEnumerable<dynamic> dynamicObjects)
        {
            if (dynamicObjects == null) return null;
            return System.Text.Json.JsonSerializer.Deserialize<List<Observation>>(System.Text.Json.JsonSerializer.Serialize(dynamicObjects),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task WriteFeature(
            StreamWriter streamWriter,
            JsonSerializer geoJsonSerializer,
            List<PropertyFieldDescription> propertyFields,
            IDictionary<string, object> record,
            bool excludeNullValues)
        {
            Geometry geometry = GetFeatureGeometryFromDictionary(record);
            if (geometry == null) return;
            AttributesTable attributesTable = new AttributesTable(record);
            //AttributesTable attributesTable = GetFeatureAttributesTable(observation, propertyFields, excludeNullValues);
            string id = GetOccurrenceIdFromDictionary(record);
            await WriteFeature(streamWriter, geoJsonSerializer, geometry, attributesTable, id);
        }

        private async Task WriteFeature(
            StreamWriter streamWriter,
            JsonSerializer geoJsonSerializer,
            List<PropertyFieldDescription> propertyFields,
            Observation observation,
            bool excludeNullValues)
        {
            Geometry geometry = GetFeatureGeometry(observation);
            if (geometry == null) return;
            AttributesTable attributesTable = GetFeatureAttributesTable(observation, propertyFields, excludeNullValues);
            string id = observation?.Occurrence?.OccurrenceId;
            await WriteFeature(streamWriter, geoJsonSerializer, geometry, attributesTable, id);
        }

        private async Task WriteFeature(
            StreamWriter streamWriter, 
            JsonSerializer geoJsonSerializer,
            List<PropertyFieldDescription> propertyFields, 
            FlatObservation flatObservation,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues)
        {
            Geometry geometry = GetFeatureGeometry(flatObservation);
            if (geometry == null) return;
            AttributesTable attributesTable = GetFeatureAttributesTable(flatObservation, propertyFields, propertyLabelType, excludeNullValues);
            string id = flatObservation.OccurrenceId;
            await WriteFeature(streamWriter, geoJsonSerializer, geometry, attributesTable, id);
        }

        private async Task WriteFeature(StreamWriter streamWriter, JsonSerializer geoJsonSerializer,
            IDictionary<string, object> record, ICollection<string> outputFields)
        {
            Geometry geometry = GetFeatureGeometry(record);
            if (geometry == null) return;
            AttributesTable attributesTable = GetFeatureAttributesTable(record, outputFields);
            string id = null;
            if (record.TryGetValue("Occurrence.OccurrenceId", out object occurrenceId))
            {
                id = occurrenceId.ToString();
            }

            await WriteFeature(streamWriter, geoJsonSerializer, geometry, attributesTable, id);
        }

        private async Task WriteFeature(StreamWriter streamWriter, JsonSerializer geoJsonSerializer, Geometry geometry,
            AttributesTable attributesTable, string id)
        {
            if (id != null)
            {
                await streamWriter.WriteAsync($"{{ \"type\": \"Feature\", \"id\":\"{id}\", \"geometry\":");
            }
            else
            {
                await streamWriter.WriteAsync($"{{ \"type\": \"Feature\", \"geometry\":");
            }

            geoJsonSerializer.Serialize(streamWriter, geometry);
            await streamWriter.WriteAsync(", \"properties\":");
            geoJsonSerializer.Serialize(streamWriter, attributesTable);
            await streamWriter.WriteAsync("}");
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
            List<PropertyFieldDescription> propertyFields,
            bool excludeNullValues)
        {
            var attributesTable = new AttributesTable();
            attributesTable.Add("Occurrence", observation.Occurrence);
            return attributesTable;
            return null;
        }

        private AttributesTable GetFeatureAttributesTable(
            FlatObservation flatObservation, 
            List<PropertyFieldDescription> propertyFields, 
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