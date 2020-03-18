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
    /// Class for creating life stage field mapping.
    /// </summary>
    public class LifeStageFieldMappingFactory : ArtportalenFieldMappingFactoryBase
    {
        private readonly IMetadataRepository _metadataRepository;
        private readonly ILogger<LifeStageFieldMappingFactory> _logger;
        protected override FieldMappingFieldId FieldId => FieldMappingFieldId.LifeStage;
        protected override bool Localized => true;

        /// <summary>
        /// Constructor
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

        protected override async Task<ICollection<FieldMappingValue>> GetFieldMappingValues()
        {
            var stages = await _metadataRepository.GetStagesAsync();
            var fieldMappingValues = base.ConvertToLocalizedFieldMappingValues(stages.ToArray());
            return fieldMappingValues;
        }
    }
}