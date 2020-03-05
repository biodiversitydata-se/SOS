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
    public class ValidationStatusArtportalenFieldMappingFactory : ArtportalenFieldMappingFactoryBase, Interfaces.IValidationStatusFieldMappingFactory
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<ValidationStatusArtportalenFieldMappingFactory> _logger;
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.ValidationStatus;
        protected override bool Localized => true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public ValidationStatusArtportalenFieldMappingFactory(
            IMetadataRepository artportalenMetadataRepository,
            ILogger<ValidationStatusArtportalenFieldMappingFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ?? throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<ICollection<FieldMappingValue>> GetFieldMappingValues()
        {
            var validationStatusList = await _artportalenMetadataRepository.GetValidationStatusAsync();
            var fieldMappingValues = base.ConvertToLocalizedFieldMappingValues(validationStatusList.ToArray());
            return fieldMappingValues;
        }
    }
}