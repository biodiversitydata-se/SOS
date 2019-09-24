using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonDiffPatchDotNet;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOS.Core.Models;
using SOS.Core.Models.Observations;
using SOS.Core.Models.Versioning;

namespace SOS.Core.Repositories
{
    public class VersionedObservationRepository<T> where T : class, IObservationKey
    {
        private readonly MongoDbContext _dbContext;
        public IMongoCollection<VersionedObservation<T>> Collection { get; private set; }
        private readonly JsonDiffPatch _jdp = new JsonDiffPatch();

        public VersionedObservationRepository(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
            Collection = _dbContext.MongoDbCollection<VersionedObservation<T>>();
        }

        public async Task InsertDocumentAsync(T doc)
        {
            var currentDocument = await GetDocumentAsync(doc.DataProviderId, doc.CatalogNumber);
            if (currentDocument == null) // The document doesn't exist. First insert.
            {
                var versionedDoc = new VersionedObservation<T>(doc);
                Collection.InsertOne(versionedDoc);
                return;
            }

            UpdateVersionedObservationObject(currentDocument, doc);
            var result = Collection.ReplaceOne(item => item.DataProviderId == doc.DataProviderId && item.CatalogNumber == doc.CatalogNumber, currentDocument);

            if (result.ModifiedCount != 1) // the number of modified documents
            {
                // print("Someone must have gotten there first, re-fetch the new document, try again");
                // todo - Is this something that can occur?
            }
        }


        public async Task InsertDocumentsAsync(List<T> speciesObservations)
        {
            var (newObservations, updatedObservations) = await GetNewAndUpdatedObservationsAsync(speciesObservations);

            if (newObservations.Any())
            {
                List<VersionedObservation<T>> newVersionedObservations = new List<VersionedObservation<T>>();
                foreach (var observation in newObservations)
                {
                    newVersionedObservations.Add(new VersionedObservation<T>(observation));
                }
                await Collection.InsertManyAsync(newVersionedObservations);
            }

            if (updatedObservations.Any())
            {
                await UpdateDocumentsAsync(updatedObservations);
            }
        }

        private async Task<(T[] newObservations, T[] updatedObservations)> GetNewAndUpdatedObservationsAsync(
            List<T> speciesObservations)
        {
            List<ObservationKey> foundObservationsTuples = await FindObservationKeysAsync(speciesObservations);
            HashSet<ObservationKey> foundObservationIdsSet = new HashSet<ObservationKey>(foundObservationsTuples);
            var newObservations = speciesObservations.Where(x =>
                    !foundObservationIdsSet.Contains(new ObservationKey(x.DataProviderId, x.CatalogNumber))).ToArray();
            var updatedObservations = speciesObservations.Where(x =>
                    foundObservationIdsSet.Contains(new ObservationKey(x.DataProviderId, x.CatalogNumber))).ToArray();
            return (newObservations, updatedObservations);
        }

        private async Task<List<ObservationKey>> FindObservationKeysAsync(List<T> speciesObservations)
        {
            var filterDef = new FilterDefinitionBuilder<VersionedObservation<T>>();
            var filter = filterDef.In(x => x.CompositeId, speciesObservations.Select(x => $"{x.DataProviderId}_{x.CatalogNumber}"));
            var projection = Builders<VersionedObservation<T>>.Projection
                .Include(x => x.DataProviderId)
                .Include(x => x.CatalogNumber)
                .Exclude("_id");  // _id is special and needs to be explicitly excluded if not needed
            var options = new FindOptions<VersionedObservation<T>, ObservationKey> { Projection = projection };
            var foundObservationsTuples = await (await Collection.FindAsync(filter, options)).ToListAsync();
            return foundObservationsTuples;
        }

        private bool UpdateVersionedObservationObject(VersionedObservation<T> versionedObservation, T updatedObservation)
        {
            var previousVersionNumber = versionedObservation.Version;
            JToken jtokenCurrentDoc = versionedObservation.Current == null
                ? JToken.Parse("{}")
                : JToken.FromObject(versionedObservation.Current);
            JToken diff = _jdp.Diff(jtokenCurrentDoc, JToken.FromObject(updatedObservation));
            if (diff == null) return false; // no change

            versionedObservation.Current = updatedObservation;
            VersionHistory versionHistory = new VersionHistory(diff.ToString(Formatting.None))
            {
                Version = previousVersionNumber,
                UtcDate = versionedObservation.UtcDate,
                Type = versionedObservation.Type,
            };
            versionedObservation.UtcDate = DateTime.UtcNow;
            versionedObservation.Prev.Add(versionHistory);
            versionedObservation.Version = previousVersionNumber + 1;
            versionedObservation.IsDeleted = false;
            versionedObservation.Type = updatedObservation.GetType().ToString();
            return true;
        }

        private async Task UpdateDocumentsAsync(IList<T> updatedObservations)
        {
            Dictionary<string, T> updatedObservationsByCompositeId = updatedObservations.ToDictionary(x => $"{x.DataProviderId}_{x.CatalogNumber}", x => x);
            var filterDef = new FilterDefinitionBuilder<VersionedObservation<T>>();
            var filter = filterDef.In(x => x.CompositeId, updatedObservations.Select(x => $"{x.DataProviderId}_{x.CatalogNumber}"));
            var foundObservations = await (await Collection.FindAsync(filter)).ToListAsync();
            var changedObservations = new ConcurrentBag<VersionedObservation<T>>();
            Parallel.For(0, foundObservations.Count - 1, i =>
            {
                VersionedObservation<T> versionedObservation = foundObservations[i];
                var updatedObservation = updatedObservationsByCompositeId[versionedObservation.CompositeId];
                if (UpdateVersionedObservationObject(versionedObservation, updatedObservation))
                {
                    changedObservations.Add(versionedObservation);
                }
            });

            if (changedObservations.Count > 0)
            {
                // todo - create transaction?
                var deleteFilter = filterDef.In(x => x.CompositeId, changedObservations.Select(x => $"{x.DataProviderId}_{x.CatalogNumber}"));
                await Collection.DeleteManyAsync(deleteFilter);
                await Collection.InsertManyAsync(changedObservations);
            }
        }

        public async Task<VersionedObservation<T>> GetDocumentAsync(int dataProviderId, string catalogNumber)
        {
            return await Collection.FindAsync(x => x.DataProviderId == dataProviderId
                                             && x.CatalogNumber == catalogNumber).Result.FirstOrDefaultAsync();
        }


        public async Task<VersionedObservation<T>> GetDocumentAsync(ObjectId objectId)
        {
            return await Collection.FindAsync(x => x.Id == objectId).Result.FirstOrDefaultAsync();
        }




        public async Task DeleteDocumentAsync(int dataProviderId, string catalogNumber)
        {
            // todo - check if we delete an already deleted document, then return
            var currentDocument = await GetDocumentAsync(dataProviderId, catalogNumber);
            var previousVersionNumber = currentDocument.Version;
            JToken diff = _jdp.Diff(JToken.FromObject(currentDocument.Current), JToken.Parse("{}"));
            currentDocument.Current = null;
            VersionHistory versionHistory = new VersionHistory(diff.ToString(Formatting.None))
            {
                Version = previousVersionNumber,
                UtcDate = currentDocument.UtcDate,
                IsDeleted = true,
                Type = currentDocument.Type
            };
            currentDocument.UtcDate = DateTime.UtcNow;
            currentDocument.Prev.Add(versionHistory);
            currentDocument.Version = previousVersionNumber + 1;
            currentDocument.IsDeleted = true;
            var result = Collection.ReplaceOne(
                item => item.DataProviderId == dataProviderId && item.CatalogNumber == catalogNumber,
                currentDocument);
        }


        public async Task<T> RestoreDocumentAsync(
            int dataProviderId,
            string catalogNumber,
            int version)
        {
            if (version < 1) throw new ArgumentException("Version must be >= 1");
            var jdp = new JsonDiffPatch();
            var currentDocument = await (await Collection.FindAsync(x => x.DataProviderId == dataProviderId
                                                                      && x.CatalogNumber == catalogNumber)).FirstOrDefaultAsync();
            if (currentDocument == null) return null;

            if (version > currentDocument.Version) throw new ArgumentException($"version {version} doesn't exist");
            if (version == currentDocument.Version) return currentDocument.Current;

            int versionCounter = currentDocument.Version;
            JToken jtokenCurrentDoc = currentDocument.Current == null
                ? JToken.Parse("{}")
                : JToken.FromObject(currentDocument.Current);

            JToken restoredDocument = jdp.Unpatch(
                jtokenCurrentDoc,
                JToken.Parse(currentDocument.Prev[versionCounter - 2].Diff));
            string strType = currentDocument.Prev[versionCounter - 2].Type;
            versionCounter--;

            while (versionCounter > version)
            {
                restoredDocument = jdp.Unpatch(
                    restoredDocument,
                    JToken.Parse(currentDocument.Prev[versionCounter - 2].Diff));
                strType = currentDocument.Prev[versionCounter - 2].Type;
                versionCounter--;
            }

            // Can use this to know if a document is deleted.
            if (!restoredDocument.HasValues)
            {
                return null;
            }

            // Or you can use this to know if a document is deleted.
            //if (versionCounter >= 2 && currentDocument.Prev[versionCounter - 2].IsDeleted)
            //{
            //    return null;
            //}
            Type type = Type.GetType(strType);
            T restoredObject = restoredDocument.ToObject(type) as T;
            return restoredObject;
        }
    }
}
