using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Destination.SpeciesPortal.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating geographical region field mappings.
    /// </summary>
    public class GeoRegionFieldMappingFactory : Interfaces.IGeoRegionFieldMappingFactory
    {
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly ILogger<GeoRegionFieldMappingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="areaVerbatimRepository"></param>
        /// <param name="logger"></param>
        public GeoRegionFieldMappingFactory(
            IAreaRepository areaRepository,
            IAreaVerbatimRepository areaVerbatimRepository,
            ILogger<GeoRegionFieldMappingFactory> logger)
        {
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<Dictionary<FieldMappingFieldId, FieldMapping>> CreateFieldMappingsAsync()
        {
            var areas = (await _areaVerbatimRepository.GetAllAsync()).ToArray();
            var countyFieldMapping = CreateFieldMapping(areas, FieldMappingFieldId.County, AreaType.County);
            var municipalityFieldMapping = CreateFieldMapping(areas, FieldMappingFieldId.Municipality, AreaType.Municipality);
            var provinceFieldMapping = CreateFieldMapping(areas, FieldMappingFieldId.Province, AreaType.Province);
            var parishFieldMapping = CreateFieldMapping(areas, FieldMappingFieldId.Parish, AreaType.Parish);
            var dictionary = new Dictionary<FieldMappingFieldId, FieldMapping>
            {
                { FieldMappingFieldId.County, countyFieldMapping },
                { FieldMappingFieldId.Municipality, municipalityFieldMapping },
                { FieldMappingFieldId.Province, provinceFieldMapping },
                { FieldMappingFieldId.Parish, parishFieldMapping }
            };

            return dictionary;
        }

        private FieldMapping CreateFieldMapping(ICollection<Area> areas, FieldMappingFieldId fieldMappingFieldId, AreaType areaType)
        {
            var selectedAreas = areas.Where(m => m.AreaType == areaType).ToArray();
            FieldMapping fieldMapping = new FieldMapping
            {
                Id = fieldMappingFieldId,
                Name = fieldMappingFieldId.ToString(),
                Localized = false,
                Values = CreateFieldMappingValues(selectedAreas, areaType),
                ExternalSystemsMapping = new List<ExternalSystemMapping>()
            };

            fieldMapping.ExternalSystemsMapping.Add(GetArtportalenExternalSystemMapping(selectedAreas));
            fieldMapping.ExternalSystemsMapping.Add(GetSSosExternalSystemMapping(selectedAreas));
            if (areaType == AreaType.County || areaType == AreaType.Municipality || areaType == AreaType.Province)
            {
                fieldMapping.ExternalSystemsMapping.Add(GetDarwinCoreExternalSystemMapping(selectedAreas, areaType));
            }

            return fieldMapping;
        }

        private ExternalSystemMapping GetDarwinCoreExternalSystemMapping(IEnumerable<Area> areas, AreaType areaType)
        {
            ExternalSystemMapping externalSystemMapping = new ExternalSystemMapping
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
                    return FieldMappingKeyFields.DwcCounty;
                case AreaType.Municipality:
                    return FieldMappingKeyFields.DwcMunicipality;
                case AreaType.Province:
                    return FieldMappingKeyFields.DwcStateProvince;
                default:
                    throw new ArgumentException($"DarwinCore don't have support for {areaType}");
            }
        }

        private ExternalSystemMappingField GetDarwinCoreTermMapping(IEnumerable<Area> areas, AreaType areaType)
        {
            string darwinCoreTerm = GetDarwinCoreTerm(areaType);
            ExternalSystemMappingField mappingField = new ExternalSystemMappingField
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
            ExternalSystemMapping externalSystemMapping = new ExternalSystemMapping
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
            ExternalSystemMappingField mappingField = new ExternalSystemMappingField
            {
                Key = FieldMappingKeyFields.AreaDatasetIdFeatureIdTuple,
                Description = "The key is a tuple of <AreaDatasetId, FeatureId>",
                Values = new List<ExternalSystemMappingValue>()
            };

            foreach (var areaEntity in areas)
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value = new { AreaDatasetId = areaEntity.AreaType, FeatureId = areaEntity.FeatureId },
                    SosId = areaEntity.Id
                });
            }

            return mappingField;
        }

        private ExternalSystemMappingField GetArtportalenFeatureIdMapping(IEnumerable<Area> areas)
        {
            ExternalSystemMappingField mappingField = new ExternalSystemMappingField
            {
                Key = FieldMappingKeyFields.FeatureId,
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
            ExternalSystemMappingField mappingField = new ExternalSystemMappingField
            {
                Key = FieldMappingKeyFields.Id,
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

        private ICollection<FieldMappingValue> CreateFieldMappingValues(IEnumerable<Area> areas, AreaType areaType)
        {
            List<FieldMappingValue> values = new List<FieldMappingValue>();
            foreach (var area in areas)
            {
                values.Add(new FieldMappingValue
                {
                    Id = area.Id,
                    Name = area.Name,
                    Localized = false,
                    Extra = new { AreaDatasetId = area.AreaType, FeatureId = area.FeatureId }
                });
            }

            return values;
        }

        private ExternalSystemMapping GetSSosExternalSystemMapping(ICollection<Area> areas)
        {
            ExternalSystemMapping externalSystemMapping = new ExternalSystemMapping
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
            ExternalSystemMappingField mappingField = new ExternalSystemMappingField
            {
                Key = FieldMappingKeyFields.Guid,
                Description = "The key is WebRegion.GUID in SSOS",
                Values = new List<ExternalSystemMappingValue>()
            };

            foreach (var areaEntity in areas)
            {
                mappingField.Values.Add(new ExternalSystemMappingValue
                {
                    Value = $"URN:LSID:artportalen.se:area:DataSet{(int)areaEntity.AreaType}Feature{areaEntity.FeatureId}",
                    SosId = areaEntity.Id
                });
            }

            return mappingField;
        }
    }
}