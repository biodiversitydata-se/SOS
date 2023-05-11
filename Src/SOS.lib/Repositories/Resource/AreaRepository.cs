using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using NetTopologySuite.Geometries;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Area repository
    /// </summary>
    public class AreaRepository : RepositoryBase<Area, string>, IAreaRepository
    {
        private readonly GridFSBucket _gridFSBucket;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public AreaRepository(
            IProcessClient processClient,
            ILogger<AreaRepository> logger) : base(processClient, logger)
        {
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new GeometryConverter());

            if (Database != null)
            {
                _gridFSBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = nameof(Area) });
            }
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new[]
            {
                new CreateIndexModel<Area>(
                    Builders<Area>.IndexKeys.Ascending(a => a.Name)),
                new CreateIndexModel<Area>(
                    Builders<Area>.IndexKeys.Ascending(a => a.AreaType)),
                new CreateIndexModel<Area>(
                    Builders<Area>.IndexKeys.Ascending(a => a.FeatureId)),
                new CreateIndexModel<Area>(
                    Builders<Area>.IndexKeys.Ascending(a => a.AreaType).Ascending(a => a.FeatureId))
            };

            Logger.LogDebug("Start creating Area indexes");
            await MongoCollection.Indexes.CreateManyAsync(indexModels);
            Logger.LogDebug("Finish creating Area indexes");
        }

        /// <inheritdoc />
        public async Task DropGeometriesAsync()
        {
            await _gridFSBucket.DropAsync();
        }

        /// <inheritdoc />
        public async Task<Geometry> GetGeometryAsync(AreaType areaType, string featureId)
        {
            try
            {
                var bytes = await _gridFSBucket.DownloadAsBytesByNameAsync(areaType.ToAreaId(featureId));
                var utfString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                return JsonSerializer.Deserialize<Geometry>(utfString, _jsonSerializerOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Area>> GetAsync(AreaType[] areaTypes)
        {
            var filter = Builders<Area>.Filter.In(y => y.AreaType, areaTypes);
            var res = await (await MongoCollection.FindAsync(filter)).ToListAsync();
            return res;
        }

        /// <inheritdoc />
        public async Task<Area> GetAsync(AreaType areaType, string feature)
        {
            var filters = new [] {
                Builders<Area>.Filter.Eq(a => a.AreaType, areaType),
                Builders<Area>.Filter.Eq(a => a.FeatureId, feature)
            };

            var res = await (await MongoCollection.FindAsync(Builders<Area>.Filter.And(filters))).FirstOrDefaultAsync();
            return res;
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
            else
            {
                // Make sure economic zone of sweden is NOT matched
                filters.Add(Builders<Area>.Filter.In(a => a.AreaType, ((AreaType[])Enum.GetValues(typeof(AreaType))).Where(at => at != AreaType.EconomicZoneOfSweden))); 
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
        public async Task<bool> StoreGeometriesAsync(IDictionary<string, Geometry> areaGeometries)
        {
            foreach (var geometry in areaGeometries)
            {
                var fileName = geometry.Key;
                var geometryString = JsonSerializer.Serialize(geometry.Value, _jsonSerializerOptions);
                var byteArray = Encoding.UTF8.GetBytes(geometryString);

                await _gridFSBucket.UploadFromBytesAsync(fileName, byteArray);
            }

            return true;
        }
    }
}