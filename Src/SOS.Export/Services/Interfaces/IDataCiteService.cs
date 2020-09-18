using System.Threading.Tasks;

namespace SOS.Export.Services.Interfaces
{
    public interface IDataCiteService
    {
        /// <summary>
        /// Create DOI
        /// </summary>
        /// <returns></returns>
        Task<bool> CreateDoiAsync();
    }
}