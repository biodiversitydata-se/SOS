using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GridFS;
using NetTopologySuite.Geometries;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Shared;

namespace SOS.Import.Repositories.Destination.Artportalen
{
    /// <summary>
    ///     Area repository
    /// </summary>
    public class AreaVerbatimRepository : VerbatimRepository<Area, int>, IAreaVerbatimRepository
    {
        private readonly GridFSBucket _gridFSBucket;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public AreaVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<AreaVerbatimRepository> logger) : base(importClient, logger)
        {
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new GeoShapeConverter());

            _gridFSBucket = new GridFSBucket(Database, new GridFSBucketOptions {BucketName = nameof(Area)});
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