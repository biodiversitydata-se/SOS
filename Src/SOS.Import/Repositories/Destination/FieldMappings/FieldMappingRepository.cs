using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.FieldMappings.Interfaces;
using SOS.Import.Repositories.Destination.Taxon.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Repositories.Destination.FieldMappings
{
    /// <summary>
    /// Field mapping repository.
    /// </summary>
    public class FieldMappingRepository : VerbatimRepository<FieldMapping, int>, IFieldMappingResourceRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public FieldMappingRepository(
            IImportClient importClient,
            ILogger<FieldMappingRepository> logger) : base(importClient, logger)
        {
        }
    }
}
