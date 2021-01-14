using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Shared;
using GridFSFileInfo = MongoDB.Driver.GridFS.GridFSFileInfo;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Report repository
    /// </summary>
    public class ReportRepository : RepositoryBase<Report, string>, Interfaces.IReportRepository
    {
        private readonly GridFSBucket _gridFsBucket;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public ReportRepository(
            IProcessClient processClient,
            ILogger<ReportRepository> logger) : base(processClient, logger)
        {
            if (Database != null)
            {
                _gridFsBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = nameof(Report) });
            }
        }

        public async Task<bool> StoreFileAsync(string filename, byte[] file)
        {
            await _gridFsBucket.UploadFromBytesAsync(filename, file);
            return true;
        }

        public async Task<bool> DeleteFileAsync(string filename)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, filename);
            var fileInfo = (await (await _gridFsBucket.FindAsync(filter)).ToListAsync()).FirstOrDefault();
            if (fileInfo != null)
            {
                await _gridFsBucket.DeleteAsync(fileInfo.Id);
                return true;
            }
            
            return false;
        }

        public async Task<bool> DeleteFilesAsync(IEnumerable<string> filenames)
        {
            var filter = Builders<GridFSFileInfo>.Filter.In(x => x.Filename, filenames);
            var fileInfos = await (await _gridFsBucket.FindAsync(filter)).ToListAsync();
            foreach (var fileInfo in fileInfos)
            {
                await _gridFsBucket.DeleteAsync(fileInfo.Id);
            }
            
            return true;
        }

        public async Task<byte[]> GetFileAsync(string filename)
        {
            var bytes = await _gridFsBucket.DownloadAsBytesByNameAsync(filename);
            return bytes;
        }
    }
}