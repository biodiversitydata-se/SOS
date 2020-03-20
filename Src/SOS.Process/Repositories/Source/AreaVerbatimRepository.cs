using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class AreaVerbatimRepository : VerbatimBaseRepository<Area, int>, Interfaces.IAreaVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public AreaVerbatimRepository(IVerbatimClient client,
            ILogger<AreaVerbatimRepository> logger) : base(client, logger)
        {

        }

        /// <inheritdoc />
        public async Task<IEnumerable<Area>> GetAreasByCoordinatesAsync(double longitude, double latitude)
        {
            try
            {
                var filter = Builders<Area>.Filter.GeoIntersects(a => a.Geometry,
                    new GeoJsonPoint<GeoJson2DCoordinates>(
                        new GeoJson2DCoordinates(longitude, latitude)
                    )
                );

                var res = await MongoCollection
                    .Find(filter)
                    .Project(a => new Area(a.AreaType){ Id = a.Id, Name = a.Name }) // Project to skip geometry
                    .ToListAsync();

                return res;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());

                return default;
            }
        }

        /// <inheritdoc />
        public async Task<List<AreaBase>> GetAllAreaBaseAsync()
        {
            var res = await MongoCollection
                .Find(x => true)
                .Project(m => new AreaBase(m.AreaType)
                {
                    FeatureId = m.FeatureId,
                    Id = m.Id,
                    Name = m.Name,
                    ParentId = m.ParentId,
                })
                .ToListAsync();

            return res;
        }
    }
}