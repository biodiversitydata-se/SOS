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
using ProjNet.CoordinateSystems;
using SOS.Import.Factories.FieldMappings;
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
        private readonly GeoRegionFieldMappingFactory _geoRegionFieldMappingFactory;
        private readonly ActivityFieldMappingFactory _activityFieldMappingFactory;
        private readonly GenderFieldMappingFactory _genderFieldMappingFactory;
        private readonly LifeStageFieldMappingFactory _lifeStageFieldMappingFactory;
        private readonly BiotopeFieldMappingFactory _biotopeFieldMappingFactory;
        private readonly SubstrateFieldMappingFactory _substrateFieldMappingFactory;
        private readonly ValidationStatusFieldMappingFactory _validationStatusFieldMappingFactory;
        private readonly OrganizationFieldMappingFactory _organizationFieldMappingFactory;
        private readonly UnitFieldMappingFactory _unitFieldMappingFactory;
        private readonly BasisOfRecordFieldMappingFactory _basisOfRecordFieldMappingFactory;
        private readonly ContinentFieldMappingFactory _continentFieldMappingFactory;
        private readonly ILogger<FieldMappingFactory> _logger;
        private readonly Dictionary<FieldMappingFieldId, IFieldMappingCreatorFactory> _fieldMappingFactoryById;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldMappingRepository"></param>
        /// <param name="genderFieldMappingFactory"></param>
        /// <param name="lifeStageFieldMappingFactory"></param>
        /// <param name="validationStatusFieldMappingFactory"></param>
        /// <param name="organizationFieldMappingFactory"></param>
        /// <param name="unitFieldMappingFactory"></param>
        /// <param name="basisOfRecordFieldMappingFactory"></param>
        /// <param name="continentFieldMappingFactory"></param>
        /// <param name="logger"></param>
        /// <param name="geoRegionFieldMappingFactory"></param>
        /// <param name="activityFieldMappingFactory"></param>
        /// <param name="biotopeFieldMappingFactory"></param>
        /// <param name="substrateFieldMappingFactory"></param>
        public FieldMappingFactory(
            IFieldMappingRepository fieldMappingRepository,
            GeoRegionFieldMappingFactory geoRegionFieldMappingFactory,
            ActivityFieldMappingFactory activityFieldMappingFactory,
            GenderFieldMappingFactory genderFieldMappingFactory,
            LifeStageFieldMappingFactory lifeStageFieldMappingFactory,
            BiotopeFieldMappingFactory biotopeFieldMappingFactory,
            SubstrateFieldMappingFactory substrateFieldMappingFactory,
            ValidationStatusFieldMappingFactory validationStatusFieldMappingFactory,
            OrganizationFieldMappingFactory organizationFieldMappingFactory,
            UnitFieldMappingFactory unitFieldMappingFactory,
            BasisOfRecordFieldMappingFactory basisOfRecordFieldMappingFactory,
            ContinentFieldMappingFactory continentFieldMappingFactory,
            ILogger<FieldMappingFactory> logger)
        {
            _fieldMappingRepository = fieldMappingRepository ?? throw new ArgumentNullException(nameof(fieldMappingRepository));
            _geoRegionFieldMappingFactory = geoRegionFieldMappingFactory ?? throw new ArgumentNullException(nameof(geoRegionFieldMappingFactory));
            _activityFieldMappingFactory = activityFieldMappingFactory ?? throw new ArgumentNullException(nameof(activityFieldMappingFactory));
            _genderFieldMappingFactory = genderFieldMappingFactory ?? throw new ArgumentNullException(nameof(genderFieldMappingFactory));
            _lifeStageFieldMappingFactory = lifeStageFieldMappingFactory ?? throw new ArgumentNullException(nameof(lifeStageFieldMappingFactory));
            _biotopeFieldMappingFactory = biotopeFieldMappingFactory ?? throw new ArgumentNullException(nameof(biotopeFieldMappingFactory));
            _substrateFieldMappingFactory = substrateFieldMappingFactory ?? throw new ArgumentNullException(nameof(substrateFieldMappingFactory));
            _validationStatusFieldMappingFactory = validationStatusFieldMappingFactory ?? throw new ArgumentNullException(nameof(validationStatusFieldMappingFactory));
            _organizationFieldMappingFactory = organizationFieldMappingFactory ?? throw new ArgumentNullException(nameof(organizationFieldMappingFactory));
            _unitFieldMappingFactory = unitFieldMappingFactory ?? throw new ArgumentNullException(nameof(unitFieldMappingFactory));
            _basisOfRecordFieldMappingFactory = basisOfRecordFieldMappingFactory ?? throw new ArgumentNullException(nameof(basisOfRecordFieldMappingFactory));
            _continentFieldMappingFactory = continentFieldMappingFactory ?? throw new ArgumentNullException(nameof(continentFieldMappingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _fieldMappingFactoryById = new Dictionary<FieldMappingFieldId, IFieldMappingCreatorFactory>
            {
                {FieldMappingFieldId.LifeStage, _lifeStageFieldMappingFactory},
                {FieldMappingFieldId.Activity, _activityFieldMappingFactory},
                {FieldMappingFieldId.Gender, _genderFieldMappingFactory},
                {FieldMappingFieldId.Biotope, _biotopeFieldMappingFactory},
                {FieldMappingFieldId.Substrate, _substrateFieldMappingFactory},
                {FieldMappingFieldId.ValidationStatus, _validationStatusFieldMappingFactory},
                {FieldMappingFieldId.Organization, _organizationFieldMappingFactory},
                {FieldMappingFieldId.Unit, _unitFieldMappingFactory},
                {FieldMappingFieldId.BasisOfRecord, _basisOfRecordFieldMappingFactory},
                {FieldMappingFieldId.Continent, _continentFieldMappingFactory}
            };

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

        public async Task<IEnumerable<FieldMapping>> CreateAllFieldMappingsAsync(IEnumerable<FieldMappingFieldId> fieldMappingFieldIds)
        {
            List<FieldMapping> fieldMappings = new List<FieldMapping>();
            foreach (var fieldMappingFieldId in fieldMappingFieldIds)
            {
                var fieldMapping = await CreateFieldMappingAsync(fieldMappingFieldId);
                fieldMappings.Add(fieldMapping);
            }
            return fieldMappings;
        }

        /// <inheritdoc />
        public async Task<(string Filename, byte[] Bytes)> CreateFieldMappingFileAsync(FieldMappingFieldId fieldMappingFieldId)
        {
            string filename = $"{fieldMappingFieldId.ToString()}FieldMapping.json";
            FieldMapping fieldMapping = await CreateFieldMappingAsync(fieldMappingFieldId);
            return CreateFieldMappingFileResult(fieldMapping, filename);
        }

        private async Task<FieldMapping> CreateFieldMappingAsync(FieldMappingFieldId fieldMappingFieldId)
        {
            switch (fieldMappingFieldId)
            {
                case FieldMappingFieldId.Activity:
                case FieldMappingFieldId.LifeStage:
                case FieldMappingFieldId.Gender:
                case FieldMappingFieldId.Biotope:
                case FieldMappingFieldId.Substrate:
                case FieldMappingFieldId.ValidationStatus:
                case FieldMappingFieldId.Organization:
                case FieldMappingFieldId.Unit:
                case FieldMappingFieldId.BasisOfRecord:
                case FieldMappingFieldId.Continent:
                    var fieldMappingFactory = _fieldMappingFactoryById[fieldMappingFieldId];
                    return await fieldMappingFactory.CreateFieldMappingAsync();

                case FieldMappingFieldId.County:
                case FieldMappingFieldId.Municipality:
                case FieldMappingFieldId.Province:
                case FieldMappingFieldId.Parish:
                    var fieldMappingDictionary = await _geoRegionFieldMappingFactory.CreateFieldMappingsAsync();
                    return fieldMappingDictionary[fieldMappingFieldId];

                default:
                    throw new ArgumentException(
                        $"{MethodBase.GetCurrentMethod().Name}() does not support the value {fieldMappingFieldId}", nameof(fieldMappingFieldId));
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