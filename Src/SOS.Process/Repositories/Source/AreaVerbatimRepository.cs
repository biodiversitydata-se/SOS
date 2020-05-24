using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GridFS;
using Nest;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Shared;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class AreaVerbatimRepository : VerbatimBaseRepository<Area, int>, Interfaces.IAreaVerbatimRepository
    {
        private readonly GridFSBucket _gridFSBucket;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public AreaVerbatimRepository(IVerbatimClient client,
            ILogger<AreaVerbatimRepository> logger) : base(client, logger)
        {
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new GeoShapeConverter());

            _gridFSBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = nameof(Area) });
        }

        /// <inheritdoc />
        public async Task<IGeoShape> GetGeometryAsync(int areaId)
        {
            var bytes = await _gridFSBucket.DownloadAsBytesByNameAsync($"geometry-{ areaId }");
            var utfString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            return JsonSerializer.Deserialize<IGeoShape>(utfString, _jsonSerializerOptions);
        }
    }
}