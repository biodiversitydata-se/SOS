using SOS.Lib.Models.Processed.ProcessInfo;

namespace SOS.Lib.Repositories.Processed.Interfaces;

/// <summary>
///     Processed data class
/// </summary>
public interface IProcessInfoRepository : IMongoDbProcessedRepositoryBase<ProcessInfo, string>
{

}