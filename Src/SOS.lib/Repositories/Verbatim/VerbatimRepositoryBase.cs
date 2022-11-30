using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class VerbatimRepositoryBase<TEntity, TKey> : RepositoryBase<TEntity, TKey>, IVerbatimRepositoryBase<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        private readonly GridFSBucket _gridFSBucket;

        private string GetKey(int providerId) => $"source_{providerId}";

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

                return fileStream;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> StoreSourceFileAsync(int providerId, Stream fileStream)
        {
            if (fileStream == null)
            {
                return false;
            }

            try
            {
                // Make sure no other file for this provider exists
                await DeleteSourceFileAsync(providerId);

                await _gridFSBucket.UploadFromStreamAsync(GetKey(providerId), fileStream);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}