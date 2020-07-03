﻿using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    ///     Field mappings repository.
    /// </summary>
    public class ProcessedFieldMappingRepository : ProcessBaseRepository<FieldMapping, FieldMappingFieldId>,
        IProcessedFieldMappingRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessedFieldMappingRepository(
            IProcessClient client,
            ILogger<ProcessBaseRepository<FieldMapping, FieldMappingFieldId>> logger) : base(client, false, logger)
        {
        }
    }
}