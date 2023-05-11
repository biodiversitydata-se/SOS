using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Harvest.Harvesters.Interfaces
{
    /// <summary>
    ///     Interface for harvest projects.
    /// </summary>
    public interface IProjectHarvester
    {
        public Task<HarvestInfo> HarvestProjectsAsync();
    }
}