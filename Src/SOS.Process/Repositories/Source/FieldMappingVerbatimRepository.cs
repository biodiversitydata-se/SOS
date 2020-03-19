using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using SOS.Lib.Enums;
using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Shared;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class FieldMappingVerbatimRepository : VerbatimBaseRepository<FieldMapping, FieldMappingFieldId>, Interfaces.IFieldMappingVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public FieldMappingVerbatimRepository(IVerbatimClient client,
            ILogger<FieldMappingVerbatimRepository> logger) : base(client, logger)
        {
           
        }
    }
}