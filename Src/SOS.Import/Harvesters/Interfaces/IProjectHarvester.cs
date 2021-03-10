using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Interfaces
{
    /// <summary>
    ///     Interface for harvest projects.
    /// </summary>
    public interface IProjectHarvester
    {
        public Task<HarvestInfo> HarvestProjectsAsync();
    }
}