using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nest;
using SOS.Export.Repositories;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;

namespace SOS.Export.Repositories
{
    /// <summary>
    ///     Area repository
    /// </summary>
    public class AreaRepository : BaseRepository<Area, int>, IAreaRepository
    {
        private readonly GridFSBucket _gridFSBucket;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public AreaRepository(
            IProcessClient client,
            ILogger<AreaRepository> logger) : base(client, false, logger)
        {
            _gridFSBucket = new GridFSBucket(Database, new GridFSBucketOptions {BucketName = nameof(Area)});
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new GeoShapeConverter());
        }

        /// <inheritdoc />
        public async Task<PagedResult<Area>> GetAreasAsync(IEnumerable<AreaType> areaTypes, string searchString,
            int skip, int take)
        {
            var filters = new List<FilterDefinition<Area>>();

            if (areaTypes?.Any() ?? false)
            {
                filters.Add(Builders<Area>.Filter.In(a => a.AreaType, areaTypes));
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                filters.Add(Builders<Area>.Filter
                    .Where(f => f
                        .Name.ToLower()
                        .Contains(searchString.ToLower())));
            }

            var filter = filters.Count == 0 ? Builders<Area>.Filter.Empty : Builders<Area>.Filter.And(filters);

            var total = await MongoCollection
                .Find(filter)
                .CountDocumentsAsync();

            var result = await MongoCollection
                .Find(filter)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return new PagedResult<Area>
            {
                Records = result,
                Skip = skip,
                Take = take,
                TotalCount = total
            };
        }

        /// <inheritdoc />
        public async Task<Area> GetAreaAsync(int areaId)
        {
            return await GetAsync(areaId);
        }

        /// <inheritdoc />
        public async Task<IGeoShape> GetGeometryAsync(int areaId)
        {
            var bytes = await _gridFSBucket.DownloadAsBytesByNameAsync($"geometry-{areaId}");
            var utfString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            return JsonSerializer.Deserialize<IGeoShape>(utfString, _jsonSerializerOptions);
        }
    }
}