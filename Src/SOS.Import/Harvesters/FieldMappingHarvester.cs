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
using SOS.Import.Factories.FieldMapping;
using SOS.Import.Factories.FieldMapping.Interfaces;
using SOS.Import.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Harvesters
{
    /// <summary>
    ///     Class for handling field mappings.
    /// </summary>
    public class FieldMappingHarvester : IFieldMappingHarvester
    {
        private readonly Dictionary<FieldMappingFieldId, IFieldMappingCreatorFactory> _fieldMappingFactoryById;

        private readonly IFieldMappingRepository _fieldMappingRepository;
        private readonly ILogger<FieldMappingHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="fieldMappingRepository"></param>
        /// <param name="genderFieldMappingFactory"></param>
        /// <param name="lifeStageFieldMappingFactory"></param>
        /// <param name="validationStatusFieldMappingFactory"></param>
        /// <param name="institutionFieldMappingFactory"></param>
        /// <param name="unitFieldMappingFactory"></param>
        /// <param name="basisOfRecordFieldMappingFactory"></param>
        /// <param name="continentFieldMappingFactory"></param>
        /// <param name="parishFieldMappingFactory"></param>
        /// <param name="establishmentMeansFieldMappingFactory"></param>
        /// <param name="determinationMethodFieldMappingFactory"></param>
        /// <param name="logger"></param>
        /// <param name="activityFieldMappingFactory"></param>
        /// <param name="biotopeFieldMappingFactory"></param>
        /// <param name="substrateFieldMappingFactory"></param>
        /// <param name="countyFieldMappingFactory"></param>
        /// <param name="municipalityFieldMappingFactory"></param>
        /// <param name="provinceFieldMappingFactory"></param>
        /// <param name="typeFieldMappingFactory"></param>
        /// <param name="countryFieldMappingFactory"></param>
        /// <param name="accessRightsFieldMappingFactory"></param>
        /// <param name="occurrenceStatusFieldMappingFactory"></param>
        public FieldMappingHarvester(
            IFieldMappingRepository fieldMappingRepository,
            ActivityFieldMappingFactory activityFieldMappingFactory,
            GenderFieldMappingFactory genderFieldMappingFactory,
            LifeStageFieldMappingFactory lifeStageFieldMappingFactory,
            BiotopeFieldMappingFactory biotopeFieldMappingFactory,
            SubstrateFieldMappingFactory substrateFieldMappingFactory,
            ValidationStatusFieldMappingFactory validationStatusFieldMappingFactory,
            InstitutionFieldMappingFactory institutionFieldMappingFactory,
            UnitFieldMappingFactory unitFieldMappingFactory,
            BasisOfRecordFieldMappingFactory basisOfRecordFieldMappingFactory,
            ContinentFieldMappingFactory continentFieldMappingFactory,
            CountyFieldMappingFactory countyFieldMappingFactory,
            MunicipalityFieldMappingFactory municipalityFieldMappingFactory,
            ProvinceFieldMappingFactory provinceFieldMappingFactory,
            ParishFieldMappingFactory parishFieldMappingFactory,
            TypeFieldMappingFactory typeFieldMappingFactory,
            CountryFieldMappingFactory countryFieldMappingFactory,
            AccessRightsFieldMappingFactory accessRightsFieldMappingFactory,
            OccurrenceStatusFieldMappingFactory occurrenceStatusFieldMappingFactory,
            EstablishmentMeansFieldMappingFactory establishmentMeansFieldMappingFactory,
            AreaTypeFieldMappingFactory areaTypeFieldMappingFactory,
            DiscoveryMethodFieldMappingFactory discoveryMethodFieldMappingFactory,
            DeterminationMethodFieldMappingFactory determinationMethodFieldMappingFactory,
            ILogger<FieldMappingHarvester> logger)
        {
            _fieldMappingRepository =
                fieldMappingRepository ?? throw new ArgumentNullException(nameof(fieldMappingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fieldMappingFactoryById = new Dictionary<FieldMappingFieldId, IFieldMappingCreatorFactory>
            {
                {FieldMappingFieldId.LifeStage, lifeStageFieldMappingFactory},
                {FieldMappingFieldId.Activity, activityFieldMappingFactory},
                {FieldMappingFieldId.Gender, genderFieldMappingFactory},
                {FieldMappingFieldId.Biotope, biotopeFieldMappingFactory},
                {FieldMappingFieldId.Substrate, substrateFieldMappingFactory},
                {FieldMappingFieldId.ValidationStatus, validationStatusFieldMappingFactory},
                {FieldMappingFieldId.Institution, institutionFieldMappingFactory},
                {FieldMappingFieldId.Unit, unitFieldMappingFactory},
                {FieldMappingFieldId.BasisOfRecord, basisOfRecordFieldMappingFactory},
                {FieldMappingFieldId.Continent, continentFieldMappingFactory},
                {FieldMappingFieldId.County, countyFieldMappingFactory},
                {FieldMappingFieldId.Municipality, municipalityFieldMappingFactory},
                {FieldMappingFieldId.Province, provinceFieldMappingFactory},
                {FieldMappingFieldId.Parish, parishFieldMappingFactory},
                {FieldMappingFieldId.Type, typeFieldMappingFactory},
                {FieldMappingFieldId.Country, countryFieldMappingFactory},
                {FieldMappingFieldId.AccessRights, accessRightsFieldMappingFactory},
                {FieldMappingFieldId.OccurrenceStatus, occurrenceStatusFieldMappingFactory},
                {FieldMappingFieldId.EstablishmentMeans, establishmentMeansFieldMappingFactory},
                {FieldMappingFieldId.AreaType, areaTypeFieldMappingFactory},
                {FieldMappingFieldId.DiscoveryMethod, discoveryMethodFieldMappingFactory},
                {FieldMappingFieldId.DeterminationMethod, determinationMethodFieldMappingFactory}
            };
        }

        /// <summary>
        ///     Import field mappings.
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestAsync()
        {
            var harvestInfo = new HarvestInfo(nameof(FieldMapping), DataProviderType.FieldMappings, DateTime.Now);
            var fieldMappings = new List<FieldMapping>();
            try
            {
                _logger.LogDebug("Start importing field mappings");

                foreach (var fileName in Directory.GetFiles(@"Resources\FieldMappings\"))
                {
                    var fieldMapping = CreateFieldMappingFromJsonFile(fileName);
                    fieldMappings.Add(fieldMapping);
                }

                fieldMappings = fieldMappings.OrderBy(m => m.Id).ToList();

                await _fieldMappingRepository.DeleteCollectionAsync();
                await _fieldMappingRepository.AddCollectionAsync();
                await _fieldMappingRepository.AddManyAsync(fieldMappings);
                _logger.LogDebug("Finish storing field mappings");
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = fieldMappings.Count;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed importing field mappings");
                harvestInfo.Status = RunStatus.Failed;
            }

            harvestInfo.End = DateTime.Now;
            return harvestInfo;
        }

        /// <inheritdoc />
        public async Task<byte[]> CreateFieldMappingsZipFileAsync(IEnumerable<FieldMappingFieldId> fieldMappingFieldIds)
        {
            var fielMappingFileTuples = new List<(string Filename, byte[] Bytes)>();
            foreach (var fieldMappingFieldId in fieldMappingFieldIds)
            {
                fielMappingFileTuples.Add(await CreateFieldMappingFileAsync(fieldMappingFieldId));
            }

            var zipFile = CreateZipFile(fielMappingFileTuples);
            return zipFile;
        }

        public async Task<IEnumerable<FieldMapping>> CreateAllFieldMappingsAsync(
            IEnumerable<FieldMappingFieldId> fieldMappingFieldIds)
        {
            var fieldMappings = new List<FieldMapping>();
            foreach (var fieldMappingFieldId in fieldMappingFieldIds)
            {
                var fieldMapping = await CreateFieldMappingAsync(fieldMappingFieldId);
                fieldMappings.Add(fieldMapping);
            }

            return fieldMappings;
        }

        /// <inheritdoc />
        public async Task<(string Filename, byte[] Bytes)> CreateFieldMappingFileAsync(
            FieldMappingFieldId fieldMappingFieldId)
        {
            var filename = $"{fieldMappingFieldId.ToString()}FieldMapping.json";
            var fieldMapping = await CreateFieldMappingAsync(fieldMappingFieldId);
            return CreateFieldMappingFileResult(fieldMapping, filename);
        }

        private async Task<FieldMapping> CreateFieldMappingAsync(FieldMappingFieldId fieldMappingFieldId)
        {
            var fieldMappingFactory = _fieldMappingFactoryById[fieldMappingFieldId];
            return await fieldMappingFactory.CreateFieldMappingAsync();
        }

        private FieldMapping CreateFieldMappingFromJsonFile(string filename)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, filename);
            var str = File.ReadAllText(filePath, Encoding.UTF8);
            var fieldMapping = JsonConvert.DeserializeObject<FieldMapping>(str);
            ValidateFieldMapping(fieldMapping);
            return fieldMapping;
        }

        private void ValidateFieldMapping(FieldMapping fieldMapping)
        {
            var externalSystemMappingFields = fieldMapping?.ExternalSystemsMapping?.SelectMany(mapping => mapping.Mappings);
            if (externalSystemMappingFields == null) return;

            foreach (var externalSystemMappingField in externalSystemMappingFields)
            {
                // Check if there exists duplicate synonyms.
                if (externalSystemMappingField.Values.First().Value is string)
                {
                    if (externalSystemMappingField.Values.Select(m => m.Value.ToString()?.ToLower()).HasDuplicates())
                    {
                        throw new Exception($"Duplicate mappings exist for field \"{fieldMapping.Name}\"");
                    }
                }
                else
                {
                    if (externalSystemMappingField.Values.Select(m => m.Value).HasDuplicates())
                    {
                        throw new Exception($"Duplicate mappings exist for field \"{fieldMapping.Name}\"");
                    }
                }
            }
        }

        private (string Filename, byte[] Bytes) CreateFieldMappingFileResult(FieldMapping fieldMapping, string fileName)
        {
            var bytes = SerializeToJsonArray(fieldMapping,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}, Formatting.Indented);
            return (Filename: fileName, Bytes: bytes);
        }

        private byte[] SerializeToJsonArray(object value, JsonSerializerSettings jsonSerializerSettings,
            Formatting formatting)
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