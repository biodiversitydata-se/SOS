using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Destination.FieldMappings.Interfaces;
using SOS.Import.Repositories.Resource;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Repositories.Destination.FieldMappings
{
    /// <summary>
    ///     Field mapping repository.
    /// </summary>
    public class FieldMappingRepository : ResourceRepositoryBase<FieldMapping, FieldMappingFieldId>, IFieldMappingRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public FieldMappingRepository(
            IProcessClient processClient,
            ILogger<FieldMappingRepository> logger) : base(processClient, false, logger)
        {
        }
    }
}