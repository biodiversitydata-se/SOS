using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Field mapping repository.
    /// </summary>
    public class FieldMappingRepository : RepositoryBase<FieldMapping, FieldMappingFieldId>, IFieldMappingRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public FieldMappingRepository(
            IProcessClient processClient,
            ILogger<FieldMappingRepository> logger) : base(processClient, logger)
        {
        }
    }
}