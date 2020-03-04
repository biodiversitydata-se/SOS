using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Import.Entities;
using SOS.Import.Repositories.Destination.FieldMappings.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Import.Services;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{

    /// <summary>
    /// Class for creating Gender field mapping.
    /// </summary>
    public class GenderFieldMappingFactory : FieldMappingFactoryBase, Interfaces.IGenderFieldMappingFactory
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<GenderFieldMappingFactory> _logger;
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.Gender;
        protected override bool Localized => true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metadataRepository"></param>
        /// <param name="logger"></param>
        public GenderFieldMappingFactory(
            IMetadataRepository metadataRepository,
            ILogger<GenderFieldMappingFactory> logger)
        {
            _artportalenMetadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<ICollection<FieldMappingValue>> GetFieldMappingValues()
        {
            var genders = await _artportalenMetadataRepository.GetGendersAsync();
            var fieldMappingValues = base.ConvertToLocalizedFieldMappingValues(genders.ToArray());
            return fieldMappingValues;
        }

        protected override List<ExternalSystemMapping> GetExternalSystemMappings(ICollection<FieldMappingValue> fieldMappingValues)
        {
            return new List<ExternalSystemMapping>
            {
                GetArtportalenExternalSystemMapping(fieldMappingValues),
                GetDarwinCoreExternalSystemMapping(fieldMappingValues)
            };
        }

        private ExternalSystemMapping GetDarwinCoreExternalSystemMapping(ICollection<FieldMappingValue> fieldMappingValues)
        {
            ExternalSystemMapping externalSystemMapping = new ExternalSystemMapping
            {
                Id = ExternalSystemId.DarwinCore,
                Name = ExternalSystemId.DarwinCore.ToString(),
                Description = "The Darwin Core format (https://dwc.tdwg.org/terms/)",
                Mappings = new List<ExternalSystemMappingField>()
            };

            ExternalSystemMappingField mappingField = new ExternalSystemMappingField
            {
                Key = FieldMappingKeyFields.DwcSex,
                Description = "The sex term (http://rs.tdwg.org/dwc/terms/sex)",
                Values = new List<ExternalSystemMappingValue>
                {
                    new ExternalSystemMappingValue
                    {
                        Value = "male", SosId = fieldMappingValues.Single(m => m.Name == "male").Id
                    },
                    new ExternalSystemMappingValue
                    {
                        Value = "female", SosId = fieldMappingValues.Single(m => m.Name == "female").Id
                    },
                    new ExternalSystemMappingValue
                    {
                        Value = "hermaphrodite",
                        SosId = fieldMappingValues.Single(m => m.Name == "hermaphroditic").Id
                    },
                    new ExternalSystemMappingValue
                    {
                        Value = "undetermined", SosId = fieldMappingValues.Single(m => m.Name == "empty").Id
                    }
                }
            };

            externalSystemMapping.Mappings.Add(mappingField);
            return externalSystemMapping;
        }
    }
}