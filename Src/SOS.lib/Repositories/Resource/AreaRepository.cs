﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nest;
using NetTopologySuite.Geometries;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Area repository
    /// </summary>
    public class AreaRepository : RepositoryBase<Area, int>, IAreaRepository
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
            _jsonSerializerOptions.Converters.Add(new GeoShapeConverter());

            if (Database != null)
            {
                _gridFSBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = nameof(Area) });
            }
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            var indexModels = new List<CreateIndexModel<Area>>
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
            var bytes = await _gridFSBucket.DownloadAsBytesByNameAsync($"geometry-{areaId}");
            var utfString = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            return JsonSerializer.Deserialize<IGeoShape>(utfString, _jsonSerializerOptions);
        }

        public async Task<List<Area>> GetAsync(AreaType[] areaTypes)
        {
            var filter = Builders<Area>.Filter.In(y => y.AreaType, areaTypes);
            var res = await (await MongoCollection.FindAsync(filter)).ToListAsync();
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
        public async Task<bool> StoreGeometriesAsync(IDictionary<int, Geometry> areaGeometries)
        {
            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new GeometryConverter());

            foreach (var geometry in areaGeometries)
            {
                var fileName = $"geometry-{geometry.Key}";
                var geometryString = JsonSerializer.Serialize(geometry.Value, serializeOptions);
                var byteArray = Encoding.UTF8.GetBytes(geometryString);

                await _gridFSBucket.UploadFromBytesAsync(fileName, byteArray);
            }

            return true;
        }
    }
}