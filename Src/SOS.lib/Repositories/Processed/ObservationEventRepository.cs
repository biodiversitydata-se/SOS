using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed.Interfaces;
using static SOS.Lib.Models.Processed.DataStewardship.Dataset.ObservationDataset;
using static SOS.Lib.Models.Processed.DataStewardship.Event.ObservationEvent;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Observation event repository.
    /// </summary>
    public class ObservationEventRepository : ProcessRepositoryBase<ObservationEvent, string>,
        IObservationEventRepository
    {
        /// <summary>
        /// Add the collection
        /// </summary>
        /// <returns></returns>
        private async Task<bool> AddCollectionAsync()
        {
            var createIndexResponse = await Client.Indices.CreateAsync(IndexName, s => s
                .Settings(s => s
                    .NumberOfShards(NumberOfShards)
                    .NumberOfReplicas(NumberOfReplicas)
                    .Setting("max_terms_count", 110000)
                    .Setting(UpdatableIndexSettings.MaxResultWindow, 100000)
                )
                .Map<ObservationEvent>(m => m
                    .AutoMap<ObservationEvent>()
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.Id)
                        .KeyWordLowerCase(kwlc => kwlc.EventId)
                        .KeyWordLowerCase(kwlc => kwlc.ParentEventId, false)
                        .KeyWordLowerCase(kwlc => kwlc.EventType, false)
                        .KeyWordLowerCase(kwlc => kwlc.SamplingProtocol, false)
                        .KeyWordLowerCase(kwlc => kwlc.SamplingEffort, false)
                        .KeyWordLowerCase(kwlc => kwlc.SampleSizeValue, false)
                        .KeyWordLowerCase(kwlc => kwlc.SampleSizeUnit, false)
                        .KeyWordLowerCase(kwlc => kwlc.Habitat, false)
                        .Text(t => t
                            .Name(nm => nm.EventRemarks)
                            .IndexOptions(IndexOptions.Docs)
                        )
                        .Object<Location>(l => l
                            .AutoMap()
                            .Name(nm => nm.Location)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<EventDataset>(l => l
                            .AutoMap()
                            .Name(nm => nm.Dataset)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.Identifier)
                                .KeyWordLowerCase(kwlc => kwlc.Title)
                            )
                        )
                        .Object<VocabularyValue>(t => t
                            .Name(nm => nm.DiscoveryMethod)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.Value)
                                .Number(nr => nr
                                    .Name(nm => nm.Id)
                                    .Type(NumberType.Integer)
                                )
                            )
                        )
                        .Nested<ExtendedMeasurementOrFact>(n => n
                            .AutoMap()
                            .Name(nm => nm.MeasurementOrFacts)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.OccurrenceID)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementRemarks, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementAccuracy, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementDeterminedBy, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementDeterminedDate, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementID, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementMethod, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementType, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementTypeID, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementUnit, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementUnitID, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementValue, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementValueID, false)
                            )
                        )
                        .Nested<Multimedia>(n => n
                            .AutoMap()
                            .Name(nm => nm.Media)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.Description, false)
                                .KeyWordLowerCase(kwlc => kwlc.Audience, false)
                                .KeyWordLowerCase(kwlc => kwlc.Contributor, false)
                                .KeyWordLowerCase(kwlc => kwlc.Created, false)
                                .KeyWordLowerCase(kwlc => kwlc.Creator, false)
                                .KeyWordLowerCase(kwlc => kwlc.DatasetID, false)
                                .KeyWordLowerCase(kwlc => kwlc.Format, false)
                                .KeyWordLowerCase(kwlc => kwlc.Identifier, false)
                                .KeyWordLowerCase(kwlc => kwlc.License, false)
                                .KeyWordLowerCase(kwlc => kwlc.Publisher, false)
                                .KeyWordLowerCase(kwlc => kwlc.References, false)
                                .KeyWordLowerCase(kwlc => kwlc.RightsHolder, false)
                                .KeyWordLowerCase(kwlc => kwlc.Source, false)
                                .KeyWordLowerCase(kwlc => kwlc.Title, false)
                                .KeyWordLowerCase(kwlc => kwlc.Type, false)
                            )
                        )
                        .Object<Organisation>(t => t
                            .AutoMap()
                            .Name(nm => nm.RecorderOrganisation)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationCode, false)
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationID, false)
                            )
                        )
                        .Object<WeatherVariable>(t => t
                            .AutoMap()
                            .Name(nm => nm.Weather)
                        )                        
                    )
                )
            );
            
            return createIndexResponse.Acknowledged && createIndexResponse.IsValid ? true : throw new Exception($"Failed to create ObservationEvent index. Error: {createIndexResponse.DebugInformation}");
        }

        public async Task<List<ObservationEvent>> GetEventsByIds(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any()) throw new ArgumentException("ids is empty");

            var query = new List<Func<QueryContainerDescriptor<ObservationEvent>, QueryContainer>>();
            query.TryAddTermsCriteria("eventId", ids);
            var searchResponse = await Client.SearchAsync<ObservationEvent>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)
                    )
                )
                .Size(ids?.Count() ?? 0)
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);
            var events = searchResponse.Documents.ToList();
            return events;
        }

        /// <summary>
        /// Delete collection
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeleteCollectionAsync()
        {
            var res = await Client.Indices.DeleteAsync(IndexName);
            return res.IsValid;
        }

        /// <summary>
        /// Write data to elastic search
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private BulkAllObserver WriteToElastic(IEnumerable<ObservationEvent> items)
        {
            if (!items.Any())
            {
                return null;
            }

            //check
            var currentAllocation = Client.Cat.Allocation();
            if (currentAllocation != null && currentAllocation.IsValid)
            {
                var diskUsageDescription = "Current diskusage in cluster:";
                foreach (var record in currentAllocation.Records)
                {
                    if (int.TryParse(record.DiskPercent, out int percentageUsed))
                    {
                        diskUsageDescription += percentageUsed + "% ";
                        if (percentageUsed > 90)
                        {
                            Logger.LogError($"Disk usage too high in cluster ({percentageUsed}%), aborting indexing");
                            return null;
                        }
                    }
                }

                Logger.LogDebug(diskUsageDescription);
            }

            var count = 0;
            return Client.BulkAll(items, b => b
                    .Index(IndexName)
                    // how long to wait between retries
                    .BackOffTime("30s")
                    // how many retries are attempted if a failure occurs                        .
                    .BackOffRetries(2)
                    // how many concurrent bulk requests to make
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    // number of items per bulk request
                    .Size(WriteBatchSize)
                    .DroppedDocumentCallback((r, o) =>
                    {
                        Logger.LogError($"EventId: {o?.Id}, Error: {r?.Error?.Reason}");
                    })
                )
                .Wait(TimeSpan.FromDays(1),
                    next => { Logger.LogDebug($"Indexing events for search:{count += next.Items.Count}"); });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="logger"></param>
        public ObservationEventRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<ObservationEventRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {
            LiveMode = true;
            _id = nameof(Observation); // The active instance should be the same as the ProcessedObservationRepository which uses the Observation type.
        }

        /// <inheritdoc />
        public async Task<int> AddManyAsync(IEnumerable<ObservationEvent> items)
        {
            // Save valid processed data
            Logger.LogDebug($"Start indexing ObservationEvent batch for searching with {items.Count()} items");
            var indexResult = WriteToElastic(items);
            Logger.LogDebug("Finished indexing ObservationEvent batch for searching");
            if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
            return items.Count();
        }

        public async Task<bool> DeleteAllDocumentsAsync()
        {
            try
            {
                var res = await Client.DeleteByQueryAsync<ObservationEvent>(q => q
                    .Index(IndexName)
                    .Query(q => q.MatchAll())
                );

                return res.IsValid;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ClearCollectionAsync()
        {
            await DeleteCollectionAsync();
            return await AddCollectionAsync();
        }

        /// <inheritdoc />
        public async Task<bool> DisableIndexingAsync()
        {
            var updateSettingsResponse =
                await Client.Indices.UpdateSettingsAsync(IndexName,
                    p => p.IndexSettings(g => g.RefreshInterval(-1)));

            return updateSettingsResponse.Acknowledged && updateSettingsResponse.IsValid;
        }

        /// <inheritdoc />
        public async Task EnableIndexingAsync()
        {
            await Client.Indices.UpdateSettingsAsync(IndexName,
                p => p.IndexSettings(g => g.RefreshInterval(1)));
        }

        /// <inheritdoc />
        public string UniqueIndexName => IndexHelper.GetIndexName<ObservationEvent>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

        /// <inheritdoc />
        public async Task<bool> VerifyCollectionAsync()
        {
            var response = await Client.Indices.ExistsAsync(IndexName);

            if (!response.Exists)
            {
                await AddCollectionAsync();
            }

            return !response.Exists;
        }
    }
}