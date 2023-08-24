using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Harvest.Factories.Vocabularies;
using SOS.Harvest.Factories.Vocabularies.Interfaces;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Harvest.Harvesters
{
    /// <summary>
    ///     Class for handling vocabularies.
    /// </summary>
    public class VocabularyHarvester : IVocabularyHarvester
    {
        private readonly Dictionary<VocabularyId, IVocabularyFactory> _vocabularyFactoryById;
        private readonly IVocabularyRepository _vocabularyRepository;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger<VocabularyHarvester> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vocabularyRepository"></param>
        /// <param name="activityVocabularyFactory"></param>
        /// <param name="sexVocabularyFactory"></param>
        /// <param name="lifeStageVocabularyFactory"></param>
        /// <param name="biotopeVocabularyFactory"></param>
        /// <param name="substrateVocabularyFactory"></param>
        /// <param name="verificationStatusVocabularyFactory"></param>
        /// <param name="institutionVocabularyFactory"></param>
        /// <param name="unitVocabularyFactory"></param>
        /// <param name="basisOfRecordVocabularyFactory"></param>
        /// <param name="continentVocabularyFactory"></param>
        /// <param name="typeVocabularyFactory"></param>
        /// <param name="countryVocabularyFactory"></param>
        /// <param name="accessRightsVocabularyFactory"></param>
        /// <param name="occurrenceStatusVocabularyFactory"></param>
        /// <param name="establishmentMeansVocabularyFactory"></param>
        /// <param name="areaTypeVocabularyFactory"></param>
        /// <param name="discoveryMethodVocabularyFactory"></param>
        /// <param name="determinationMethodVocabularyFactory"></param>
        /// <param name="reproductiveConditionVocabularyFactory"></param>
        /// <param name="behaviorVocabularyFactory"></param>
        /// <param name="sensitivityCategoryVocabularyFactory"></param>
        /// <param name="birdNestActivityVocabularyFactory"></param>
        /// <param name="cacheManager"></param>
        /// <param name="logger"></param>
        public VocabularyHarvester(
            IVocabularyRepository vocabularyRepository,
            ActivityVocabularyFactory activityVocabularyFactory,
            SexVocabularyFactory sexVocabularyFactory,
            LifeStageVocabularyFactory lifeStageVocabularyFactory,
            BiotopeVocabularyFactory biotopeVocabularyFactory,
            SubstrateVocabularyFactory substrateVocabularyFactory,
            VerificationStatusVocabularyFactory verificationStatusVocabularyFactory,
            InstitutionVocabularyFactory institutionVocabularyFactory,
            UnitVocabularyFactory unitVocabularyFactory,
            BasisOfRecordVocabularyFactory basisOfRecordVocabularyFactory,
            ContinentVocabularyFactory continentVocabularyFactory,
            TypeVocabularyFactory typeVocabularyFactory,
            CountryVocabularyFactory countryVocabularyFactory,
            AccessRightsVocabularyFactory accessRightsVocabularyFactory,
            OccurrenceStatusVocabularyFactory occurrenceStatusVocabularyFactory,
            EstablishmentMeansVocabularyFactory establishmentMeansVocabularyFactory,
            AreaTypeVocabularyFactory areaTypeVocabularyFactory,
            DiscoveryMethodVocabularyFactory discoveryMethodVocabularyFactory,
            DeterminationMethodVocabularyFactory determinationMethodVocabularyFactory,
            ReproductiveConditionVocabularyFactory reproductiveConditionVocabularyFactory,
            BehaviorVocabularyFactory behaviorVocabularyFactory,
            SensitivityCategoryVocabularyFactory sensitivityCategoryVocabularyFactory,
            BirdNestActivityVocabularyFactory birdNestActivityVocabularyFactory,
            TaxonCategoryVocabularyFactory taxonCategoryVocabularyFactory,
            ICacheManager cacheManager,
            ILogger<VocabularyHarvester> logger)
        {
            _vocabularyRepository =
                vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vocabularyFactoryById = new Dictionary<VocabularyId, IVocabularyFactory>
            {
                {VocabularyId.LifeStage, lifeStageVocabularyFactory},
                {VocabularyId.Activity, activityVocabularyFactory},
                {VocabularyId.Sex, sexVocabularyFactory},
                {VocabularyId.Biotope, biotopeVocabularyFactory},
                {VocabularyId.Substrate, substrateVocabularyFactory},
                {VocabularyId.VerificationStatus, verificationStatusVocabularyFactory},
                {VocabularyId.Institution, institutionVocabularyFactory},
                {VocabularyId.Unit, unitVocabularyFactory},
                {VocabularyId.BasisOfRecord, basisOfRecordVocabularyFactory},
                {VocabularyId.Continent, continentVocabularyFactory},
                {VocabularyId.Type, typeVocabularyFactory},
                {VocabularyId.Country, countryVocabularyFactory},
                {VocabularyId.AccessRights, accessRightsVocabularyFactory},
                {VocabularyId.OccurrenceStatus, occurrenceStatusVocabularyFactory},
                {VocabularyId.EstablishmentMeans, establishmentMeansVocabularyFactory},
                {VocabularyId.AreaType, areaTypeVocabularyFactory},
                {VocabularyId.DiscoveryMethod, discoveryMethodVocabularyFactory},
                {VocabularyId.DeterminationMethod, determinationMethodVocabularyFactory},
                {VocabularyId.ReproductiveCondition, reproductiveConditionVocabularyFactory},
                {VocabularyId.Behavior, behaviorVocabularyFactory},
                {VocabularyId.SensitivityCategory, sensitivityCategoryVocabularyFactory},
                {VocabularyId.BirdNestActivity, birdNestActivityVocabularyFactory},
                {VocabularyId.TaxonCategory, taxonCategoryVocabularyFactory}
            };
        }

        /// <summary>
        ///     Import vocabularies.
        /// </summary>
        /// <returns></returns>
        public async Task<HarvestInfo> HarvestAsync()
        {
            var harvestInfo = new HarvestInfo(nameof(Vocabulary), DateTime.Now);
            var vocabularies = new List<Vocabulary>();
            try
            {
                _logger.LogDebug("Start importing vocabularies");

                foreach (var fileName in Directory.GetFiles(@"Resources/Vocabularies/"))
                {
                    var vocabulary = CreateVocabularyFromJsonFile(fileName);

                    if (vocabulary != null)
                    {
                        vocabularies.Add(vocabulary);
                    }
                }

                vocabularies = vocabularies.OrderBy(m => m.Id).ToList();

                await _vocabularyRepository.DeleteCollectionAsync();
                await _vocabularyRepository.AddCollectionAsync();
                await _vocabularyRepository.AddManyAsync(vocabularies);
                _logger.LogDebug("Finish storing vocabularies");

                // Clear observation api cache
                await _cacheManager.ClearAsync(Cache.Vocabulary);

                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = vocabularies.Count;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed importing vocabularies");
                harvestInfo.Status = RunStatus.Failed;
            }

            harvestInfo.End = DateTime.Now;
            return harvestInfo;
        }

        /// <inheritdoc />
        public async Task<byte[]> CreateVocabulariesZipFileAsync(IEnumerable<VocabularyId> vocabularyIds)
        {
            var fielMappingFileTuples = new List<(string Filename, byte[] Bytes)>();
            foreach (var vocabularyId in vocabularyIds)
            {
                fielMappingFileTuples.Add(await CreateVocabularyFileAsync(vocabularyId));
            }

            var zipFile = ZipFileHelper.CreateZipFile(fielMappingFileTuples);
            return zipFile;
        }

        public async Task<IEnumerable<Vocabulary>> CreateAllVocabulariesAsync(
            IEnumerable<VocabularyId> vocabularyIds)
        {
            var vocabularies = new List<Vocabulary>();
            foreach (var vocabularyId in vocabularyIds)
            {
                var vocabulary = await CreateVocabularyAsync(vocabularyId);
                vocabularies.Add(vocabulary);
            }

            return vocabularies;
        }

        /// <inheritdoc />
        public async Task<(string Filename, byte[] Bytes)> CreateVocabularyFileAsync(
            VocabularyId vocabularyId)
        {
            var filename = $"{vocabularyId}Vocabulary.json";
            var vocabulary = await CreateVocabularyAsync(vocabularyId);
            return CreateVocabularyFileResult(vocabulary, filename);
        }

        private async Task<Vocabulary> CreateVocabularyAsync(VocabularyId vocabularyId)
        {
            var vocabularyFactory = _vocabularyFactoryById[vocabularyId];
            return await vocabularyFactory.CreateVocabularyAsync();
        }

        private Vocabulary? CreateVocabularyFromJsonFile(string filename)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath!, filename);
            var str = File.ReadAllText(filePath, Encoding.UTF8);
            var vocabulary = JsonConvert.DeserializeObject<Vocabulary>(str);
            ValidateVocabulary(vocabulary);
            return vocabulary;
        }

        private void ValidateVocabulary(Vocabulary? vocabulary)
        {
            if (vocabulary == null)
            {
                return;
            }

            var externalSystemMappingFields = vocabulary?.ExternalSystemsMapping?.SelectMany(mapping => mapping.Mappings);
            if (externalSystemMappingFields == null) return;

            foreach (var externalSystemMappingField in externalSystemMappingFields)
            {
                if (externalSystemMappingField.Values == null || externalSystemMappingField.Values.Count == 0)
                    continue;

                // Check if there exists duplicate synonyms.
                if (externalSystemMappingField.Values.First().Value is string)
                {
                    if (externalSystemMappingField.Values.Select(m => m.Value.ToString()?.ToLower()).HasDuplicates())
                    {
                        throw new Exception($"Duplicate mappings exist for field \"{vocabulary?.Name}\"");
                    }
                }
                else
                {
                    if (externalSystemMappingField.Values.Select(m => m.Value).HasDuplicates())
                    {
                        throw new Exception($"Duplicate mappings exist for field \"{vocabulary?.Name}\"");
                    }
                }
            }
        }

        private (string Filename, byte[] Bytes) CreateVocabularyFileResult(Vocabulary vocabulary, string fileName)
        {
            var bytes = SerializeToJsonArray(vocabulary,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}, Formatting.Indented);
            return (Filename: fileName, Bytes: bytes);
        }

        private byte[] SerializeToJsonArray(object value, JsonSerializerSettings jsonSerializerSettings,
            Formatting formatting)
        {
            var result = JsonConvert.SerializeObject(value, formatting, jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(result);
        }
    }
}