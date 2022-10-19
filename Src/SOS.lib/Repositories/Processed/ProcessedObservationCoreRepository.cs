using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Elasticsearch.Net;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataQuality;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedObservationCoreRepository : ProcessedObservationBaseRepository,
        IProcessedObservationCoreRepository
    {
        private const int ElasticSearchMaxRecords = 10000;
        protected readonly TelemetryClient _telemetry;
        protected readonly ITaxonManager _taxonManager;

        /// <summary>
        /// Add the collection
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private async Task<bool> AddCollectionAsync(bool protectedIndex)
        {
            var createIndexResponse = await Client.Indices.CreateAsync(protectedIndex ? ProtectedIndexName : PublicIndexName, s => s
                .Settings(s => s
                    .NumberOfShards(NumberOfShards)
                    .NumberOfReplicas(NumberOfReplicas)
                    .Setting("max_terms_count", 110000)
                    .Setting(UpdatableIndexSettings.MaxResultWindow, 100000)
                )

                .Map<Observation>(m => m
                    .AutoMap<Observation>()
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.Id, false)
                        .KeyWordLowerCase(kwlc => kwlc.DynamicProperties, false)
                        .KeyWordLowerCase(kwlc => kwlc.InformationWithheld, false)
                        .KeyWordLowerCase(kwlc => kwlc.BibliographicCitation, false)
                        .KeyWordLowerCase(kwlc => kwlc.CollectionId, false)
                        .KeyWordLowerCase(kwlc => kwlc.CollectionCode, false)
                        .KeyWordLowerCase(kwlc => kwlc.DataGeneralizations, false)
                        .KeyWordLowerCase(kwlc => kwlc.DatasetId, false)
                        .KeyWordLowerCase(kwlc => kwlc.DataStewardshipDatasetId)
                        .KeyWordLowerCase(kwlc => kwlc.DatasetName) // WFS
                        .KeyWordLowerCase(kwlc => kwlc.InstitutionId)
                        .KeyWordLowerCase(kwlc => kwlc.Language, false)
                        .KeyWordLowerCase(kwlc => kwlc.License, false)
                        .KeyWordLowerCase(kwlc => kwlc.OwnerInstitutionCode, false)
                        .KeyWordLowerCase(kwlc => kwlc.PrivateCollection)
                        .KeyWordLowerCase(kwlc => kwlc.PublicCollection)
                        .KeyWordLowerCase(kwlc => kwlc.References, false)
                        .KeyWordLowerCase(kwlc => kwlc.RightsHolder, false)
                        .KeyWordLowerCase(kwlc => kwlc.SpeciesCollectionLabel)
                        .Nested<ExtendedMeasurementOrFact>(n => n
                            .Name(nm => nm.MeasurementOrFacts)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementAccuracy, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementDeterminedBy, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementDeterminedDate, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementID, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementMethod, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementRemarks, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementType, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementTypeID, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementUnit, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementUnitID, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementValue, false)
                                .KeyWordLowerCase(kwlc => kwlc.MeasurementValueID, false)
                                .KeyWordLowerCase(kwlc => kwlc.OccurrenceID, false)
                            )
                        )
                        .Object<ProjectsSummary>(t => t
                            .AutoMap()
                            .Name(nm => nm.ProjectsSummary)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.Project1Name) // WFS
                                .KeyWordLowerCase(kwlc => kwlc.Project1Category) // WFS
                                .KeyWordLowerCase(kwlc => kwlc.Project1Url, false)
                                .KeyWordLowerCase(kwlc => kwlc.Project1Values) // WFS
                                .KeyWordLowerCase(kwlc => kwlc.Project2Name) // WFS
                                .KeyWordLowerCase(kwlc => kwlc.Project2Category) // WFS
                                .KeyWordLowerCase(kwlc => kwlc.Project2Url, false)
                                .KeyWordLowerCase(kwlc => kwlc.Project2Values)
                            )
                        )
                        .Nested<Project>(n => n
                            .AutoMap()
                            .Name(nm => nm.Projects)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.AccessRights)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<ArtportalenInternal>(t => t
                            .AutoMap()
                            .Name(nm => nm.ArtportalenInternal)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.LocationExternalId)
                                .KeyWordLowerCase(kwlc => kwlc.LocationPresentationNameParishRegion, false)
                                .KeyWordLowerCase(kwlc => kwlc.ParentLocality)
                                .KeyWordLowerCase(kwlc => kwlc.ReportedByUserAlias)
                                .KeyWordLowerCase(kwlc => kwlc.SightingBarcodeURL)
                                .Nested<UserInternal>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.OccurrenceRecordedByInternal)
                                    .Properties(ps => ps
                                        .KeyWordLowerCase(kwlc => kwlc.UserAlias)
                                    )
                                )
                                .Nested<UserInternal>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.OccurrenceVerifiedByInternal)
                                    .Properties(ps => ps
                                        .KeyWordLowerCase(kwlc => kwlc.UserAlias)
                                    )
                                )
                                .KeyWordLowerCase(kwlc => kwlc.BirdValidationAreaIds)
                            )
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.BasisOfRecord)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<DataQuality>(t => t
                            .AutoMap()
                            .Name(nm => nm.DataQuality)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.UniqueKey, false)
                            )
                        )
                        .Object<IDictionary<string, string>>(c => c
                            .AutoMap()
                            .Name(nm => nm.Defects)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.Keys, false)
                                .KeyWordLowerCase(kwlc => kwlc.Values, false)
                            )
                        )
                        .Object<Event>(t => t
                            .AutoMap()
                            .Name(nm => nm.Event)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<GeologicalContext>(c => c
                            .Name(nm => nm.GeologicalContext)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.Bed, false)
                                .KeyWordLowerCase(kwlc => kwlc.EarliestAgeOrLowestStage, false)
                                .KeyWordLowerCase(kwlc => kwlc.EarliestEonOrLowestEonothem, false)
                                .KeyWordLowerCase(kwlc => kwlc.EarliestEpochOrLowestSeries, false)
                                .KeyWordLowerCase(kwlc => kwlc.EarliestEraOrLowestErathem, false)
                                .KeyWordLowerCase(kwlc => kwlc.EarliestGeochronologicalEra, false)
                                .KeyWordLowerCase(kwlc => kwlc.EarliestPeriodOrLowestSystem, false)
                                .KeyWordLowerCase(kwlc => kwlc.Formation, false)
                                .KeyWordLowerCase(kwlc => kwlc.GeologicalContextId, false)
                                .KeyWordLowerCase(kwlc => kwlc.Group, false)
                                .KeyWordLowerCase(kwlc => kwlc.HighestBiostratigraphicZone, false)
                                .KeyWordLowerCase(kwlc => kwlc.LatestAgeOrHighestStage, false)
                                .KeyWordLowerCase(kwlc => kwlc.LatestEonOrHighestEonothem, false)
                                .KeyWordLowerCase(kwlc => kwlc.LatestEpochOrHighestSeries, false)
                                .KeyWordLowerCase(kwlc => kwlc.LatestEraOrHighestErathem, false)
                                .KeyWordLowerCase(kwlc => kwlc.LatestGeochronologicalEra, false)
                                .KeyWordLowerCase(kwlc => kwlc.LatestPeriodOrHighestSystem, false)
                                .KeyWordLowerCase(kwlc => kwlc.LithostratigraphicTerms, false)
                                .KeyWordLowerCase(kwlc => kwlc.LowestBiostratigraphicZone, false)
                                .KeyWordLowerCase(kwlc => kwlc.Member, false)
                            )
                        )
                        .Object<Identification>(c => c
                            .AutoMap()
                            .Name(nm => nm.Identification)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.IdentificationRemarks, false)
                                .KeyWordLowerCase(kwlc => kwlc.ConfirmedBy, false)
                                .KeyWordLowerCase(kwlc => kwlc.ConfirmedDate, false)
                                .KeyWordLowerCase(kwlc => kwlc.DateIdentified, false)
                                .KeyWordLowerCase(kwlc => kwlc.IdentificationId, false)
                                .KeyWordLowerCase(kwlc => kwlc.IdentificationQualifier, false)
                                .KeyWordLowerCase(kwlc => kwlc.IdentificationReferences, false)
                                .KeyWordLowerCase(kwlc => kwlc.IdentifiedBy, false)
                                .KeyWordLowerCase(kwlc => kwlc.TypeStatus, false)
                                .KeyWordLowerCase(kwlc => kwlc.VerifiedBy)
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.DeterminationMethod)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.ValidationStatus)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.VerificationStatus)
                                    .Properties(ps => ps.GetMapping())
                                )
                            )
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.InstitutionCode)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<Location>(l => l
                            .AutoMap()
                            .Name(nm => nm.Location)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<MaterialSample>(c => c
                            .Name(nm => nm.MaterialSample)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.MaterialSampleId)
                                    .Index(false)
                                )
                            )
                        )
                        .Object<Occurrence>(t => t
                            .AutoMap()
                            .Name(nm => nm.Occurrence)
                            .Properties(ps => ps
                                .Date(d => d
                                    .Name(nm => nm.ReportedDate)
                                )
                                .KeyWordLowerCase(kwlc => kwlc.AssociatedMedia)
                                .KeyWordLowerCase(kwlc => kwlc.AssociatedOccurrences, false)
                                .KeyWordLowerCase(kwlc => kwlc.AssociatedReferences, false)
                                .KeyWordLowerCase(kwlc => kwlc.AssociatedSequences, false)
                                .KeyWordLowerCase(kwlc => kwlc.AssociatedTaxa, false)
                                .KeyWordLowerCase(kwlc => kwlc.BiotopeDescription, false)
                                .KeyWordLowerCase(kwlc => kwlc.IndividualId, false)
                                .KeyWordLowerCase(kwlc => kwlc.RecordedBy)
                                .KeyWordLowerCase(kwlc => kwlc.CatalogNumber)
                                .KeyWordLowerCase(kwlc => kwlc.Disposition, false)
                                .KeyWordLowerCase(kwlc => kwlc.IndividualCount, false)
                                .KeyWordLowerCase(kwlc => kwlc.OccurrenceId)
                                .KeyWordLowerCase(kwlc => kwlc.OccurrenceStatus)
                                .KeyWordLowerCase(kwlc => kwlc.OrganismQuantity)
                                .KeyWordLowerCase(kwlc => kwlc.OtherCatalogNumbers, false)
                                .KeyWordLowerCase(kwlc => kwlc.Preparations, false)
                                .KeyWordLowerCase(kwlc => kwlc.RecordNumber)
                                .KeyWordLowerCase(kwlc => kwlc.ReportedBy)
                                .KeyWordLowerCase(kwlc => kwlc.Url, false)
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
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Activity)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Behavior)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Biotope)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.EstablishmentMeans)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.LifeStage)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.OccurrenceStatus)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.OrganismQuantityUnit)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.ReproductiveCondition)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Sex)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<Substrate>(c => c
                                    .AutoMap()
                                    .Name(nm => nm.Substrate)
                                    .Properties(ps => ps
                                        .KeyWordLowerCase(kwlc => kwlc.SpeciesScientificName, false)
                                        .KeyWordLowerCase(kwlc => kwlc.Description, false)
                                        .KeyWordLowerCase(kwlc => kwlc.SpeciesDescription, false)
                                        .KeyWordLowerCase(kwlc => kwlc.SubstrateDescription, false)
                                        .KeyWordLowerCase(kwlc => kwlc.SpeciesVernacularName, false)
                                        .Object<VocabularyValue>(c => c
                                            .Name(nm => nm.Name)
                                            .Properties(ps => ps.GetMapping())
                                        )
                                    )
                                )
                                .Text(t => t
                                    .Name(nm => nm.OccurrenceRemarks)
                                    .IndexOptions(IndexOptions.Docs)
                                )
                            )
                        )
                        .Object<Organism>(c => c
                            .AutoMap()
                            .Name(nm => nm.Organism)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.AssociatedOrganisms, false)
                                .KeyWordLowerCase(kwlc => kwlc.OrganismId, false)
                                .KeyWordLowerCase(kwlc => kwlc.OrganismName, false)
                                .KeyWordLowerCase(kwlc => kwlc.OrganismRemarks, false)
                                .KeyWordLowerCase(kwlc => kwlc.OrganismScope, false)
                                .KeyWordLowerCase(kwlc => kwlc.PreviousIdentifications, false)
                            )
                        )
                        .Object<Taxon>(t => t
                            .AutoMap()
                            .Name(nm => nm.Taxon)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.Type)
                            .Properties(ps => ps.GetMapping())
                        )
                    )
                )
            );

            return createIndexResponse.Acknowledged && createIndexResponse.IsValid ? true : throw new Exception($"Failed to create observation index. Error: {createIndexResponse.DebugInformation}");
        }

        /// <summary>
        /// Cast dynamic to observation
        /// </summary>
        /// <param name="dynamicObjects"></param>
        /// <returns></returns>
        private List<Observation> CastDynamicsToObservations(IEnumerable<dynamic> dynamicObjects)
        {
            if (dynamicObjects == null) return null;
            return JsonSerializer.Deserialize<List<Observation>>(JsonSerializer.Serialize(dynamicObjects),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        /// <summary>
        /// Make sure no duplicates of occurrence id's exists in index
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private async Task<bool> EnsureNoDuplicates(bool protectedIndex)
        {
            try
            {
                const int maxReturnedItems = 1000;
                var duplicates = await TryToGetOccurenceIdDuplicatesAsync(protectedIndex, maxReturnedItems);

                while (duplicates?.Any() ?? false)
                {
                    var searchResponse = await Client.SearchAsync<dynamic>(s => s
                        .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                        .Query(q => q
                            .Terms(t => t
                                .Field("occurrence.occurrenceId")
                                .Terms(duplicates)
                            )
                        )
                        .Sort(s => s
                            .Ascending("occurrence.occurrenceId")
                            .Descending("modified")
                         )
                        .Size(duplicates.Count() * 3) // It's not likely that average numbers of duplicates exceeds 3
                        .Source(s => s.ExcludeAll())
                        .TrackTotalHits(false)
                    );

                    searchResponse.ThrowIfInvalid();
                    var observations = searchResponse.Documents.Cast<IDictionary<string, object>>().ToArray();
                    var idsToRemove = new HashSet<string>();
                    var prevOccurrenceId = string.Empty;
                    foreach (var hit in searchResponse.Hits)
                    {
                        var occurrenceId = hit.Sorts.First().ToString();
                        // Remove all but first occurrence of occurrenceId (latest data)
                        if (occurrenceId == prevOccurrenceId)
                        {
                            idsToRemove.Add(hit.Id);
                        }
                        prevOccurrenceId = occurrenceId;
                    }
                    await DeleteByIdsAsync(idsToRemove, protectedIndex);
                    duplicates = await TryToGetOccurenceIdDuplicatesAsync(protectedIndex, maxReturnedItems);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get last modified date for provider
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private async Task<DateTime> GetLatestModifiedDateForProviderAsync(int providerId, bool protectedIndex)
        {
            try
            {
                var res = await Client.SearchAsync<Observation>(s => s
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.DataProviderId)
                            .Value(providerId)))
                    .Aggregations(a => a
                        .Max("latestModified", m => m
                            .Field(f => f.Modified)
                        )
                    )
                    .Size(0)
                    .Source(s => s.ExcludeAll())
                    .TrackTotalHits(false)
                );

                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddMilliseconds(res.Aggregations?.Max("latestModified")?.Value ?? 0);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to get last modified date for provider: { providerId }, index: { (protectedIndex ? ProtectedIndexName : PublicIndexName) }");
                return DateTime.MinValue;
            }
        }

        private async Task<ISearchResponse<T>> SearchAfterAsync<T>(
           string searchIndex,
           SearchDescriptor<T> searchDescriptor,
           string pointInTimeId = null,
           IEnumerable<object> searchAfter = null) where T : class
        {
            var keepAlive = "20m";
            if (string.IsNullOrEmpty(pointInTimeId))
            {
                var pitResponse = await Client.OpenPointInTimeAsync(searchIndex, pit => pit
                    .RequestConfiguration(c => c
                        .RequestTimeout(TimeSpan.FromSeconds(30))
                    )
                    .KeepAlive(keepAlive)
                );
                pointInTimeId = pitResponse.Id;
            }

            // Retry policy by Polly
            var searchResponse = await PollyHelper.GetRetryPolicy(3, 100).ExecuteAsync(async () =>
            {
                var queryResponse = await Client.SearchAsync<T>(searchDescriptor
                   .Sort(s => s.Ascending(SortSpecialField.ShardDocumentOrder))
                   .PointInTime(pointInTimeId, pit => pit.KeepAlive(keepAlive))
                   .SearchAfter(searchAfter)
                   .Size(ScrollBatchSize)
                   .TrackTotalHits(false)
                );

                queryResponse.ThrowIfInvalid();

                return queryResponse;
            });

            if (!string.IsNullOrEmpty(pointInTimeId) && (searchResponse?.Hits?.Count ?? 0) == 0)
            {
                await Client.ClosePointInTimeAsync(pitr => pitr.Id(pointInTimeId));
            }

            return searchResponse;
        }

        /// <summary>
        /// Write data to elastic search
        /// </summary>
        /// <param name="items"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private BulkAllObserver WriteToElastic(IEnumerable<Observation> items, bool protectedIndex)
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
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
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
                        if (r.Error != null)
                        {
                            Logger.LogError($"OccurrenceId: {o?.Occurrence?.OccurrenceId}, { r.Error.Reason }");
                        }
                    })
                )
                .Wait(TimeSpan.FromHours(1),
                    next =>
                    {
                        Logger.LogDebug($"Indexing item for search:{count += next.Items.Count}");
                    });
        }

        /// Constructor used in public mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="telemetry"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public ProcessedObservationCoreRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            TelemetryClient telemetry,
            ILogger<ProcessedObservationCoreRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        /// <summary>
        /// Constructor used in admin mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="logger"></param>
        public ProcessedObservationCoreRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<ProcessedObservationCoreRepository> logger) : base(false, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {

        }

        /// <inheritdoc />
        public async Task<int> AddManyAsync(IEnumerable<Observation> items, bool protectedIndex)
        {
            // Save valid processed data
            Logger.LogDebug($"Start indexing batch for searching with {items.Count()} items");
            var indexResult = WriteToElastic(items, protectedIndex);
            Logger.LogDebug("Finished indexing batch for searching");
            if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
            return items.Count();
        }

        /// <inheritdoc />
        public async Task<bool> ClearCollectionAsync(bool protectedIndex)
        {
            await DeleteCollectionAsync(protectedIndex);
            return await AddCollectionAsync(protectedIndex);
        }

        /// <summary>
        /// Delete collection
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        public async Task<bool> DeleteCollectionAsync(bool protectedIndex)
        {
            var res = await Client.Indices.DeleteAsync(protectedIndex ? ProtectedIndexName : PublicIndexName);
            return res.IsValid;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAllDocumentsAsync(bool protectedIndex)
        {
            try
            {
                var res = await Client.DeleteByQueryAsync<Observation>(q => q
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
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
        public async Task<bool> DeleteByOccurrenceIdAsync(IEnumerable<string> occurenceIds, bool protectedIndex)
        {
            try
            {
                // Create the collection
                var res = await Client.DeleteByQueryAsync<Observation>(q => q
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .Terms(t => t
                            .Field(f => f.Occurrence.OccurrenceId)
                            .Terms(occurenceIds)
                        )
                    )
                );

                return res.IsValid;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Delete observations by id's
        /// </summary>
        /// <param name="Ids"></param>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private async Task<bool> DeleteByIdsAsync(IEnumerable<string> Ids, bool protectedIndex)
        {
            try
            {
                // Create the collection
                var res = await Client.DeleteByQueryAsync<Observation>(q => q
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .Terms(t => t
                            .Field("_id")
                            .Terms(Ids)
                        )
                    )
                    .Refresh(true)
                    .WaitForCompletion(true)
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
        public async Task<bool> DeleteProviderDataAsync(DataProvider dataProvider, bool protectedIndex)
        {
            try
            {
                // Create the collection
                var res = await Client.DeleteByQueryAsync<Observation>(q => q
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.DataProviderId)
                            .Value(dataProvider.Id))));

                return res.IsValid;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DisableIndexingAsync(bool protectedIndex)
        {
            var updateSettingsResponse =
                await Client.Indices.UpdateSettingsAsync(protectedIndex ? ProtectedIndexName : PublicIndexName,
                    p => p.IndexSettings(g => g.RefreshInterval(-1)));

            return updateSettingsResponse.Acknowledged && updateSettingsResponse.IsValid;
        }

        /// <inheritdoc />
        public async Task EnableIndexingAsync(bool protectedIndex)
        {
            await Client.Indices.UpdateSettingsAsync(protectedIndex ? ProtectedIndexName : PublicIndexName,
                p => p.IndexSettings(g => g.RefreshInterval(1)));
        }

        /// <inheritdoc />
        public async Task<bool> EnsureNoDuplicatesAsync()
        {
            var tasks = new[] {
                EnsureNoDuplicates(false),
                EnsureNoDuplicates(true)
            };

            await Task.WhenAll(tasks);

            return tasks.All(t => t.Result);
        }

        /// <inheritdoc />
        public async Task<DataQualityReport> GetDataQualityReportAsync(string organismGroup)
        {
            var index = PublicIndexName;

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(index)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f.Term(t => t
                            .Field("taxon.attributes.organismGroup")
                            .Value(organismGroup?.ToLower())))
                    )
                )
                .Aggregations(a => a
                    .Terms("uniqueKeyCount", f => f
                        .Field("dataQuality.uniqueKey")
                        .MinimumDocumentCount(2)
                        .Size(65536)
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            var duplicates = searchResponse
                .Aggregations
                .Terms("uniqueKeyCount")
                .Buckets?
                .Select(b =>
                    new
                    {
                        UniqueKey = b.Key,
                        b.DocCount
                    }).ToArray();

            var report = new DataQualityReport();

            if (duplicates?.Any() ?? false)
            {
                var rowCount = 0;

                foreach (var duplicate in duplicates)
                {
                    searchResponse = await Client.SearchAsync<dynamic>(s => s
                        .Index(index)
                        .Query(q => q
                            .Bool(b => b
                                .Filter(f => f.Term(t => t
                                    .Field("dataQuality.uniqueKey")
                                    .Value(duplicate.UniqueKey)))
                            )
                        )
                        .Sort(sort => sort.Field(f => f.Field("dataProviderId")))
                        .Size(10000)
                        .Source(s => s.Includes(i => i
                            .Field("dataProviderId")
                            .Field("occurrence.occurrenceId")
                            .Field("location.locality")
                            .Field("event.startDate")
                            .Field("event.endDate")
                            .Field("taxon.id")
                            .Field("taxon.scientificName")
                        ))
                        .TrackTotalHits(false)
                    );

                    searchResponse.ThrowIfInvalid();
                    var docCount = searchResponse.Documents.Count;
                    if (docCount == 0)
                    {
                        continue;
                    }

                    if (rowCount + docCount > 2000)
                    {
                        break;
                    }

                    var firstDocument = searchResponse.Documents.Cast<IDictionary<string, dynamic>>().First();
                    var locality = string.Empty;
                    if (firstDocument.TryGetValue(nameof(Observation.Location).ToLower(), out var locationDictionary))
                    {
                        locality = (string)locationDictionary["locality"];
                    }

                    var record = new DataQualityReportRecord
                    {
                        EndDate = firstDocument["event"]["endDate"],
                        Locality = locality,
                        Observations = searchResponse.Documents.Select(d => new DataQualityReportObservation
                        {
                            DataProviderId = d["dataProviderId"].ToString(),
                            OccurrenceId = d["occurrence"]["occurrenceId"],
                        }),
                        StartDate = firstDocument["event"]["startDate"],
                        TaxonId = firstDocument["taxon"]["id"].ToString(),
                        TaxonScientificName = firstDocument["taxon"]["scientificName"],
                        UniqueKey = duplicate.UniqueKey
                    };

                    report.Records.Add(record);
                    rowCount += docCount;
                }
            }

            return report;
        }

        /// <inheritdoc />
        public async Task<GeoGridMetricResult> GetMetricGridAggregationAsync(
            SearchFilter filter,
            int gridCellSizeInMeters)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            using var operation =
                _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_MetricGridAggregation");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Composite("gridCells", c => c
                        .Size(MaxNrElasticSearchAggregationBuckets + 1)
                        .Sources(s => s
                            .Terms("sweref99tm_x", t => t
                                .Script(sct => sct
                                    .Source(
                                        $"(Math.floor(doc['location.sweref99TmX'].value / {gridCellSizeInMeters}) * {gridCellSizeInMeters}).intValue()")
                                )
                            )
                            .Terms("sweref99tm_y", t => t
                                .Script(sct => sct
                                    .Source(
                                        $"(Math.floor(doc['location.sweref99TmY'].value / {gridCellSizeInMeters}) * {gridCellSizeInMeters}).intValue()")
                                )
                            )
                        )
                        .Aggregations(a => a
                            .Cardinality("taxa_count", c => c
                                .Field("taxon.id")
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid)
            {
                if (searchResponse.ServerError?.Error?.CausedBy?.Type == "too_many_buckets_exception")
                {
                    throw new ArgumentOutOfRangeException($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
                }

                searchResponse.ThrowIfInvalid();
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations?.Composite("gridCells")?.Buckets?.Count ?? 0;
            if (nrOfGridCells > MaxNrElasticSearchAggregationBuckets)
            {
                throw new ArgumentOutOfRangeException($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
            }

            _telemetry.StopOperation(operation);

            var gridResult = new GeoGridMetricResult()
            {
                BoundingBox = filter.Location?.Geometries?.BoundingBox,
                GridCellSizeInMeters = gridCellSizeInMeters,
                GridCellCount = nrOfGridCells,
                GridCells = searchResponse.Aggregations.Composite("gridCells").Buckets.Select(b =>
                    new GridCell
                    {
                        Sweref99TmBoundingBox = new XYBoundingBox
                        {
                            BottomRight = new XYCoordinate(double.Parse(b.Key["sweref99tm_x"].ToString()) + gridCellSizeInMeters, double.Parse(b.Key["sweref99tm_y"].ToString())),
                            TopLeft = new XYCoordinate(double.Parse(b.Key["sweref99tm_x"].ToString()), double.Parse(b.Key["sweref99tm_y"].ToString()) + gridCellSizeInMeters)
                        },
                        ObservationsCount = b.DocCount,
                        TaxaCount = (long?)b.Cardinality("taxa_count").Value
                    }
                )
            };

            // When operation is disposed, telemetry item is sent.
            return gridResult;
        }

        /// <inheritdoc />
        public async Task<WaitForStatus> GetHealthStatusAsync(WaitForStatus waitForStatus, int waitForSeconds)
        {
            try
            {
                var response = await Client.Cluster
                        .HealthAsync(new []{ Indices.Index(PublicIndexName), Indices.Index(ProtectedIndexName) }, chr => chr
                            .Level(Level.Indices)
                            .Timeout(TimeSpan.FromSeconds(waitForSeconds))
                            .WaitForStatus(waitForStatus)
                        );

                var healthColor = response.Status.ToString().ToLower();

                return healthColor switch
                {
                    "green" => WaitForStatus.Green,
                    "yellow" => WaitForStatus.Yellow,
                    "red" => WaitForStatus.Red,
                    _ => WaitForStatus.Red
                };
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to get ElasticSearch health", e);
                return WaitForStatus.Red;
            }
        }

        /// <inheritdoc />
        public async Task<DateTime> GetLatestModifiedDateForProviderAsync(int providerId)
        {
            var publicLatestModifiedDate = await GetLatestModifiedDateForProviderAsync(providerId, false);
            var protectedLatestModifiedDate = await GetLatestModifiedDateForProviderAsync(providerId, true);

            return protectedLatestModifiedDate > publicLatestModifiedDate
                ? protectedLatestModifiedDate
                : publicLatestModifiedDate;
        }

        /// <inheritdoc />
        public async Task<long> GetMatchCountAsync(SearchFilterBase filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var countResponse = await Client.CountAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );
            countResponse.ThrowIfInvalid();

            return countResponse.Count;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetObservationsAsync(IEnumerable<string> occurrenceIds, bool protectedIndex)
        {
            try
            {
                var searchResponse = await Client.SearchAsync<Observation>(s => s
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .Terms(t => t
                            .Field(f => f.Occurrence.OccurrenceId)
                            .Terms(occurrenceIds)
                        )
                    )
                    .Size(occurrenceIds?.Count() ?? 0)
                    .Source(s => s
                        .Includes(i => i.Fields(f => f.Occurrence, f => f.Location))
                    )
                    .TrackTotalHits(false)
                );
                searchResponse.ThrowIfInvalid();
                
                return searchResponse.Documents;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetObservationsAsync(
            IEnumerable<string> occurrenceIds,
            IEnumerable<string> outputFields,
            bool protectedIndex)
        {
            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Query(q => q
                    .Terms(t => t
                        .Field(f => f.Occurrence.OccurrenceId)
                        .Terms(occurrenceIds)
                    )
                )
                .Size(occurrenceIds?.Count() ?? 0)
                .Source(p => p
                    .Includes(i => i
                        .Fields(outputFields
                            .Select(f => new Field(f))
                        )
                    )
                )
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse.Documents;
        }

        /// <inheritdoc />
        public async Task<SearchAfterResult<ExtendedMeasurementOrFactRow>> GetMeasurementOrFactsBySearchAfterAsync(
           SearchFilterBase filter,
           string pointInTimeId = null,
           IEnumerable<object> searchAfter = null)
        {
            var searchIndex = GetCurrentIndex(filter);
            var searchResponse = await SearchAfterAsync<dynamic>(searchIndex, new SearchDescriptor<dynamic>()
                .Index(searchIndex)
                .Query(query => query
                    .Bool(boolQueryDescriptor => boolQueryDescriptor
                        .Filter(filter.ToMeasurementOrFactsQuery())
                    )
                )
                .Source(source => source
                    .Includes(fieldsDescriptor => fieldsDescriptor
                        .Field("occurrence.occurrenceId")
                        .Field("measurementOrFacts"))), 
                pointInTimeId, 
                searchAfter
            );

            return new SearchAfterResult<ExtendedMeasurementOrFactRow>
            {
                Records = searchResponse.Documents.ToObservations()?.ToExtendedMeasurementOrFactRows(),
                PointInTimeId = searchResponse.PointInTimeId,
                SearchAfter = searchResponse.Hits?.LastOrDefault()?.Sorts
            };
        }

        /// <inheritdoc />
        public async Task<SearchAfterResult<SimpleMultimediaRow>> GetMultimediaBySearchAfterAsync(
            SearchFilterBase filter,
            string pointInTimeId = null,
            IEnumerable<object> searchAfter = null)
        {
            var searchIndex = GetCurrentIndex(filter);

            var searchResponse = await SearchAfterAsync<dynamic>(searchIndex, new SearchDescriptor<dynamic>()
                .Index(searchIndex)
                .Query(query => query
                    .Bool(boolQueryDescriptor => boolQueryDescriptor
                        .Filter(filter.ToMultimediaQuery())
                    )
                )
                .Source(source => source
                    .Includes(fieldsDescriptor => fieldsDescriptor
                        .Field("occurrence.occurrenceId")
                        .Field("media")
                    )
                ),
                pointInTimeId,
                searchAfter);

            return new SearchAfterResult<SimpleMultimediaRow>
            {
                Records = searchResponse.Documents?.ToObservations().ToSimpleMultimediaRows(),
                PointInTimeId = searchResponse.PointInTimeId,
                SearchAfter = searchResponse.Hits?.LastOrDefault()?.Sorts
            };
        }

        /// <inheritdoc />
        public async Task<SearchAfterResult<T>> GetObservationsBySearchAfterAsync<T>(
            SearchFilter filter,
            string pointInTimeId = null,
            IEnumerable<object> searchAfter = null) 
        {
            var searchIndex = GetCurrentIndex(filter);

            var searchResponse = await SearchAfterAsync<dynamic>(searchIndex, new SearchDescriptor<dynamic>()
                .Index(searchIndex)
                .Source(filter.Output?.Fields.ToProjection(filter is SearchFilterInternal))
                    .Query(q => q
                        .Bool(b => b
                            .Filter(filter.ToQuery())
                        )
                    ),
                pointInTimeId,
                searchAfter);

            return new SearchAfterResult<T>
            {
                Records = (IEnumerable<T>)(typeof(T).Equals(typeof(Observation)) ? searchResponse.Documents?.ToObservations()?.ToArray() : searchResponse.Documents),
                PointInTimeId = searchResponse.PointInTimeId,
                SearchAfter = searchResponse.Hits?.LastOrDefault()?.Sorts
            };
        }

        public async Task<ScrollResult<dynamic>> GetObservationsByScrollAsync(
            SearchFilter filter,
            int take,
            string scrollId)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            
            var sortDescriptor = await Client.GetSortDescriptorAsync<dynamic>(indexNames, filter?.Output?.SortOrders);
            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search");

            operation.Telemetry.Properties["Filter"] = filter.ToString();
           
            // Retry policy by Polly
            var searchResponse = await PollyHelper.GetRetryPolicy(3, 100).ExecuteAsync(async () =>
            {
                var queryResponse = string.IsNullOrEmpty(scrollId) ?
                    await Client.SearchAsync<dynamic>(s => s
                        .Index(indexNames)
                        .Query(q => q
                            .Bool(b => b
                                .MustNot(excludeQuery)
                                .Filter(query)
                            )
                        )
                        .Sort(sort => sortDescriptor)
                        .Size(take)
                        .Source(filter.Output?.Fields.ToProjection(filter is SearchFilterInternal))
                        .Scroll(ScrollTimeout)
                    ) : await Client
                        .ScrollAsync<dynamic>(ScrollTimeout, scrollId);

                queryResponse.ThrowIfInvalid();
   
                return queryResponse;
            });

            operation.Telemetry.Metrics["SpeciesObservationCount"] = searchResponse.Documents.Count;

            // Optional: explicitly send telemetry item:
            _telemetry.StopOperation(operation);

            return new ScrollResult<dynamic>
            {
                Records = searchResponse.Documents,
                ScrollId = searchResponse.Documents.Count < take ? null : searchResponse.ScrollId,
                Take = take,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };

            // When operation is disposed, telemetry item is sent.
        }

        /// <inheritdoc /> 
        public async Task<(DateTime? firstSpotted, DateTime? lastSpotted, GeoBounds geographicCoverage)> GetProviderMetaDataAsync(int providerId, bool protectedIndex)
        {
            var res = await Client.SearchAsync<Observation>(s => s
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.DataProviderId)
                        .Value(providerId)))
                .Aggregations(a => a
                    .Min("firstSpotted", m => m
                        .Field(f => f.Event.StartDate)
                    )
                    .Max("lastSpotted", m => m
                        .Field(f => f.Event.EndDate)
                    )
                    .GeoBounds("geographicCoverage", g => g
                        .Field(f => f.Location.PointLocation)
                        .WrapLongitude()
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            var defaultGeoBounds = new GeoBounds
            { BottomRight = new LatLon() { Lat = 0.0, Lon = 0.0 }, TopLeft = new LatLon() { Lat = 0.0, Lon = 0.0 } };
            if (!res.IsValid)
            {
                return (null, null, defaultGeoBounds);
            }

            var firstSpotted = res.Aggregations?.Min("firstSpotted")?.Value;
            var lastSpotted = res.Aggregations?.Max("lastSpotted")?.Value;
            var geographicCoverage = res.Aggregations?.GeoBounds("geographicCoverage")?.Bounds;

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

            return (epoch.AddMilliseconds(firstSpotted ?? 0).ToUniversalTime(), epoch.AddMilliseconds(lastSpotted ?? 0).ToUniversalTime(), geographicCoverage?.BottomRight != null ? geographicCoverage : defaultGeoBounds);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetRandomObservationsAsync(int take, bool protectedIndex)
        {
            try
            {
                var searchResponse = await Client.SearchAsync<Observation>(s => s
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .FunctionScore(fs => fs
                            .Functions(f => f
                                .RandomScore(rs => rs
                                    .Seed(DateTime.Now.ToBinary())
                                    .Field(p => p.Occurrence.OccurrenceId)))))
                    .Size(take)
                    .Source(s => s
                        .Includes(i => i.Fields(f => f.Occurrence, f => f.Location))
                    )
                    .TrackTotalHits(false)
                );
                searchResponse.ThrowIfInvalid();
                
                return searchResponse.Documents;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return null;
            }
        }

        /// <inheritdoc />
        public Uri HostUrl => Client.ConnectionSettings.ConnectionPool.Nodes.FirstOrDefault().Uri;

        /// <inheritdoc />
        public async Task<long> IndexCountAsync(bool protectedIndex)
        {
            try
            {
                var countResponse = await Client.CountAsync<dynamic>(s => s
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                );

                countResponse.ThrowIfInvalid();
                
                return countResponse.Count;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return -1;
            }
        }

        /// <inheritdoc />
        public async Task<ScrollResult<ExtendedMeasurementOrFactRow>> ScrollMeasurementOrFactsAsync(
            SearchFilterBase filter,
            string scrollId = null)
        {
            ISearchResponse<dynamic> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                var indexNames = GetCurrentIndex(filter);
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexNames)
                    .Source(source => source
                        .Includes(fieldsDescriptor => fieldsDescriptor
                            .Field("occurrence.occurrenceId")
                            .Field("measurementOrFacts")))
                    .Query(query => query
                        .Bool(boolQueryDescriptor => boolQueryDescriptor
                            .Filter(filter.ToMeasurementOrFactsQuery())
                        )
                    )
                    .Sort(s => s.Ascending(new Field("_doc")))
                    .Scroll(ScrollTimeout)
                    .Size(ScrollBatchSize)
                );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<Observation>(ScrollTimeout, scrollId);
            }

            searchResponse.ThrowIfInvalid();
            
            return new ScrollResult<ExtendedMeasurementOrFactRow>
            {
                Records = searchResponse.Documents?.ToObservations()?.ToExtendedMeasurementOrFactRows(),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <inheritdoc />
        public async Task<ScrollResult<SimpleMultimediaRow>> ScrollMultimediaAsync(
            SearchFilterBase filter,
            string scrollId = null)
        {
            ISearchResponse<dynamic> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                var indexNames = GetCurrentIndex(filter);
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexNames)
                    .Source(source => source
                        .Includes(fieldsDescriptor => fieldsDescriptor
                            .Field("occurrence.occurrenceId")
                            .Field("media")))
                    .Query(query => query
                        .Bool(boolQueryDescriptor => boolQueryDescriptor
                            .Filter(filter.ToMultimediaQuery())
                        )
                    )
                    .Sort(s => s
                        .Ascending(new Field("_doc"))
                    )
                    .Scroll(ScrollTimeout)
                    .Size(ScrollBatchSize)
                );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<dynamic>(ScrollTimeout, scrollId);
            }

            searchResponse.ThrowIfInvalid();
    
            return new ScrollResult<SimpleMultimediaRow>
            {
                Records = searchResponse.Documents?.ToObservations()?.ToSimpleMultimediaRows(),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <inheritdoc />
        public async Task<ScrollResult<T>> ScrollObservationsAsync<T>(
            SearchFilterBase filter,
            string scrollId)
        {            
            // Retry policy by Polly
            var searchResponse = await PollyHelper.GetRetryPolicy(3, 100).ExecuteAsync(async () =>
            {
                var queryResponse = string.IsNullOrEmpty(scrollId) ? await Client
                    .SearchAsync<dynamic>(s => s
                        .Index(GetCurrentIndex(filter))
                        .Source(p => new SourceFilterDescriptor<dynamic>()
                            .Excludes(e => e
                                .Field("artportalenInternal")
                                .Field("location.point")
                                .Field("location.pointLocation")
                                .Field("location.pointWithBuffer")
                                .Field("location.pointWithDisturbanceBuffer")
                            ))
                        .Query(q => q
                            .Bool(b => b
                                .Filter(filter.ToQuery())
                            )
                        )
                        .Sort(s => s.Ascending(new Field("_doc")))
                        .Scroll(ScrollTimeout)
                        .Size(ScrollBatchSize)
                    ) :
                     await Client
                    .ScrollAsync<dynamic>(ScrollTimeout, scrollId);
                queryResponse.ThrowIfInvalid();

                return queryResponse;
            });

            return new ScrollResult<T>
            {
                Records = (typeof(T) == typeof(Observation) ? CastDynamicsToObservations(searchResponse.Documents) : searchResponse.Documents) as IEnumerable<T>,
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata?.Total?.Value ?? 0
            };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> TryToGetOccurenceIdDuplicatesAsync(bool protectedIndex, int maxReturnedItems)
        {
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Aggregations(a => a
                    .Terms("OccurrenceIdDuplicatesExists", f => f
                        .Field("occurrence.occurrenceId")
                        .MinimumDocumentCount(2)
                        .Size(maxReturnedItems)
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();
           
            return searchResponse.Aggregations.Terms("OccurrenceIdDuplicatesExists").Buckets?.Select(b => b.Key);
        }

        /// <inheritdoc />
        public string UniquePublicIndexName => IndexHelper.GetIndexName<Observation>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

        /// <inheritdoc />
        public string UniqueProtectedIndexName => IndexHelper.GetIndexName<Observation>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, true);

        /// <inheritdoc />
        public async Task<bool> ValidateProtectionLevelAsync(bool protectedIndex)
        {
            try
            {
                var countResponse = protectedIndex ?
                    await Client.CountAsync<Observation>(s => s
                    .Index(ProtectedIndexName)
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(mn => mn.Term(t => t
                                .Field(f => f.DiffusionStatus).Value(DiffusionStatus.NotDiffused))
                            )
                        )
                    ))
                    :
                    await Client.CountAsync<Observation>(s => s
                    .Index(PublicIndexName)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Term(t => t
                                    .Field(f => f.AccessRights.Id).Value((int)AccessRightsId.NotForPublicUsage)
                                ), f => f
                                .Term(t => t
                                    .Field(f => f.DiffusionStatus).Value(DiffusionStatus.NotDiffused)
                                )
                            )
                        )
                    ));

                countResponse.ThrowIfInvalid();

                if (!countResponse.Count.Equals(0))
                {
                    Logger.LogError($"Failed to validate protection level for Index: {(protectedIndex ? ProtectedIndexName : PublicIndexName)}, count of observations with protection:{protectedIndex} = {countResponse.Count}, should be 0");
                }
                return countResponse.Count.Equals(0);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> VerifyCollectionAsync(bool protectedIndex)
        {
            var response = await Client.Indices.ExistsAsync(protectedIndex ? ProtectedIndexName : PublicIndexName);

            if (!response.Exists)
            {
                await AddCollectionAsync(protectedIndex);
            }

            return !response.Exists;
        }        

        public async Task<IEnumerable<AggregationItem>> GetAggregationItemsAsync(SearchFilter filter, string aggregationField)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("termAggregation", t => t
                        .Size(65536)
                        .Field(aggregationField)
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();
            IEnumerable<AggregationItem> result = searchResponse.Aggregations
                .Terms("termAggregation")
                .Buckets
                .Select(b => new AggregationItem { AggregationKey = b.Key, DocCount = (int)(b.DocCount ?? 0) });

            return result;
        }

        public async Task<List<AggregationItem>> GetAllAggregationItemsAsync(SearchFilter filter, string aggregationField)
        {
            var indexName = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            var eventIdItems = new List<AggregationItem>();
            CompositeKey nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageAggregationItemAsync(indexName, aggregationField, query, excludeQuery, nextPageKey, pageTaxaAsyncTake);
                var compositeAgg = searchResponse.Aggregations.Composite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    eventIdItems.Add(new AggregationItem
                    {
                        AggregationKey = bucket.Key["termAggregation"].ToString(),
                        DocCount = Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0))
                    });
                }

                nextPageKey = compositeAgg.Buckets.Count >= pageTaxaAsyncTake ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);

            return eventIdItems;
        }

        private async Task<ISearchResponse<dynamic>> PageAggregationItemAsync(
            string indexName,
            string aggregationField,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
            CompositeKey nextPage,
            int take)
        {
            ISearchResponse<dynamic> searchResponse;

            searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Composite("compositeAggregation", g => g
                        .After(nextPage ?? null)
                        .Size(take)
                        .Sources(src => src
                            .Terms("termAggregation", tt => tt
                                .Field(aggregationField)
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse;
        }


        public async Task<List<EventOccurrenceAggregationItem>> GetEventOccurrenceItemsAsync(SearchFilter filter)
        {
            var indexName = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);            
            var occurrencesByEventId = new Dictionary<string, List<string>>();
            CompositeKey nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageEventOccurrenceItemAsync(indexName, query, excludeQuery, nextPageKey, pageTaxaAsyncTake);
                var compositeAgg = searchResponse.Aggregations.Composite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    string eventId = bucket.Key["eventId"].ToString();
                    string occurrenceId = bucket.Key["occurrenceId"].ToString();
                    if (!occurrencesByEventId.ContainsKey(eventId))
                        occurrencesByEventId[eventId] = new List<string>();
                    occurrencesByEventId[eventId].Add(occurrenceId);
                }

                nextPageKey = compositeAgg.Buckets.Count >= pageTaxaAsyncTake ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);


            var eventIdItems = occurrencesByEventId.Select(m => new EventOccurrenceAggregationItem { EventId = m.Key, OccurrenceIds = m.Value }).ToList();
            return eventIdItems;
        }

        private async Task<ISearchResponse<dynamic>> PageEventOccurrenceItemAsync(
            string indexName,            
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
            CompositeKey nextPage,
            int take)
        {
            ISearchResponse<dynamic> searchResponse;

            searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Composite("compositeAggregation", g => g
                        .After(nextPage ?? null)
                        .Size(take)
                        .Sources(src => src
                            .Terms("eventId", tt => tt
                                .Field("event.eventId")
                            )
                            .Terms("occurrenceId", tt => tt
                                .Field("occurrence.occurrenceId")
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse;
        }
        
        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, bool getAllFields = false)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var sortDescriptor = await Client.GetSortDescriptorAsync<Observation>(indexNames, filter?.Output?.SortOrders);
            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search");

            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)                
                .Source(getAllFields ? p => new SourceFilterDescriptor<dynamic>() : filter.Output?.Fields.ToProjection(filter is SearchFilterInternal))
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Sort(sort => sortDescriptor)
            );

            searchResponse.ThrowIfInvalid();

            var totalCount = searchResponse.HitsMetadata.Total.Value;

            var includeRealCount = totalCount >= ElasticSearchMaxRecords;

            if (filter is SearchFilterInternal internalFilter)
            {
                includeRealCount = internalFilter.IncludeRealCount;
            }

            if (includeRealCount)
            {
                var countResponse = await Client.CountAsync<dynamic>(s => s
                    .Index(indexNames)
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    )
                );
                countResponse.ThrowIfInvalid();

                totalCount = countResponse.Count;
            }

            operation.Telemetry.Metrics["SpeciesObservationCount"] = searchResponse.Documents.Count;

            // Optional: explicitly send telemetry item:
            _telemetry.StopOperation(operation);

            return new PagedResult<dynamic>
            {
                Records = searchResponse.Documents,
                Skip = skip,
                Take = take,
                TotalCount = totalCount
            };
        }

        /// <inheritdoc/>
        public async Task<dynamic> GetObservationAsync(string occurrenceId, SearchFilter filter, bool getAllFields = false)
        {
            var indexNames = GetCurrentIndex(filter);
            var query = filter.ToQuery(true);
            query.TryAddTermCriteria("occurrence.occurrenceId", occurrenceId);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Get");
            operation.Telemetry.Properties["OccurrenceId"] = occurrenceId;
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)
                    )
                )
                .Size(1)
                .Source(getAllFields ? p => new SourceFilterDescriptor<dynamic>() : filter.Output?.Fields.ToProjection(filter is SearchFilterInternal))                
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            // Optional: explicitly send telemetry item:
            _telemetry.StopOperation(operation);

            return searchResponse.Documents;

        }
    }
}
