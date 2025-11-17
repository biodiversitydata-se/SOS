using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Import;

public class iNaturalistServiceConfiguration : RestServiceConfiguration
{
    public string DatasetKey { get; set; }

    /// <summary>
    ///     The year to start harvest from.
    /// </summary>
    public int StartHarvestYear { get; set; } = 2014;

    public long HarvestCompleteStartId { get; set; } = 200_000_000; //236_000_000;

    public int HarvestCompleBatchDelayInSeconds { get; set; } = 2;
}