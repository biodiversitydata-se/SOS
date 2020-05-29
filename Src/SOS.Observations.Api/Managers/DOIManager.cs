using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.DOI;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Observations.Services.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Area manager
    /// </summary>
    public class DOIManager : IDOIManager
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDOIRepository _doiRepository;
        private readonly ILogger<DOIManager> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="doiRepository"></param>
        /// <param name="blobStorageService"></param>
        /// <param name="logger"></param>
        public DOIManager(
            IDOIRepository doiRepository,
            IBlobStorageService blobStorageService,
            ILogger<DOIManager> logger)
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