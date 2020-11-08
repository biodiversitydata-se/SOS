using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating geographical region vocabulary.
    /// </summary>
    public abstract class GeoRegionVocabularyFactoryBase
    {
        private readonly IAreaRepository _areaProcessedRepository;
        private readonly ILogger<GeoRegionVocabularyFactoryBase> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="areaProcessedRepository"></param>
        /// <param name="logger"></param>
        protected GeoRegionVocabularyFactoryBase(
            IAreaRepository areaProcessedRepository,
            ILogger<GeoRegionVocabularyFactoryBase> logger)
        {
            _areaProcessedRepository =
                areaProcessedRepository ?? throw new ArgumentNullException(nameof(areaProcessedRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        protected async Task<Vocabulary> CreateVocabularyAsync(
            VocabularyId vocabularyId, AreaType areaType)
        {
            var areas = (await _areaProcessedRepository.GetAllAsync()).ToArray();
            var vocabulary = CreateVocabulary(areas, vocabularyId, areaType);
            return vocabulary;
        }

        private Vocabulary CreateVocabulary(ICollection<Area> areas,
            VocabularyId vocabularyId, AreaType areaType)
        {
            var selectedAreas = areas.Where(m => m.AreaType == areaType).ToArray();
            var vocabulary = new Vocabulary
            {
                Id = vocabularyId,
                Name = vocabularyId.ToString(),
                Localized = false,
                Values = CreateVocabularyValues(selectedAreas, areaType),
                ExternalSystemsMapping = new List<ExternalSystemMapping>()
            };

            vocabulary.ExternalSystemsMapping.Add(GetArtportalenExternalSystemMapping(selectedAreas));
            vocabulary.ExternalSystemsMapping.Add(GetSSosExternalSystemMapping(selectedAreas));
            if (areaType == AreaType.County || areaType == AreaType.Municipality || areaType == AreaType.Province)
            {
                vocabulary.ExternalSystemsMapping.Add(GetDarwinCoreExternalSystemMapping(selectedAreas, areaType));
            }

            return vocabulary;
        }

        private ExternalSystemMapping GetDarwinCoreExternalSystemMapping(IEnumerable<Area> areas, AreaType areaType)
        {
            var externalSystemMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.DarwinCore,
                Name = ExternalSystemId.DarwinCore.ToString(),
                Description = "The Darwin Core format(https://dwc.tdwg.org/terms/)",
                Mappings = new List<ExternalSystemMappingField>()
            };

            externalSystemMapping.Mappings.Add(GetDarwinCoreTermMapping(areas, areaType));
            return externalSystemMapping;
        }

        private string GetDarwinCoreTerm(AreaType areaType)
        {
            switch (areaType)
            {
                case AreaType.County:
                    return VocabularyMappingKeyFields.DwcCounty;
                case AreaType.Municipality:
                    return VocabularyMappingKeyFields.DwcMunicipality;
                case AreaType.Province:
                    return VocabularyMappingKeyFields.DwcStateProvince;
                default:
                    throw new ArgumentException($"DarwinCore don't have support for {areaType}");
            }
        }

        private ExternalSystemMappingField GetDarwinCoreTermMapping(IEnumerable<Area> areas, AreaType areaType)
        {
            var darwinCoreTerm = GetDarwinCoreTerm(areaType);
            var mappingField = new ExternalSystemMappingField
            {
                Key = darwinCoreTerm,
                Description = $"The {darwinCoreTerm} term (http://rs.tdwg.org/dwc/terms/{darwinCoreTerm})",
                Values = new List<ExternalSystemMappingValue>()
            };

            // 1-1 mapping between Id fields.
            foreach (var area in areas)
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value = area.Name,
                    SosId = area.Id
                });
            }

            return mappingField;
        }

        private ExternalSystemMapping GetArtportalenExternalSystemMapping(ICollection<Area> areas)
        {
            var externalSystemMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.Artportalen,
                Name = ExternalSystemId.Artportalen.ToString(),
                Description = "The Artportalen system",
                Mappings = new List<ExternalSystemMappingField>()
            };

            externalSystemMapping.Mappings.Add(GetArtportalenIdMapping(areas));
            externalSystemMapping.Mappings.Add(GetArtportalenTupleMapping(areas));
            externalSystemMapping.Mappings.Add(GetArtportalenFeatureIdMapping(areas));
            return externalSystemMapping;
        }

        private ExternalSystemMappingField GetArtportalenTupleMapping(IEnumerable<Area> areas)
        {
            var mappingField = new ExternalSystemMappingField
            {
                Key = VocabularyMappingKeyFields.AreaDatasetIdFeatureIdTuple,
                Description = "The key is a tuple of <AreaDatasetId, FeatureId>",
                Values = new List<ExternalSystemMappingValue>()
            };

            foreach (var areaEntity in areas)
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value = new {AreaDatasetId = areaEntity.AreaType, areaEntity.FeatureId},
                    SosId = areaEntity.Id
                });
            }

            return mappingField;
        }

        private ExternalSystemMappingField GetArtportalenFeatureIdMapping(IEnumerable<Area> areas)
        {
            var mappingField = new ExternalSystemMappingField
            {
                Key = VocabularyMappingKeyFields.FeatureId,
                Description = "The key is FeatureId",
                Values = new List<ExternalSystemMappingValue>()
            };

            foreach (var areaEntity in areas)
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value = areaEntity.FeatureId,
                    SosId = areaEntity.Id
                });
            }

            return mappingField;
        }


        private ExternalSystemMappingField GetArtportalenIdMapping(IEnumerable<Area> areas)
        {
            var mappingField = new ExternalSystemMappingField
            {
                Key = VocabularyMappingKeyFields.Id,
                Description = "The Area.Id field",
                Values = new List<ExternalSystemMappingValue>()
            };

            // 1-1 mapping between Id fields.
            foreach (var area in areas)
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value = area.Id,
                    SosId = area.Id
                });
            }

            return mappingField;
        }

        private ICollection<VocabularyValueInfo> CreateVocabularyValues(IEnumerable<Area> areas, AreaType areaType)
        {
            var values = new List<VocabularyValueInfo>();
            foreach (var area in areas)
            {
                values.Add(new VocabularyValueInfo
                {
                    Id = area.Id,
                    Value = area.Name,
                    Localized = false,
                    Extra = new {AreaDatasetId = area.AreaType, area.FeatureId}
                });
            }

            return values;
        }

        private ExternalSystemMapping GetSSosExternalSystemMapping(ICollection<Area> areas)
        {
            var externalSystemMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.SwedishSpeciesObservationService,
                Name = "Swedish Species Observation Service (SSOS)",
                Description = "The Artdatabanken SOAP-based Swedish Species Observation Service (SSOS)",
                Mappings = new List<ExternalSystemMappingField>()
            };

            externalSystemMapping.Mappings.Add(GetSsosGuidMapping(areas));
            return externalSystemMapping;
        }

        private ExternalSystemMappingField GetSsosGuidMapping(ICollection<Area> areas)
        {
            var mappingField = new ExternalSystemMappingField
            {
                Key = VocabularyMappingKeyFields.Guid,
                Description = "The key is WebRegion.GUID in SSOS",
                Values = new List<ExternalSystemMappingValue>()
            };

            foreach (var areaEntity in areas)
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value =
                        $"URN:LSID:artportalen.se:area:DataSet{(int) areaEntity.AreaType}Feature{areaEntity.FeatureId}",
                    SosId = areaEntity.Id
                });
            }

            return mappingField;
        }
    }
}