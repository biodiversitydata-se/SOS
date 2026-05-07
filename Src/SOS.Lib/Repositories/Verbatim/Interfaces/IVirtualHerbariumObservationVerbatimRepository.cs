using SOS.Lib.Models.Verbatim.VirtualHerbarium;

namespace SOS.Lib.Repositories.Verbatim.Interfaces;

/// <summary>
///     Repository for Virtual Herbarium observations (New NDJSON format)
/// </summary>
public interface IVirtualHerbariumObservationVerbatimRepository : IVerbatimRepositoryBase<VirtualHerbariumObservationVerbatim, int>
{
}
