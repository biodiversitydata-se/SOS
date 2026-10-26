using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Skogsskada;
using SOS.Harvest.Harvesters.Skogsskada.Interfaces;
using SOS.Harvest.Repositories.Source.Skogsskada.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Models.Verbatim.Skogsskada;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Harvesters.Skogsskada;

/// <summary>
///     Skogsskada observation harvester
/// </summary>
public class SkogsskadaObservationHarvester : ObservationHarvesterBase<SkogsskadaObservationVerbatim, int>, ISkogsskadaObservationHarvester
{
    private readonly ISkogsskadaRepository _skogsskadaRepository;
    private readonly SkogsskadaConfiguration _skogsskadaConfiguration;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="skogsskadaRepository"></param>
    /// <param name="skogsskadaVerbatimRepository"></param>
    /// <param name="skogsskadaConfiguration"></param>
    /// <param name="logger"></param>
    public SkogsskadaObservationHarvester(
        ISkogsskadaRepository skogsskadaRepository,
        ISkogsskadaObservationVerbatimRepository skogsskadaVerbatimRepository,
        SkogsskadaConfiguration skogsskadaConfiguration,
        ILogger<SkogsskadaObservationHarvester> logger) : base("Skogsskada", skogsskadaVerbatimRepository, logger)
    {
        _skogsskadaRepository = skogsskadaRepository ??
                                throw new ArgumentNullException(nameof(skogsskadaRepository));
        _skogsskadaConfiguration = skogsskadaConfiguration ??
                                   throw new ArgumentNullException(nameof(skogsskadaConfiguration));
    }

    /// <summary>
    /// Cast a sosview entity to a Skogsskada verbatim observation.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private static SkogsskadaObservationVerbatim? CastEntityToVerbatim(SkogsskadaEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new SkogsskadaObservationVerbatim
        {
            occurrenceID = entity.occurrenceID,
            basisOfRecord = entity.basisOfRecord,
            eventDate = entity.eventDate,
            scientificName = entity.scientificName,
            identificationVerificationStatus = entity.identificationVerificationStatus,
            decimalLatitude = entity.decimalLatitude,
            decimalLongitude = entity.decimalLongitude,
            geodeticDatum = entity.geodeticDatum,
            footprintWKT = entity.footprintWKT,
            footprintSRS = entity.footprintSRS,
            ownerInstitutionCode = entity.ownerInstitutionCode,
            institutionCode = entity.institutionCode,
            occurrenceRemarks = entity.occurrenceRemarks,
            verbatimCoordinates = entity.verbatimCoordinates,
            verbatimSRS = entity.verbatimSRS,
            country = entity.country,
            countryCode = entity.countryCode,
            recordedBy = entity.recordedBy,
            rightsHolder = entity.rightsHolder,
            accessRights = entity.accessRights,
            samplingProtocol = entity.samplingProtocol
        };
    }

    /// inheritdoc />
    public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider dataProvider,
        JobRunModes mode,
        DateTime? fromDate, IJobCancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            throw new NotImplementedException("Not implemented for this provider");
        });
        return null!;
    }

    /// inheritdoc />
    public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
    {
        var runStatus = RunStatus.Success;
        var harvestCount = 0;
        (DateTime startDate, long preHarvestCount) initValues = (DateTime.Now, 0);
        try
        {
            initValues.preHarvestCount = await InitializeHarvestAsync(true);

            var id = 1;
            string? afterOccurrenceId = null;
            var batchIndex = 0;

            Logger.LogDebug("Start getting Skogsskada observations");

            var batch = (await _skogsskadaRepository.GetChunkAsync(afterOccurrenceId, _skogsskadaConfiguration.ChunkSize))?.ToArray();
            while (batch?.Any() ?? false)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                batchIndex++;

                var verbatimBatch = new List<SkogsskadaObservationVerbatim>();
                foreach (var entity in batch)
                {
                    var verbatim = CastEntityToVerbatim(entity);
                    verbatim.Id = id++;
                    verbatimBatch.Add(verbatim);

                    afterOccurrenceId = verbatim.occurrenceID;
                }

                if (verbatimBatch.Any())
                {
                    await VerbatimRepository.AddManyAsync(verbatimBatch);
                    harvestCount += verbatimBatch.Count;
                }

                batch = (await _skogsskadaRepository.GetChunkAsync(afterOccurrenceId, _skogsskadaConfiguration.ChunkSize))?.ToArray();
            }
        }
        catch (JobAbortedException)
        {
            Logger.LogWarning("Skogsskada harvest was cancelled");
            runStatus = RunStatus.Canceled;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to harvest Skogsskada");
            runStatus = RunStatus.Failed;
        }

        return await FinishHarvestAsync(initValues, runStatus, harvestCount,
            provider.PreviousProcessLimit.GetValueOrDefault(80));
    }

    /// inheritdoc />
    public Task<HarvestInfo> HarvestAllObservationsSlowlyAsync(DataProvider provider, IJobCancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
