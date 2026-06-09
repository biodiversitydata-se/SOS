using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Aqua;
using SOS.Harvest.Repositories.Source.Aqua.Interfaces;
using SOS.Harvest.Services;
using SOS.Lib.Configuration.Import;

namespace SOS.Harvest.Repositories.Source.Aqua;

public class NorsRepository : AquaBaseRepository, INorsRepository
{
    private const string EmofViewName = "mart.NORS_SBDIemof_VIEW";
    private const string EventViewName = "mart.NORS_SBDIevent_VIEW";
    private const string OccurrenceViewName = "mart.NORS_SBDIoccurence_VIEW";

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="dataService"></param>
    /// <param name="logger"></param>
    public NorsRepository(NorsServiceConfiguration configuration, ILogger<NorsRepository> logger) :
        base(new AquaDataService(configuration, logger), logger)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SbdiEmof>?> GetEmofsChunkAsync(int offset, int take)
    {
        return await GetEmofsChunkAsync(EmofViewName, offset, take);
    }

    public async Task<IEnumerable<SbdiEvent>?> GetEventsChunkAsync(int offset, int take)
    {
        return await GetEventsChunkAsync(EventViewName, offset, take);
        
    }

    public async Task<IEnumerable<SbdiOccurrence>?> GetOccurrencesChunkAsync(int offset, int take)
    {
        return await GetOccurrencesChunkAsync(OccurrenceViewName, offset, take);
    }
}