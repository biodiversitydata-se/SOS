using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    ///     Class for creating life stage field mapping.
    /// </summary>
    public class LifeStageFieldMappingFactory : ArtportalenFieldMappingFactoryBase
    {
        private readonly ILogger<LifeStageFieldMappingFactory> _logger;
        private readonly IMetadataRepository _metadataRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        /// <param name="logger"></param>
        public LifeStageFieldMappingFactory(
            IMetadataRepository metadataRepository,
            ILogger<LifeStageFieldMappingFactory> logger)
        {
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.LifeStage;
        protected override bool Localized => true;

        protected override async Task<ICollection<FieldMappingValue>> GetFieldMappingValues()
        {
            var stages = await _metadataRepository.GetStagesAsync();
            var fieldMappingValues = base.ConvertToLocalizedFieldMappingValues(stages.ToArray());
            return fieldMappingValues;
        }

        protected override List<ExternalSystemMapping> GetExternalSystemMappings(
           ICollection<FieldMappingValue> fieldMappingValues)
        {
            return new List<ExternalSystemMapping>
            {
                GetArtportalenExternalSystemMapping(fieldMappingValues),
                GetDarwinCoreExternalSystemMapping(fieldMappingValues)
            };
        }

        private ExternalSystemMapping GetDarwinCoreExternalSystemMapping(
            ICollection<FieldMappingValue> fieldMappingValues)
        {
            var externalSystemMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.DarwinCore,
                Name = ExternalSystemId.DarwinCore.ToString(),
                Description = "The Darwin Core format (https://dwc.tdwg.org/terms/)",
                Mappings = new List<ExternalSystemMappingField>()
            };

            var dwcMappings = GetDwcMappings(fieldMappingValues);
            var mappingField = new ExternalSystemMappingField
            {
                Key = FieldMappingKeyFields.DwcLifeStage,
                Description = "The lifeStage term (http://rs.tdwg.org/dwc/terms/lifeStage)",
                Values = dwcMappings.Select(pair => new ExternalSystemMappingValue { Value = pair.Key, SosId = pair.Value }).ToList()
            };

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }

        private Dictionary<string, int> GetDwcMappings(ICollection<FieldMappingValue> fieldMappingValues)
        {
            Dictionary<string, string> dwcMappingSynonyms = GetDwcMappingSynonyms();
            Dictionary<string, int> sosIdByText = new Dictionary<string, int>();
            
            foreach (var fieldMappingValue in fieldMappingValues)
            {
                foreach (var fieldMappingTranslation in fieldMappingValue.Translations)
                {
                    if (!string.IsNullOrWhiteSpace(fieldMappingTranslation.Value))
                    {
                        if (!sosIdByText.ContainsKey(fieldMappingTranslation.Value))
                        {
                            sosIdByText.Add(fieldMappingTranslation.Value, fieldMappingValue.Id);
                        }
                    }
                }

                foreach (var keyValuePair in dwcMappingSynonyms.Where(pair => pair.Value == fieldMappingValue.Value))
                {
                    if (!sosIdByText.ContainsKey(keyValuePair.Key))
                    {
                        sosIdByText.Add(keyValuePair.Key, fieldMappingValue.Id);
                    }
                }
            }

            return sosIdByText;
        }

        private Dictionary<string, string> GetDwcMappingSynonyms()
        {
            return new Dictionary<string, string>
            {
                {"egg", "egg"},
                {"eg (egg)", "egg"},
                {"eggs", "egg"},
                {"female with eggs", "egg"},
                {"nestling", "nestling"},
                {"pull", "nestling"},
                {"pull.", "nestling"},
                {"adult", "adult"},
                {"ad.", "adult"},
                {"adults", "adult"},
                {"ad (adult)", "adult"},
                {"adult female", "adult"},
                {"adult male", "adult"},
                {"larva/nymph", "larva/nymph"},
                {"larvae", "larvae"},
                {"lv (larvae)", "larvae"},
                {"larva", "larvae"},
                {"juvenile", "juvenile"},
                {"juvenil", "juvenile"},
                {"jv (juvenile)", "juvenile"},
                {"juv.", "juvenile"},
                {"juv", "juvenile"},
                {"juveniles", "juvenile"},
                {"juvenile female", "juvenile"},
                {"juvenile male", "juvenile"},
                {"juveniles, males, fe", "juvenile"},
                {"pupa", "pupa"},
                {"imago/adult", "image/adult"},
                {"1st calendar year", "1st calendar year"},
                {"1K", "1st calendar year"},
                {"HY", "1st calendar year"},
                {"HY (hatching year)", "1st calendar year"},
                {"1K+", "at least 1st calendar year"},
                {"AHY", "at least 1st calendar year"},
                {"AHY (after hatching year)", "at least 1st calendar year"},
                {"2nd calendar year", "2nd calendar year"},
                {"2K", "2nd calendar year"},
                {"SY", "2nd calendar year"},
                {"SY (second year)", "2nd calendar year"},
                {"2K+", "at least 2nd calendar year"},
                {"ASY", "at least 2nd calendar year"},
                {"ASY (after second year)", "at least 2nd calendar year"},
                {"3rd calendar year", "3rd calendar year"},
                {"3K", "3rd calendar year"},
                {"TH", "3rd calendar year"},
                {"TH (third year)", "3rd calendar year"},
                {"3K+", "at least 3rd calendar year"},
                {"ATH", "at least 3rd calendar year"},
                {"ATH (after third year)", "at least 3rd calendar year"},
                {"fruiting", "in fruit"}
            };

            /*
                todo - Add new values for subadult, nauplius, embryo, c1, c2, c3, c4, c5?
                subad.
                sub-adult
                sub-adults
                subadult male
                subadult
                subadults
                male, subadult
                sub adult
                subadult female
                subad
                sub-adult female
                SUBADULT MALE, FEMAL
                sub adults
                NP (Nauplius)
                "C1 (Copepodite I)"
                "C2 (Copepodite II)"
                "C3 (Copepodite III)"
                "C4 (Copepodite IV)"
                "C5 (Copepodite V)"
                "embryo"
                "embryos"
                "embryo"
                */
        }
    }
}