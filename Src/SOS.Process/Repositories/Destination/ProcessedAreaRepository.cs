using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nest;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Shared;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Repository for retrieving processed areas.
    /// </summary>
    public class ProcessedAreaRepository : ProcessBaseRepository<Area, int>, IProcessedAreaRepository
    {
        private readonly GridFSBucket _gridFSBucket;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessedAreaRepository(
            IProcessClient client, 
            ILogger<ProcessedAreaRepository> logger) 
            : base(client, false, logger)
        {
            _gridFSBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = nameof(Area) });
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new GeoShapeConverter());
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new List<CreateIndexModel<Area>>()
            {
                new CreateIndexModel<Area>(
                    Builders<Area>.IndexKeys.Ascending(a => a.Name)),
                new CreateIndexModel<Area>(
                    Builders<Area>.IndexKeys.Ascending(a => a.AreaType))
            };

            Logger.LogDebug("Creating Area indexes");
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
        }

        /// <inheritdoc />
        public async Task DropGeometriesAsync()
        {
            await _gridFSBucket.DropAsync();
        }

        /// <inheritdoc />
        public async Task<IGeoShape> GetGeometryAsync(int areaId)
        {
            var bytes = await _gridFSBucket.DownloadAsBytesByNameAsync($"geometry-{ areaId }");
            var utfString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            return JsonSerializer.Deserialize<IGeoShape>(utfString, _jsonSerializerOptions);
        }

        /// <inheritdoc />
        public async Task<bool> StoreGeometryAsync(int id, IGeoShape geometry)
        {
            try
            {
                var fileName = $"geometry-{id}";
                var geometryString = JsonSerializer.Serialize(geometry, _jsonSerializerOptions);
                var byteArray = Encoding.UTF8.GetBytes(geometryString);

                await _gridFSBucket.UploadFromBytesAsync(fileName, byteArray);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}