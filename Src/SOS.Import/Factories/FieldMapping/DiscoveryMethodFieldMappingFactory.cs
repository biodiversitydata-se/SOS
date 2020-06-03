using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMapping
{
    /// <summary>
    ///     Class for creating DiscoveryMethod field mapping.
    /// </summary>
    public class DiscoveryMethodFieldMappingFactory : ArtportalenFieldMappingFactoryBase
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<DiscoveryMethodFieldMappingFactory> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public DiscoveryMethodFieldMappingFactory(IMetadataRepository artportalenMetadataRepository, ILogger<DiscoveryMethodFieldMappingFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ?? throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.DiscoveryMethod;
        protected override bool Localized => true;

        protected override async Task<ICollection<FieldMappingValue>> GetFieldMappingValues()
        {
            var discoveryMethods = await _artportalenMetadataRepository.GetDiscoveryMethodsAsync();
            var fieldMappingValues = ConvertToLocalizedFieldMappingValues(discoveryMethods.ToArray());
            return fieldMappingValues;
        }
    }
}