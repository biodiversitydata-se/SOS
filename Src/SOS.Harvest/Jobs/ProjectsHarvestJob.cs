using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Jobs
{
    /// <summary>
    ///     Projects harvest job.
    /// </summary>
    public class ProjectsHarvestJob : IProjectsHarvestJob
    {

        private readonly IProjectHarvester _projectHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<ProjectsHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="vocabularyHarvester"></param>
        /// <param name="projectHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public ProjectsHarvestJob(
            IProjectHarvester projectHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ProjectsHarvestJob> logger)
        {
            _projectHarvester = projectHarvester ?? throw new ArgumentNullException(nameof(projectHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunHarvestProjectsAsync()
        {
            _logger.LogInformation("Start harvest projects job");

            var result = await _projectHarvester.HarvestProjectsAsync();

            _logger.LogInformation($"End harvest projects job. Result: {result.Status == RunStatus.Success}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status == RunStatus.Success && result.Count > 0
                ? true
                : throw new Exception("Harvest projects job failed");
        }
    }
}