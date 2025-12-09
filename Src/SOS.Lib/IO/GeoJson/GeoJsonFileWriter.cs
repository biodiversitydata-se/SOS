using CSharpFunctionalExtensions;
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
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;

namespace SOS.Lib.IO.GeoJson;

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
            if (excludeNullValues && (value is null || IsNullOrEmptyValue(value))) continue;
            var label = ObservationPropertyFieldDescriptionHelper.GetPropertyLabel(propertyField, propertyLabelType);
            attributesTable.Add(label, value);
        }

        return attributesTable;
    }

    private static bool IsNullOrEmptyValue(object value)
    {
        return value switch
        {
            null => true,
            string s => string.IsNullOrEmpty(s),
            _ => false
        };
    }

    private static Geometry GetFeatureGeometryFromJsonNode(JsonNode record)
    {
        double? decimalLatitude = null;
        double? decimalLongitude = null;
        var locationObject = record["location"];
        if (locationObject == null)
        {
            return null;
        }
        decimalLatitude = (double?)locationObject["decimalLatitude"];
        decimalLongitude = (double?)locationObject["decimalLongitude"];
        if (decimalLatitude == null || decimalLongitude == null) return null;
        Geometry geometry = new Point(decimalLongitude.Value, decimalLatitude.Value);
        return geometry;
    }

    private static string GetOccurrenceIdFromJsonNode(JsonNode record)
    {
        string occurrenceId = null;

        var occurrenceObject = record["occurrence"];
        if (occurrenceObject != null)
        {
            occurrenceId = (string)occurrenceObject["occurrenceId"];
        }

        return occurrenceId;
    }

    private static Geometry GetFeatureGeometry(FlatObservation flatObservation)
    {
        double? decimalLatitude = flatObservation.LocationDecimalLatitude;
        double? decimalLongitude = flatObservation.LocationDecimalLongitude;
        if (decimalLatitude == null || decimalLongitude == null) return null;
        Geometry geometry = new Point(decimalLongitude.Value, decimalLatitude.Value);
        return geometry;
    }

    private static Geometry GetFeatureGeometry(Observation observation)
    {
        double? decimalLatitude = observation.Location.DecimalLatitude;
        double? decimalLongitude = observation.Location.DecimalLongitude;
        if (decimalLatitude == null || decimalLongitude == null) return null;
        Geometry geometry = new Point(decimalLongitude.Value, decimalLatitude.Value);
        return geometry;
    }

    private static async Task WriteFeature(
        IEnumerable<PropertyFieldDescription> propertyFields,
        JsonNode record,
        bool excludeNullValues,
        Utf8JsonWriter jsonWriter,
        JsonSerializerOptions jsonSerializerOptions)
    {
        Geometry geometry = GetFeatureGeometryFromJsonNode(record);
        if (geometry == null) return;
        
        string id = GetOccurrenceIdFromJsonNode(record);
        WriteFeature(geometry, record, excludeNullValues, id, jsonWriter, jsonSerializerOptions);
    }    

    private async Task WriteFeature(        
        FlatObservation flatObservation,
        Dictionary<PropertyFieldDescription, string> propertyLabels,
        bool excludeNullValues,
        Utf8JsonWriter utf8JsonWriter,
        JsonSerializerOptions jsonSerializerOptions)
    {
        Geometry geometry = GetFeatureGeometry(flatObservation);
        if (geometry == null) return;
        string id = flatObservation.OccurrenceId;
        WriteFeature(geometry, flatObservation, propertyLabels, excludeNullValues, id, utf8JsonWriter, jsonSerializerOptions);
    }

    private static void WriteFeature(
        Geometry geometry,
        AttributesTable attributesTable,
        string id,
        Utf8JsonWriter jsonWriter,
        JsonSerializerOptions jsonSerializerOptions)
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
    }

    private static void WriteFeature(
        Geometry geometry,
        JsonNode record,
        bool excludeNullValues,
        string id,
        Utf8JsonWriter jsonWriter,
        JsonSerializerOptions jsonSerializerOptions)
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
        WritePropertiesFromJsonNode(record, excludeNullValues, jsonWriter, jsonSerializerOptions);
        jsonWriter.WriteEndObject();
    }

    private static void WritePropertiesFromJsonNode(
        JsonNode record,
        bool excludeNullValues,
        Utf8JsonWriter jsonWriter,
        JsonSerializerOptions jsonSerializerOptions)
    {
        jsonWriter.WriteStartObject();        
        foreach (var kvp in record.AsObject())
        {
            JsonNode node = kvp.Value;
            if (excludeNullValues && node is null)
                continue;

            jsonWriter.WritePropertyName(kvp.Key);
            JsonSerializer.Serialize(jsonWriter, node, jsonSerializerOptions);
        }

        jsonWriter.WriteEndObject();
    }


    private static void WriteFeature(
        Geometry geometry,        
        FlatObservation flatObservation,
        Dictionary<PropertyFieldDescription, string> propertyLabels,
        bool excludeNullValues,
        string id,
        Utf8JsonWriter jsonWriter,
        JsonSerializerOptions jsonSerializerOptions)
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
        WriteProperties(flatObservation, propertyLabels, excludeNullValues, jsonWriter, jsonSerializerOptions);
        jsonWriter.WriteEndObject();
    }

    private static void WriteFeature(        
        Observation observation,
        Dictionary<PropertyFieldDescription, string> propertyLabels,
        bool excludeNullValues,
        string id,
        Utf8JsonWriter jsonWriter,
        JsonSerializerOptions jsonSerializerOptions)
    {
        var geometry = GetFeatureGeometry(observation);
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("type", "Feature");
        if (!string.IsNullOrEmpty(id))
        {
            jsonWriter.WriteString("id", id);
        }
        jsonWriter.WritePropertyName("geometry");

        JsonSerializer.Serialize(jsonWriter, geometry, jsonSerializerOptions);
        jsonWriter.WritePropertyName("properties");
        JsonSerializer.Serialize(jsonWriter, observation, jsonSerializerOptions);       
        jsonWriter.WriteEndObject();
    }

    private static void WriteProperties(        
        FlatObservation flatObservation,
        Dictionary<PropertyFieldDescription, string> propertyLabels,
        bool excludeNullValues,
        Utf8JsonWriter jsonWriter,
        JsonSerializerOptions jsonSerializerOptions)
    {        
        jsonWriter.WriteStartObject();

        foreach (var pair in propertyLabels)
        {
            var value = flatObservation.GetValue(pair.Key);
            if (excludeNullValues && (value == null || IsNullOrEmptyValue(value)))
                continue;            
            jsonWriter.WritePropertyName(pair.Value);            
            JsonSerializer.Serialize(jsonWriter, value, jsonSerializerOptions);
        }

        jsonWriter.WriteEndObject();
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
        Dictionary<PropertyFieldDescription, string> propertyLabels = propertyFields.ToDictionary(
            x => x,
            x => ObservationPropertyFieldDescriptionHelper.GetPropertyLabel(x, propertyLabelType));
        EnsureCoordinatesAreRetrievedFromDb(filter.Output);
        JsonSerializerOptions jsonSerializerOptions = CreateJsonSerializerOptions();

        var jsonWriterOptions = new JsonWriterOptions()
        {
            //To be able to display å,ä,ö e.t.c. properly we need to add the range Latin1Supplement to the list of characters which should not be displayed as UTF8 encoded values (\uxxxx).
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement),
            SkipValidation = true
        };
        await using var jsonWriter = new Utf8JsonWriter(stream, jsonWriterOptions);
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("type", "FeatureCollection");
        jsonWriter.WriteString("crs", "EPSG:4326");
        jsonWriter.WritePropertyName("features");
        jsonWriter.WriteStartArray();

        PagedResult<Observation> fastSearchResult = null;
        SearchAfterResult<Observation, ICollection<FieldValue>> searchResult = null;
        if (useFastSearch)
        {
            fastSearchResult = await _processedObservationRepository.GetChunkAsync<Observation>(filter, 0, 10000);
        }
        else
        {
            searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter);
        }

        while ((useFastSearch && (fastSearchResult?.Records?.Any() ?? false)) || (!useFastSearch && (searchResult?.Records?.Any() ?? false)))
        {
            cancellationToken?.ThrowIfCancellationRequested();

            if (flatOut)
            {
                var processedObservations = useFastSearch ? fastSearchResult.Records.ToObservationsArray() : searchResult.Records.ToObservationsArray();
                nrObservations += processedObservations.Length;
                _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, culture, true);
                await _generalizationResolver.ResolveGeneralizedObservationsAsync(filter, processedObservations);

                foreach (var observation in processedObservations)
                {
                    var flatObservation = new FlatObservation(observation);
                    await WriteFeature(flatObservation, propertyLabels, excludeNullValues, jsonWriter, jsonSerializerOptions);
                }

                processedObservations = null;
            }
            else
            {
                var processedRecords = (useFastSearch ? fastSearchResult.Records : searchResult.Records)
                    .Select(r => JsonSerializer.SerializeToNode(r, typeof(Observation), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }));
                
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
            searchResult = await _processedObservationRepository.GetObservationsBySearchAfterAsync<Observation>(filter, searchResult.PointInTimeId, searchResult.SearchAfter?.ToArray());           
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

    public async Task WriteGeoJsonFeatureCollection(
        IEnumerable<JsonObject> records,
        ICollection<string> outputFields,
        bool flatOut,
        PropertyLabelType propertyLabelType,
        bool excludeNullValues,
        Stream stream,
        LatLonBoundingBox? bbox,
        List<Feature> geographicAreas)
    {
        using var ms = new MemoryStream();
        List<PropertyFieldDescription> propertyFields =
            ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(outputFields, true);
        Dictionary<PropertyFieldDescription, string> propertyLabels = propertyFields.ToDictionary(
            x => x,
            x => ObservationPropertyFieldDescriptionHelper.GetPropertyLabel(x, propertyLabelType));
        JsonSerializerOptions jsonSerializerOptions = CreateJsonSerializerOptions();
        var jsonWriterOptions = new JsonWriterOptions()
        {
            //To be able to display å,ä,ö e.t.c. properly we need to add the range Latin1Supplement to the list of characters which should not be displayed as UTF8 encoded values (\uxxxx).
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement),
            SkipValidation = true
        };
        await using var jsonWriter = new Utf8JsonWriter(ms, jsonWriterOptions);
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("type", "FeatureCollection");
        jsonWriter.WriteString("crs", "EPSG:4326");
        if (bbox != null)
        {
            jsonWriter.WritePropertyName("bbox");
            jsonWriter.WriteStartArray();
            jsonWriter.WriteNumberValue(bbox.TopLeft.Longitude);
            jsonWriter.WriteNumberValue(bbox.BottomRight.Latitude);
            jsonWriter.WriteNumberValue(bbox.BottomRight.Longitude);
            jsonWriter.WriteNumberValue(bbox.TopLeft.Latitude);
            jsonWriter.WriteEndArray();
        }

        jsonWriter.WritePropertyName("features");
        jsonWriter.WriteStartArray();        

        if (geographicAreas != null && geographicAreas.Any())
        {
            foreach (var feature in geographicAreas)
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("type", "Feature");
                jsonWriter.WritePropertyName("geometry");
                JsonSerializer.Serialize(jsonWriter, feature.Geometry, jsonSerializerOptions);
                jsonWriter.WritePropertyName("properties");
                JsonSerializer.Serialize(jsonWriter, feature.Attributes, jsonSerializerOptions);
                jsonWriter.WriteEndObject();
            }
        }        

        if (flatOut)
        {
            var observations = records.ToObservationsArray();
            foreach (var observation in observations)
            {
                var flatObservation = new FlatObservation(observation);
                await WriteFeature(flatObservation, propertyLabels, excludeNullValues, jsonWriter, jsonSerializerOptions);
            }
        }
        else
        {
            foreach (var record in records)
            {
                await WriteFeature(propertyFields, record, excludeNullValues, jsonWriter, jsonSerializerOptions);
            }
        }

        jsonWriter.WriteEndArray();
        jsonWriter.WriteEndObject();
        await jsonWriter.FlushAsync();
        
        ms.Position = 0;
        await ms.CopyToAsync(stream);
    }
}