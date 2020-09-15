using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.DOI;
using SOS.Lib.Models.Misc;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Observations.Services.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Area manager
    /// </summary>
    public class BlobStorageManager : IBlobStorageManager
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDOIRepository _doiRepository;
        private readonly ILogger<BlobStorageManager> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="doiRepository"></param>
        /// <param name="blobStorageService"></param>
        /// <param name="logger"></param>
        public BlobStorageManager(
            IDOIRepository doiRepository,
            IBlobStorageService blobStorageService,
            ILogger<BlobStorageManager> logger)
        {
            _doiRepository = doiRepository ??
                             throw new ArgumentNullException(nameof(doiRepository));
            _blobStorageService = blobStorageService ??
                                  throw new ArgumentNullException(nameof(blobStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public string GetDOIDownloadUrl(Guid id)
        {
            return _blobStorageService.GetDOIDownloadUrl(id);
        }

        /// <inheritdoc />
        public string GetExportDownloadUrl(string fileName)
        {
            return _blobStorageService.GetExportDownloadUrl(fileName);
        }

        /// <inheritdoc />
        public IEnumerable<File> GetExportFiles()
        {
            return _blobStorageService.GetExportFiles();
        }

        /// <inheritdoc />
        public async Task<PagedResult<DOI>> GetDOIsAsync(int skip, int take)
        {
            try
            {
                return await _doiRepository.GetDoisAsync(skip, take);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get paged list of DOIs");
                return null;
            }
        }
    }
}