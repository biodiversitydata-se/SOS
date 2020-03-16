using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Entities;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating institution field mapping.
    /// </summary>
    public class InstitutionFieldMappingFactory : ArtportalenFieldMappingFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<InstitutionFieldMappingFactory> _logger;
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.Institution;
        protected override bool Localized => false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public InstitutionFieldMappingFactory(
            IMetadataRepository artportalenMetadataRepository,
            ILogger<InstitutionFieldMappingFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ?? throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<ICollection<FieldMappingValue>> GetFieldMappingValues()
        {
            var organizations = await _artportalenMetadataRepository.GetOrganizationsAsync();
            var fieldMappingValues = base.ConvertToNonLocalizedFieldMappingValues(organizations.ToArray());
            return fieldMappingValues;
        }
    }
}