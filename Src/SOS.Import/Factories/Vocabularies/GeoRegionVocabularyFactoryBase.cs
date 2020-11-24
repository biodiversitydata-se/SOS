using System;
using Microsoft.Extensions.Logging;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Factories.Vocabularies
{
    /// <summary>
    ///     Class for creating geographical region vocabulary.
    /// </summary>
    public abstract class GeoRegionVocabularyFactoryBase
    {
        private readonly IAreaRepository _areaProcessedRepository;
        private readonly ILogger<GeoRegionVocabularyFactoryBase> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="areaProcessedRepository"></param>
        /// <param name="logger"></param>
        protected GeoRegionVocabularyFactoryBase(
            IAreaRepository areaProcessedRepository,
            ILogger<GeoRegionVocabularyFactoryBase> logger)
        {
            _areaProcessedRepository =
                areaProcessedRepository ?? throw new ArgumentNullException(nameof(areaProcessedRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetDarwinCoreTerm(AreaType areaType)
        {
            switch (areaType)
            {
                case AreaType.County:
                    return VocabularyMappingKeyFields.DwcCounty;
                case AreaType.Municipality:
                    return VocabularyMappingKeyFields.DwcMunicipality;
                case AreaType.Province:
                    return VocabularyMappingKeyFields.DwcStateProvince;
                default:
                    throw new ArgumentException($"DarwinCore don't have support for {areaType}");
            }
        
        }
    }
}