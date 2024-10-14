using SOS.Lib.Models.Export;


namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IUserExportRepository : IMongoDbProcessedRepositoryBase<UserExport, int>
    {
    }
}