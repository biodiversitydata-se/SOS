using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using SOS.Import.Enums;
using SOS.Import.Repositories.Destination.FieldMappings.Interfaces;
using SOS.Import.Repositories.Destination.Taxon.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.JsonConverters;
using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Factories
{
    /// <summary>
    /// Class for importing field mappings into MongoDb.
    /// </summary>
    public class FieldMappingFactory : Interfaces.IFieldMappingFactory { 

        private readonly IFieldMappingRepository _fieldMappingRepository;
        private readonly ILogger<FieldMappingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldMappingRepository"></param>
        /// <param name="logger"></param>
        public FieldMappingFactory(
            IFieldMappingRepository fieldMappingRepository,
            ILogger<FieldMappingFactory> logger)
        {
            _fieldMappingRepository = fieldMappingRepository ?? throw new ArgumentNullException(nameof(fieldMappingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Import field mappings.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ImportAsync()
        {
            try
            {
                _logger.LogDebug("Start importing field mappings");
                var genderFieldMapping = CreateFieldMappingFromFile(@"Resources\GenderFieldMapping.json");
                var activityFieldMapping = CreateFieldMappingFromFile(@"Resources\ActivityFieldMapping.json");
                var fieldMappings = new List<FieldMapping> { genderFieldMapping, activityFieldMapping };
                await _fieldMappingRepository.DeleteCollectionAsync();
                await _fieldMappingRepository.AddCollectionAsync();
                await _fieldMappingRepository.AddManyAsync(fieldMappings);
                _logger.LogDebug("Finish storing field mappings");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed importing field mappings");
                return false;
            }

            return true;
        }

        private FieldMapping CreateFieldMappingFromFile(string filename)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, filename);
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var serializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new FieldMappingValueConverter() }
            };
            var fieldMappings = JsonConvert.DeserializeObject<FieldMapping>(str, serializerSettings);
            return fieldMappings;
        }
    }
}