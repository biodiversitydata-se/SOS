using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Import.Factories.FieldMappings.Interfaces;
using SOS.Import.Repositories.Destination.FieldMappings.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories
{
    /// <summary>
    /// Class for handling field mappings.
    /// </summary>
    public class FieldMappingFactory : Interfaces.IFieldMappingFactory { 

        private readonly IFieldMappingRepository _fieldMappingRepository;
        private readonly IGeoRegionFieldMappingFactory _geoRegionFieldMappingFactory;
        private readonly IActivityFieldMappingFactory _activityFieldMappingFactory;
        private readonly IGenderFieldMappingFactory _genderFieldMappingFactory;
        private readonly ILogger<FieldMappingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldMappingRepository"></param>
        /// <param name="genderFieldMappingFactory"></param>
        /// <param name="logger"></param>
        /// <param name="geoRegionFieldMappingFactory"></param>
        /// <param name="activityFieldMappingFactory"></param>
        public FieldMappingFactory(
            IFieldMappingRepository fieldMappingRepository,
            IGeoRegionFieldMappingFactory geoRegionFieldMappingFactory,
            IActivityFieldMappingFactory activityFieldMappingFactory,
            IGenderFieldMappingFactory genderFieldMappingFactory,
            ILogger<FieldMappingFactory> logger)
        {
            _fieldMappingRepository = fieldMappingRepository ?? throw new ArgumentNullException(nameof(fieldMappingRepository));
            _geoRegionFieldMappingFactory = geoRegionFieldMappingFactory ?? throw new ArgumentNullException(nameof(geoRegionFieldMappingFactory));
            _activityFieldMappingFactory = activityFieldMappingFactory ?? throw new ArgumentNullException(nameof(activityFieldMappingFactory));
            _genderFieldMappingFactory = genderFieldMappingFactory ?? throw new ArgumentNullException(nameof(genderFieldMappingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Import field mappings.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ImportAsync()
        {
            try
            {
                _logger.LogDebug("Start importing field mappings");
                var fieldMappings = new List<FieldMapping>();
                foreach (string fileName in Directory.GetFiles(@"Resources\FieldMappings\"))
                {
                    var fieldMapping = CreateFieldMappingFromJsonFile(fileName);
                    fieldMappings.Add(fieldMapping);
                }
                fieldMappings = fieldMappings.OrderBy(m => m.Id).ToList();
                
                await _fieldMappingRepository.DeleteCollectionAsync();
                await _fieldMappingRepository.AddCollectionAsync();
                await _fieldMappingRepository.AddManyAsync(fieldMappings);
                _logger.LogDebug("Finish storing field mappings");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed importing field mappings");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<byte[]> CreateFieldMappingsZipFileAsync(IEnumerable<FieldMappingFieldId> fieldMappingFieldIds)
        {
            var fielMappingFileTuples = new List<(string Filename, byte[] Bytes)>();
            foreach (var fieldMappingFieldId in fieldMappingFieldIds)
            {
                fielMappingFileTuples.Add(await CreateFieldMappingFileAsync(fieldMappingFieldId));
            }

            byte[] zipFile = CreateZipFile(fielMappingFileTuples);
            return zipFile;
        }

        /// <inheritdoc />
        public async Task<(string Filename, byte[] Bytes)> CreateFieldMappingFileAsync(FieldMappingFieldId fieldMappingFieldId)
        {
            Dictionary<FieldMappingFieldId, FieldMapping> fieldMappingDictionary;
            FieldMapping fieldMapping;
            switch (fieldMappingFieldId)
            {
                case FieldMappingFieldId.Activity:
                    fieldMapping = await _activityFieldMappingFactory.CreateFieldMappingAsync();
                    return CreateFieldMappingFileResult(fieldMapping, "ActivityFieldMapping.json");
                case FieldMappingFieldId.Gender:
                    fieldMapping = await _genderFieldMappingFactory.CreateFieldMappingAsync();
                    return CreateFieldMappingFileResult(fieldMapping, "GenderFieldMapping.json");
                case FieldMappingFieldId.County:
                    fieldMappingDictionary = await _geoRegionFieldMappingFactory.CreateFieldMappingsAsync();
                    return CreateFieldMappingFileResult(fieldMappingDictionary[FieldMappingFieldId.County], "CountyFieldMapping.json");
                case FieldMappingFieldId.Municipality:
                    fieldMappingDictionary = await _geoRegionFieldMappingFactory.CreateFieldMappingsAsync();
                    return CreateFieldMappingFileResult(fieldMappingDictionary[FieldMappingFieldId.Municipality], "MunicipalityFieldMapping.json");
                case FieldMappingFieldId.Province:
                    fieldMappingDictionary = await _geoRegionFieldMappingFactory.CreateFieldMappingsAsync();
                    return CreateFieldMappingFileResult(fieldMappingDictionary[FieldMappingFieldId.Province], "ProvinceFieldMapping.json");
                case FieldMappingFieldId.Parish:
                    fieldMappingDictionary = await _geoRegionFieldMappingFactory.CreateFieldMappingsAsync();
                    return CreateFieldMappingFileResult(fieldMappingDictionary[FieldMappingFieldId.Parish], "ParishFieldMapping.json");
                default:
                    throw new ArgumentException($"Argument {fieldMappingFieldId} is not supported");
            }
        }

        private FieldMapping CreateFieldMappingFromJsonFile(string filename)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, filename);
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var fieldMappings = JsonConvert.DeserializeObject<FieldMapping>(str);
            return fieldMappings;
        }

        private (string Filename, byte[] Bytes) CreateFieldMappingFileResult(FieldMapping fieldMapping, string fileName)
        {
            var bytes = SerializeToJsonArray(fieldMapping, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }, Formatting.Indented);
            return (Filename: fileName, Bytes: bytes);
        }

        private byte[] SerializeToJsonArray(object value, JsonSerializerSettings jsonSerializerSettings, Formatting formatting)
        {
            var result = JsonConvert.SerializeObject(value, formatting, jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(result);
        }

        private byte[] CreateZipFile(IEnumerable<(string Filename, byte[] Bytes)> files)
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var zipEntry = archive.CreateEntry(file.Filename, CompressionLevel.Optimal);
                    using var zipStream = zipEntry.Open();
                    zipStream.Write(file.Bytes, 0, file.Bytes.Length);
                }
            }

            return ms.ToArray();
        }
    }
}