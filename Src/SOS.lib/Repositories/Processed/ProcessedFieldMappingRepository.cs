using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Repository for retrieving field mappings.
    /// </summary>
    public class ProcessedFieldMappingRepository : MongoDbProcessedRepositoryBase<FieldMapping, FieldMappingFieldId>,
        IProcessedFieldMappingRepository
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessedFieldMappingRepository(
            IProcessClient client,
            ILogger<ProcessedFieldMappingRepository> logger)
            : base(client, false, logger)
        {
        }
    }
}