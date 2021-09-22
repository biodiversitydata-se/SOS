using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;

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
        public async Task<string> CreateFileAync(SearchFilter filter, string exportPath,
            string fileName, string culture, bool flatOut,
            IJobCancellationToken cancellationToken)
        {
            string temporaryZipExportFolderPath = null;

            try
            {
                temporaryZipExportFolderPath = Path.Combine(exportPath, fileName);
                if (!Directory.Exists(temporaryZipExportFolderPath))
                {
                    Directory.CreateDirectory(temporaryZipExportFolderPath);
                }
                await using var fileStream = File.Create(Path.Combine(temporaryZipExportFolderPath, "Observations.geojson"));
                await using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                //await streamWriter.WriteAsync("{\"type\":\"FeatureCollection\", \"crs\":\"EPSG:4326\", \"features\":[");
                await streamWriter.WriteAsync("{\"type\":\"FeatureCollection\", \"features\":[");

                var scrollResult = await _processedObservationRepository.ScrollObservationsAsync(filter, null);
                var geoJsonSerializer = GeoJsonSerializer.CreateDefault();
                var objectFlattenerHelper = new ObjectFlattenerHelper();
                while (scrollResult?.Records?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    // Fetch observations from ElasticSearch.
                    var processedObservations = scrollResult.Records.ToArray();

                    _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture, true);
                    var firstFeature = true;
                    
                    foreach (var observation in processedObservations)
                    {
                        var objectProperties = objectFlattenerHelper.Execute(observation);
                        if (!firstFeature) await streamWriter.WriteAsync(",");
                        await WriteFeature(streamWriter, geoJsonSerializer, objectProperties, filter?.OutputFields);
                        //var feature = GetFeature(objectProperties, filter?.OutputFields);
                        //geoJsonSerializer.Serialize(streamWriter, feature);
                        firstFeature = false;
                    }

                    // Get next batch of observations.
                    scrollResult = await _processedObservationRepository.ScrollObservationsAsync(filter, scrollResult.ScrollId);
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