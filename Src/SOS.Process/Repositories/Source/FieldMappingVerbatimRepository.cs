using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class FieldMappingVerbatimRepository : VerbatimBaseRepository<FieldMapping, FieldMappingFieldId>,
        IFieldMappingVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public FieldMappingVerbatimRepository(IVerbatimClient client,
            ILogger<FieldMappingVerbatimRepository> logger) : base(client, logger)
        {
        }
    }
}