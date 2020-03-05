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
using SOS.Import.Factories.FieldMappings.Interfaces;
using SOS.Import.Repositories.Source.SpeciesPortal.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Factories.FieldMappings
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