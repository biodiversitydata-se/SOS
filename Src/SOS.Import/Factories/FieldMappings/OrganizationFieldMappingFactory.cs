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
    public class OrganizationFieldMappingFactory : ArtportalenFieldMappingFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<OrganizationFieldMappingFactory> _logger;
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.Organization;
        protected override bool Localized => false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public OrganizationFieldMappingFactory(
            IMetadataRepository artportalenMetadataRepository,
            ILogger<OrganizationFieldMappingFactory> logger)
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