using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating verification status field mapping.
    /// </summary>
    public class ValidationStatusFieldMappingFactory : ArtportalenFieldMappingFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.ValidationStatus;
        protected override bool Localized => true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        public ValidationStatusFieldMappingFactory(IMetadataRepository artportalenMetadataRepository)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ?? throw new ArgumentNullException(nameof(artportalenMetadataRepository));
        }

        protected override async Task<ICollection<FieldMappingValue>> GetFieldMappingValues()
        {
            var validationStatusList = await _artportalenMetadataRepository.GetValidationStatusAsync();
            var fieldMappingValues = base.ConvertToLocalizedFieldMappingValues(validationStatusList.ToArray());
            return fieldMappingValues;
        }
    }
}