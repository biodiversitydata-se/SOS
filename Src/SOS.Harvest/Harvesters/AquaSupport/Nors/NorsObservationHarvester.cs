using DnsClient.Internal;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Aqua;
using SOS.Harvest.Harvesters.AquaSupport.Nors.Interfaces;
using SOS.Harvest.Repositories.Source.Aqua.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim;
using System.Text;

namespace SOS.Harvest.Harvesters.AquaSupport.Nors;

public class NorsObservationHarvester : INorsObservationHarvester
{
    private readonly IVerbatimClient _verbatimClient;
    private readonly INorsRepository _norsRepository;
    private readonly ILogger<NorsObservationHarvester> _logger;
    private readonly NorsServiceConfiguration _norsServiceConfiguration;

    private async Task<IDictionary<string, ICollection<DwcMeasurementOrFact>>> GetMeasurementOrFacts(IJobCancellationToken cancellationToken)
    {
        var offset = 0;
        var take = 10000;
       
        var occurrenceMeasurementOrFacts = new Dictionary<string, ICollection<DwcMeasurementOrFact>>();
        var emofBatch = await _norsRepository.GetEmofsChunkAsync(offset, take);

        while (emofBatch?.Any() ?? false)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var verbatimBatch = new List<DwcEventOccurrenceVerbatim>();
            foreach (var emof in emofBatch)
            {
                if (!occurrenceMeasurementOrFacts.TryGetValue(emof.OccurrenceID, out var measurementOrFacts))
                {
                    measurementOrFacts = new List<DwcMeasurementOrFact>();
                    occurrenceMeasurementOrFacts.Add(emof.OccurrenceID, measurementOrFacts);
                }
                measurementOrFacts.Add(new DwcMeasurementOrFact
                {
                    MeasurementType = emof.MeasurementType,
                    MeasurementValue = emof.MeasurementValue,
                    MeasurementUnit = emof.MeasurementUnit,
                    MeasurementRemarks = emof.MeasurementRemarks
                });
            }
            
            offset += take;
            emofBatch = await _norsRepository.GetEmofsChunkAsync(offset, take);
        }
        return occurrenceMeasurementOrFacts;
    }

    private string GetNorsHarvestSettingsInfoString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("NORS Harvest settings:");
        sb.AppendLine($"  Start Harvest Year: {_norsServiceConfiguration.StartHarvestYear}");
        if (_norsServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue)
        {
            sb.AppendLine(
                $"  Max Number Of Sightings Harvested: {_norsServiceConfiguration.MaxNumberOfSightingsHarvested}");
        }
        return sb.ToString();
    }

    private async Task<IDictionary<string, SbdiEvent>> PopulateEventsAsync(DwcCollectionRepository dwcCollectionRepository, IJobCancellationToken cancellationToken)
    {
        await dwcCollectionRepository.EventRepository.AddCollectionAsync();
        var offset = 0;
        var take = 10000;
        var index = 0;
        var events = new Dictionary<string, SbdiEvent>();
        var eventBatch = await _norsRepository.GetEventsChunkAsync(offset, take);

        while (eventBatch?.Any() ?? false)
        {
            var verbatimBatch = new List<DwcEventOccurrenceVerbatim>();
            foreach (var evnt in eventBatch)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                index++;
                events.Add(evnt.EventID, evnt);
                verbatimBatch.Add(new DwcEventOccurrenceVerbatim
                {
                    Id = index,
                    AccessRights = evnt.AccessRights,
                    DecimalLatitude = evnt.DecimalLatitude,
                    DecimalLongitude = evnt.DecimalLongitude,
                    EventDate = evnt.EventDate,
                    EventID = evnt.EventID,
                    EventRemarks = evnt.EventRemarks,
                    GeodeticDatum = evnt.GeodeticDatum,
                    InformationWithheld = evnt.InformationWithheld,
                    Locality = evnt.Locality,
                    LocationID = evnt.LocationID,
                    LocationRemarks = evnt.LocationRemarks,
                    MaximumDepthInMeters = evnt.MaximumDepthInMeters,
                    MinimumDepthInMeters = evnt.MinimumDepthInMeters,
                    SamplingEffort = evnt.SamplingEffort,
                    SamplingProtocol = evnt.SamplingProtocol
                });
            }
            await dwcCollectionRepository.EventRepository.AddManyAsync(verbatimBatch);

            offset += take;
            eventBatch = await _norsRepository.GetEventsChunkAsync(offset, take);
        }

        return events;
    }

    private async Task<int> PopulateOccurrencesAsync(DwcCollectionRepository dwcCollectionRepository, IDictionary<string, SbdiEvent> events, IJobCancellationToken cancellationToken)
    {
        await dwcCollectionRepository.OccurrenceRepository.AddCollectionAsync();
        var observationCount = 0;
        var offset = 0;
        var take = 10000;
        var occurrenceMeasurementOrFacts = await GetMeasurementOrFacts(cancellationToken);
        var index = 0;
        var occurrenceBatch = await _norsRepository.GetOccurrencesChunkAsync(offset, take);

        while (occurrenceBatch?.Any() ?? false)
        {
            var verbatimBatch = new List<DwcObservationVerbatim>();
            foreach (var occurrence in occurrenceBatch)
            {
                cancellationToken?.ThrowIfCancellationRequested();
                if (_norsServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                    observationCount >= _norsServiceConfiguration.MaxNumberOfSightingsHarvested)
                {
                    _logger.LogInformation("Max observations for NORS reached");
                    break;
                }
                index++;
                events.TryGetValue(occurrence.EventID, out var evnt);
                verbatimBatch.Add(new DwcObservationVerbatim
                {
                    AccessRights = evnt?.AccessRights,
                    Id = index,
                    BasisOfRecord = occurrence.BasisOfRecord,
                    DecimalLatitude = evnt?.DecimalLatitude,
                    DecimalLongitude = evnt?.DecimalLongitude,
                    EventDate = evnt?.EventDate,
                    EventID = occurrence.EventID,
                    EventRemarks = evnt?.EventRemarks,
                    GeodeticDatum = evnt?.GeodeticDatum,
                    InformationWithheld = occurrence.InformationWithheld,
                    Locality = evnt?.Locality,
                    LocationID = evnt?.LocationID,
                    LocationRemarks = evnt?.LocationRemarks,
                    MaximumDepthInMeters = evnt?.MaximumDepthInMeters,
                    MinimumDepthInMeters = evnt?.MinimumDepthInMeters,
                    OccurrenceID = occurrence.OccurrenceID,
                    OccurrenceRemarks = occurrence.OccurrenceRemarks,
                    OccurrenceStatus = occurrence.OccurrenceStatus,
                    OrganismQuantity = occurrence.OrganismQuantity,
                    OrganismQuantityType = occurrence.OrganismQuantityType,
                    ObservationMeasurementOrFacts = occurrenceMeasurementOrFacts.TryGetValue(occurrence.OccurrenceID, out var measurementOrFacts) ? measurementOrFacts : null,
                    RecordedBy = occurrence.RecordedBy,
                    SamplingEffort = evnt?.SamplingEffort,
                    SamplingProtocol = evnt?.SamplingProtocol,
                    ScientificName = occurrence.ScientificName,
                    TaxonID = occurrence.TaxonID
                });

                observationCount++;
            }
            await dwcCollectionRepository.OccurrenceRepository.AddManyAsync(verbatimBatch);

            offset += take;
            occurrenceBatch = await _norsRepository.GetOccurrencesChunkAsync(offset, take);
        }

        return observationCount;
    }

    /// <summary>
    /// Initializes a new instance of the NorsObservationHarvester class for harvesting NORS observations using the
    /// specified repositories, configuration, and logger.
    /// </summary>
    /// <param name="verbatimClient">The client used to retrieve verbatim observation data from external sources.</param>
    /// <param name="eventRepository">The repository used to manage event-related data associated with observations.</param>
    /// <param name="emofRepository">The repository for handling EMOF (Extension Metadata Object Framework) data related to observations.</param>
    /// <param name="occurrenceRepository">The repository responsible for managing occurrence records linked to observations.</param>
    /// <param name="norsServiceConfiguration">The configuration settings for the NORS observation harvesting service. Cannot be null.</param>
    /// <param name="logger">The logger used to record diagnostic and operational information for the harvester.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters verbatimClient, norsObservationVerbatimRepository, eventRepository,
    /// emofRepository, occurrenceRepository, or norsServiceConfiguration is null.</exception>
    public NorsObservationHarvester(
        IVerbatimClient verbatimClient,
        INorsRepository norsRepository,
        NorsServiceConfiguration norsServiceConfiguration,
        ILogger<NorsObservationHarvester> logger)
    {
        _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
        _norsServiceConfiguration = norsServiceConfiguration ??
                                    throw new ArgumentNullException(nameof(norsServiceConfiguration));
        _norsRepository = norsRepository ?? throw new ArgumentNullException(nameof(norsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        
        var harvestInfo = new HarvestInfo(provider.Identifier, DateTime.Now){
            Status = RunStatus.Success
        };
        
        try
        {
            _logger.LogInformation(GetNorsHarvestSettingsInfoString());

            var dwcCollectionRepository = new DwcCollectionRepository(provider, _verbatimClient, _logger);
            dwcCollectionRepository.BeginTempMode();
            _logger.LogDebug("Clear DwC-A observations for {@dataProvider}", provider.Identifier);
            await dwcCollectionRepository.DeleteCollectionsAsync();

            _logger.LogDebug("Start storing DwC-A observations for {@dataProvider}", provider.Identifier);
            var events = await PopulateEventsAsync(dwcCollectionRepository, cancellationToken);
            harvestInfo.Count = await PopulateOccurrencesAsync(dwcCollectionRepository, events, cancellationToken);
            
            await dwcCollectionRepository.PermanentizeCollectionsAsync();
            dwcCollectionRepository.EndTempMode();
            
        }
        catch (JobAbortedException)
        {
            _logger.LogInformation("NORS harvest was cancelled.");
            harvestInfo.Status = RunStatus.Canceled;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to harvest NORS");
            harvestInfo.Status = RunStatus.Failed;
        }
        harvestInfo.End = DateTime.Now;

        return harvestInfo;
    }

    public Task<HarvestInfo> HarvestAllObservationsSlowlyAsync(DataProvider provider, IJobCancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}