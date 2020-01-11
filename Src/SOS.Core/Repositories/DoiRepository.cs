using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SOS.Core.Models.DOI;

namespace SOS.Core.Repositories
{
    public class DoiRepository : IDoiRepository
    {
        private readonly MongoDbContext _dbContext;
        public IMongoCollection<DoiInfo> Collection { get; private set; }


        public DoiRepository(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
            Collection = _dbContext.MongoDbCollection<DoiInfo>();
        }

        public async Task InsertDoiDocumentAsync(DoiInfo doiInfo)
        {
            await Collection.InsertOneAsync(doiInfo);
        }

        public async Task<DoiInfo> GetDocumentAsync(string doiId)
        {
            return await Collection.FindAsync(x => x.DoiId == doiId).Result.FirstOrDefaultAsync();
        }

        public async Task<ObjectId> InsertDoiFileAsync(byte[] doiFile, string doiId, string doiFilename)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    { "doiId", doiId }
                }
            };

            var id = await DoiGridFsBucket.UploadFromBytesAsync(doiFilename, doiFile, options);
            return id;
        }

        public async Task<byte[]> GetDoiFileByNameAsync(string fileName)
        {
            var bytes = await DoiGridFsBucket.DownloadAsBytesByNameAsync(fileName);
            return bytes;
        }

        public async Task<byte[]> GetDoiFileAsync(ObjectId id)
        {
            var bytes = await DoiGridFsBucket.DownloadAsBytesAsync(id);
            return bytes;
        }

        private GridFSBucket DoiGridFsBucket
        {
            get
            {
                return new GridFSBucket(_dbContext.Mongodb, new GridFSBucketOptions
                {
                    BucketName = Constants.GridFsDoiBucketName
                });
            }
        }

        public async Task DropDoiFileBucketAsync()
        {
            await DoiGridFsBucket.DropAsync();
        }

        public async Task DropDoiCollectionAsync()
        {
            await _dbContext.Mongodb.DropCollectionAsync(Constants.DoiCollectionName);

        }
    }
}
