using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Destination.FieldMappings.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Repositories.Destination.FieldMappings
{
    /// <summary>
    ///     Field mapping repository.
    /// </summary>
    public class FieldMappingRepository : VerbatimRepository<FieldMapping, FieldMappingFieldId>, IFieldMappingRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public FieldMappingRepository(
            IVerbatimClient importClient,
            ILogger<FieldMappingRepository> logger) : base(importClient, logger)
        {
        }
    }
}