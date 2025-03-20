using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class VerbatimRepositoryBase<TEntity, TKey> : RepositoryBase<TEntity, TKey>, IVerbatimRepositoryBase<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        private readonly GridFSBucket _gridFSBucket;
        private readonly GridFSBucket _reportBucket;

        private string GetKey(int providerId) => $"source_{providerId.ToString(CultureInfo.InvariantCulture)}";

        /// <summary>
        /// Delete provider source file if any
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        private async Task<bool> DeleteSourceFileAsync(int providerId)
        {
            try
            {
                var fileInfos = await _gridFSBucket.FindAsync(
                    new ExpressionFilterDefinition<GridFSFileInfo>(f => f.Filename.Equals(GetKey(providerId))));

                await fileInfos.ForEachAsync(c =>
                {
                    _gridFSBucket.DeleteAsync(c.Id, CancellationToken.None);
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete report source file if any
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        private async Task<bool> DeleteReportSourceFileAsync(string reportId)
        {
            try
            {
                var fileInfos = await _gridFSBucket.FindAsync(
                    new ExpressionFilterDefinition<GridFSFileInfo>(f => f.Filename.Equals($"source_{reportId}")));

                await fileInfos.ForEachAsync(c =>
                {
                    _gridFSBucket.DeleteAsync(c.Id, CancellationToken.None);
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Store data in temporary collection and switch it on success 
        /// </summary>
        public virtual bool TempMode { get; set; }

        protected virtual string GetCollectionName(bool? tempMode)
        {
            if (tempMode.HasValue)
                return $"{base.CollectionName}{(tempMode.Value ? "_temp" : "")}";
            else
                return $"{base.CollectionName}{(TempMode ? "_temp" : "")}";
        }

        public virtual async Task<bool> AddCollectionAsync(bool? tempMode)
        {
            return await AddCollectionAsync(GetCollectionName(tempMode));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public VerbatimRepositoryBase(
            IVerbatimClient importClient,
            ILogger logger) : base(importClient, logger)
        {
            if (Database != null)
            {
                _gridFSBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = "SourceFile" });
                _reportBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = "Report.SourceFile" });
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="collectionName"></param>
        /// <param name="logger"></param>
        public VerbatimRepositoryBase(
            IVerbatimClient importClient,
            string collectionName,
            ILogger logger) : base(importClient, collectionName, logger)
        {
            if (Database != null)
            {
                _gridFSBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = "SourceFile" });
                _reportBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = "Report.SourceFile" });
            }
        }

        /// <summary>
        /// Name of collection
        /// </summary>
        protected override string CollectionName => $"{base.CollectionName}{(TempMode ? "_temp" : "")}";

        public virtual async Task<bool> PermanentizeCollectionAsync()
        {
            return await PermanentizeCollectionAsync(null);
        }

        /// <inheritdoc />
        public virtual async Task<bool> PermanentizeCollectionAsync(bool? tempMode = null)
        {
            if (!TempMode || !await CheckIfCollectionExistsAsync() || await CountAllDocumentsAsync() == 0)
            {
                return true;
            }

            // Switch off temp mode
            TempMode = false;
            var permanentCollectionName = CollectionName;

            // Check if permanent collection exists
            if (await CheckIfCollectionExistsAsync())
            {
                // Delete permanent collection
                await DeleteCollectionAsync();
            }

            // Re set temp mode
            TempMode = true;

            await Database.RenameCollectionAsync(CollectionName, permanentCollectionName);
            return true;
        }

        public virtual async Task<bool> PermanentizeCollectionAsync(IMongoCollection<TEntity> tempCollection, IMongoCollection<TEntity> targetCollection)
        {
            if (!await CheckIfCollectionExistsAsync(tempCollection.CollectionNamespace.CollectionName)) return false;
            if (await CountAllDocumentsAsync(tempCollection) == 0) return false;
            
            // Check if permanent collection exists
            if (await CheckIfCollectionExistsAsync(targetCollection.CollectionNamespace.CollectionName))
            {
                // Delete permanent collection
                await DeleteCollectionAsync(targetCollection.CollectionNamespace.CollectionName);
            }
            
            await Database.RenameCollectionAsync(tempCollection.CollectionNamespace.CollectionName, targetCollection.CollectionNamespace.CollectionName);
            return true;
        }

        public virtual async Task<bool> PermanentizeCollectionAsync(string tempCollectionName, string targetCollectionName)
        {
            if (!await CheckIfCollectionExistsAsync(tempCollectionName)) return false;
            var tempCollection = GetMongoCollection(tempCollectionName);
            if (await CountAllDocumentsAsync(tempCollection) == 0) return false;

            // Check if permanent collection exists
            if (await CheckIfCollectionExistsAsync(targetCollectionName))
            {
                // Delete permanent collection
                await DeleteCollectionAsync(targetCollectionName);
            }

            await Database.RenameCollectionAsync(tempCollectionName, targetCollectionName);
            return true;
        }

        public async Task<bool> RenameCollectionAsync(string currentCollectionName, string newCollectionName)
        {
            try
            {
                await Database.RenameCollectionAsync(currentCollectionName, newCollectionName);
                return true;
            }
            catch
            {
                return false;
            }
        }       

        public async Task<bool> CopyCollectionAsync(string sourceCollectionName, string targetCollectionName, bool overwriteExistingTargetCollection = true)
        {
            try
            {
                if (!await CheckIfCollectionExistsAsync(sourceCollectionName))
                {
                    return false;
                }
                if (overwriteExistingTargetCollection && await CheckIfCollectionExistsAsync(targetCollectionName))
                {
                    await DeleteCollectionAsync(targetCollectionName);                    
                }

                const int batchSize = 10000;
                var sourceCollection = Database.GetCollection<BsonDocument>(sourceCollectionName);
                var targetCollection = Database.GetCollection<BsonDocument>(targetCollectionName);
                var options = new FindOptions<BsonDocument> { BatchSize = batchSize };
                using (var cursor = await sourceCollection.FindAsync(new BsonDocument(), options))
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current.ToList();
                        if (batch.Count > 0)
                        {
                            await targetCollection.InsertManyAsync(batch);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error copying collection: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public virtual async Task<Stream> GetSourceFileAsync(int providerId)
        {
            try
            {
                var filter = Builders<GridFSFileInfo>
                    .Filter
                    .Eq(f => f.Filename, GetKey(providerId));

                ObjectId id;
                using (var cursor = await _gridFSBucket.FindAsync(filter))
                {
                    id = cursor.ToList().Select(f => f.Id).FirstOrDefault();
                }

                var fileStream = new MemoryStream();
                await _gridFSBucket.DownloadToStreamAsync(id, fileStream, new GridFSDownloadByNameOptions { Seekable = true }, CancellationToken.None);
                fileStream.Position = 0;
                return fileStream;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public virtual async Task<Stream> GetReportSourceFileAsync(string reportId)
        {
            try
            {
                var filter = Builders<GridFSFileInfo>
                    .Filter
                    .Eq(f => f.Filename, $"source_{reportId}");

                ObjectId id;
                using (var cursor = await _reportBucket.FindAsync(filter))
                {
                    id = cursor.ToList().Select(f => f.Id).FirstOrDefault();
                }

                var fileStream = new MemoryStream();
                await _reportBucket.DownloadToStreamAsync(id, fileStream, new GridFSDownloadByNameOptions { Seekable = true }, CancellationToken.None);
                fileStream.Position = 0;
                return fileStream;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> StoreSourceFileAsync(int providerId, Stream fileStream)
        {
            if (fileStream == null)
            {
                Logger.LogWarning("StoreSourceFileAsync failed. Filestream is null.");
                return false;
            }

            try
            {
                // Make sure no other file for this provider exists
                await DeleteSourceFileAsync(providerId);

                await _gridFSBucket.UploadFromStreamAsync(GetKey(providerId), fileStream);

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "StoreSourceFileAsync failed");
                return false;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> StoreReportSourceFileAsync(string reportId, Stream fileStream)
        {
            if (fileStream == null)
            {
                return false;
            }

            try
            {
                // Make sure no other file for this report exists
                await DeleteReportSourceFileAsync(reportId);

                await _reportBucket.UploadFromStreamAsync($"source_{reportId}", fileStream);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CheckDuplicatesAsync(string field, IMongoCollection<TEntity> mongoCollection)
        {
            try
            {
                var pipeline = new[]
                {
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", $"${field}" },
                        { "count", new BsonDocument("$sum", 1) }
                    }),
                    new BsonDocument("$match", new BsonDocument
                    {
                        { "count", new BsonDocument("$gt", 1) }
                    })
                };

                var result = await mongoCollection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();

                return result != null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error checking duplicates for field {field}: {ex.Message}");
                throw;
            }
        }
    }
}