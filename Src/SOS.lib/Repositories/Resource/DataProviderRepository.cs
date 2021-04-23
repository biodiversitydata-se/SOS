using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GridFS;
using SOS.Lib.Database.Interfaces;
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

        public async Task<bool> ClearEml()
        {
            try
            {
                await _gridFSBucket.DropAsync(CancellationToken.None);

                return true;
            }
            catch (Exception e)
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
            catch (Exception e)
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
                await _gridFSBucket.UploadFromBytesAsync(GetKey(providerId), await eml.ToBytesAsync());

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}