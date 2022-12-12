using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    /// Data provider repository
    /// </summary>
    public class DataProviderRepository : RepositoryBase<DataProvider, int>, IDataProviderRepository
    {
        private readonly GridFSBucket _gridFSBucket;

        private string GetKey(int providerId) => $"eml_{providerId}.xml";

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public DataProviderRepository(
            IProcessClient processClient,
            ILogger<DataProviderRepository> logger) : base(processClient, logger)
        {
            if (Database != null)
            {
                _gridFSBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = nameof(DataProvider) });
            }
        }

        /// <inheritdoc />
        public override async Task<List<DataProvider>> GetAllAsync()
        {
            var allDataProviders = await base.GetAllAsync();            
            return allDataProviders.OrderBy(provider => provider.Id).ToList();
        }

        /// <inheritdoc />
        public async Task<bool> ClearEmlAsync()
        {
            try
            {
                await _gridFSBucket.DropAsync(CancellationToken.None);

                return true;
            }
            catch 
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteEmlAsync(int providerId)
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

        /// <inheritdoc />
        public async Task<XDocument> GetEmlAsync(int providerId)
        {
            try
            {
                var bytes = await _gridFSBucket.DownloadAsBytesByNameAsync(GetKey(providerId), new GridFSDownloadByNameOptions(), CancellationToken.None);

                return await bytes?.ToXmlAsync();
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> StoreEmlAsync(int providerId, XDocument eml)
        {
            if (eml == null)
            {
                return false;
            }

            try
            {
                // Make sure no other file for this provider exists
                await DeleteEmlAsync(providerId);

                await _gridFSBucket.UploadFromBytesAsync(GetKey(providerId), await eml.ToBytesAsync());

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}