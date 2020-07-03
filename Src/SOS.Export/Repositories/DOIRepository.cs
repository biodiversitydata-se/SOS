using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.DOI;

namespace SOS.Export.Repositories
{
    /// <summary>
    ///     Process information repository
    /// </summary>
    public class DOIRepository : BaseRepository<DOI, Guid>, IDOIRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="exportClient"></param>
        /// <param name="logger"></param>
        public DOIRepository(
            IProcessClient exportClient,
            ILogger<DOIRepository> logger) : base(exportClient, false, logger)
        {
            //filter by collection name
            var exists = Database
                .ListCollectionNames(new ListCollectionNamesOptions
                {
                    Filter = new BsonDocument("name", CollectionName)
                })
                .Any();

            //check for existence
            if (!exists)
            {
                // Create the collection
                Database.CreateCollection(CollectionName);
            }
        }

        /// <inheritdoc />
        public async Task<bool> AddAsync(DOI doi)
        {
            try
            {
                await MongoCollection.InsertOneAsync(doi);

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }
    }
}