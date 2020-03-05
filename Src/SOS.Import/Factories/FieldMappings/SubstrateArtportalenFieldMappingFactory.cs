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
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
{
    /// <summary>
    /// Class for creating substrate field mapping.
    /// </summary>
    public class SubstrateArtportalenFieldMappingFactory : ArtportalenFieldMappingFactoryBase, Interfaces.ISubstrateFieldMappingFactory
    {
        private readonly IMetadataRepository _artportalenMetadataRepository;
        private readonly ILogger<SubstrateArtportalenFieldMappingFactory> _logger;
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.Substrate;
        protected override bool Localized => true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenMetadataRepository"></param>
        /// <param name="logger"></param>
        public SubstrateArtportalenFieldMappingFactory(
            IMetadataRepository artportalenMetadataRepository,
            ILogger<SubstrateArtportalenFieldMappingFactory> logger)
        {
            _artportalenMetadataRepository = artportalenMetadataRepository ?? throw new ArgumentNullException(nameof(artportalenMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<ICollection<FieldMappingValue>> GetFieldMappingValues()
        {
            var substrates = await _artportalenMetadataRepository.GetSubstratesAsync();
            var fieldMappingValues = base.ConvertToLocalizedFieldMappingValues(substrates.ToArray());
            return fieldMappingValues;
        }
    }
}