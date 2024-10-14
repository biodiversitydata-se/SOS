using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Project repository.
    /// </summary>
    public class ArtportalenDatasetMetadataRepository : RepositoryBase<ArtportalenDatasetMetadata, int>, IArtportalenDatasetMetadataRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public ArtportalenDatasetMetadataRepository(
            IProcessClient processClient,
            ILogger<ArtportalenDatasetMetadataRepository> logger) : base(processClient, logger)
        {
        }
    }
}