using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GridFS;
using NetTopologySuite.Geometries;
using SOS.Import.Repositories.Destination.Area.Interfaces;
using SOS.Import.Repositories.Resource;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.JsonConverters;

namespace SOS.Import.Repositories.Destination.Area
{
    /// <summary>
    ///     Area repository
    /// </summary>
    public class AreaProcessedRepository : ResourceRepositoryBase<Lib.Models.Shared.Area, int>, IAreaProcessedRepository
    {
        private readonly GridFSBucket _gridFSBucket;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public AreaProcessedRepository(
            IProcessClient processClient,
            ILogger<AreaProcessedRepository> logger) : base(processClient, false, logger)
        {
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new GeoShapeConverter());

            if (Database != null)
            {
                _gridFSBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = nameof(Area) });
            }
        }

        /// <inheritdoc />
        public async Task DropGeometriesAsync()
        {
            await _gridFSBucket.DropAsync();
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