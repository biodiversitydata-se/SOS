using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Event = SOS.Lib.Models.Processed.DataStewardship.Event.Event;


namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Observation event repository.
    /// </summary>
    public class EventRepository : ProcessRepositoryBase<Event, string>,
        IEventRepository
    {
        private const int ElasticSearchMaxRecords = 10000;

        /// <summary>
        /// Add the collection
        /// </summary>
        /// <returns></returns>
        private async Task<bool> AddCollectionAsync()
        {
            var createIndexResponse = await Client.Indices.CreateAsync<Event>(IndexName, s => s
                .Index(IndexName)
                .Settings(s => s
                    .NumberOfShards(NumberOfShards)
                    .NumberOfReplicas(NumberOfReplicas)
                    .MaxResultWindow(100000)
                    .MaxTermsCount(110000)
                )
                .Mappings(map => map
                    .Properties(ps => ps
                        .KeywordVal(kwlc => kwlc.Id, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.EventId)
                        .KeywordVal(kwlc => kwlc.ParentEventId, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.EventType, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.SamplingProtocol, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.SamplingEffort, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.SampleSizeValue, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.SampleSizeUnit, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.PlainStartDate, IndexSetting.SearchOnly)
                        .KeywordVal(kwlc => kwlc.PlainStartTime, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.PlainEndDate, IndexSetting.SearchOnly)
                        .KeywordVal(kwlc => kwlc.PlainEndTime, IndexSetting.None)
                        .KeywordVal(kwlc => kwlc.Habitat, IndexSetting.None)
                        .DateVal(d => d.EndDate, IndexSetting.SearchOnly)
                        .DateVal(d => d.StartDate, IndexSetting.SearchOnly)
                        .TextVal(t => t.EventRemarks, false)
                        .Object(o => o.Location, l => l
                            .Properties(ps => ps
                                .GeoShape(gs => gs.Location.Point)
                                .GeoPoint(gp => gp.Location.PointLocation)
                                .GeoShape(gs => gs.Location.PointWithBuffer)
                                .GeoShape(gs => gs.Location.PointWithDisturbanceBuffer)
                                .NumberVal(n => n.Location.DecimalLongitude, IndexSetting.SearchSortAggregate, NumberType.Double)
                                .NumberVal(n => n.Location.DecimalLatitude, IndexSetting.SearchSortAggregate, NumberType.Double)
                                .NumberVal(n => n.Location.CoordinateUncertaintyInMeters, IndexSetting.SearchSortAggregate, NumberType.Integer)
                                .NumberVal(n => n.Location.Type, IndexSetting.None, NumberType.Byte)
                                .NumberVal(n => n.Location.CoordinatePrecision, IndexSetting.None, NumberType.Double)
                                .NumberVal(n => n.Location.MaximumDepthInMeters, IndexSetting.None, NumberType.Double)
                                .NumberVal(n => n.Location.MaximumDistanceAboveSurfaceInMeters, IndexSetting.None, NumberType.Double)
                                .NumberVal(n => n.Location.MaximumElevationInMeters, IndexSetting.None, NumberType.Double)
                                .NumberVal(n => n.Location.MinimumDepthInMeters, IndexSetting.None, NumberType.Double)
                                .NumberVal(n => n.Location.MinimumDistanceAboveSurfaceInMeters, IndexSetting.None, NumberType.Double)
                                .NumberVal(n => n.Location.MinimumElevationInMeters, IndexSetting.None, NumberType.Double)
                                .BooleanVal(b => b.Location.IsInEconomicZoneOfSweden, IndexSetting.SearchOnly)
                                .KeywordVal(kwlc => kwlc.Location.LocationId, IndexSetting.SearchSortAggregate)
                                .KeywordVal(kwlc => kwlc.Location.CountryCode, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.FootprintSRS, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.GeodeticDatum, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.GeoreferencedBy, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.GeoreferencedDate, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.GeoreferenceProtocol, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.GeoreferenceSources, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.GeoreferenceVerificationStatus, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.HigherGeography, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.HigherGeographyId, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.Island, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.IslandGroup, IndexSetting.None)
                                .Keyword(kw => kw.Location.Locality, kw => kw
                                    .Normalizer("lowercase")
                                    .DocValues(true)
                                    .Fields(f => f
                                        .Keyword("raw", kw => kw
                                            .DocValues(false)
                                        )
                                    )
                                )
                                .KeywordVal(kwlc => kwlc.Location.LocationRemarks, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.LocationAccordingTo, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.FootprintSpatialFit, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.FootprintWKT, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.GeoreferenceRemarks, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.PointRadiusSpatialFit, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.VerbatimCoordinates, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.VerbatimCoordinateSystem, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.VerbatimDepth, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.VerbatimElevation, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.VerbatimLatitude, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.VerbatimLocality, IndexSetting.SearchOnly) // WFS
                                .KeywordVal(kwlc => kwlc.Location.VerbatimLongitude, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.VerbatimSRS, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Location.WaterBody, IndexSetting.None)
                                .Object(o => o.Location.Attributes, c => c
                                    .Properties(ps => ps
                                        .BooleanVal(b => b.Location.Attributes.IsPrivate, IndexSetting.None)
                                        .NumberVal(n => n.Location.Attributes.ProjectId, IndexSetting.SearchOnly, NumberType.Integer)
                                        .KeywordVal(kwlc => kwlc.Location.Attributes.ExternalId, IndexSetting.SearchOnly)
                                        .KeywordVal(kwlc => kwlc.Location.Attributes.CountyPartIdByCoordinate, IndexSetting.SearchOnly)
                                        .KeywordVal(kwlc => kwlc.Location.Attributes.ProvincePartIdByCoordinate, IndexSetting.SearchOnly)
                                        .KeywordVal(kwlc => kwlc.Location.Attributes.VerbatimMunicipality, IndexSetting.None)
                                        .KeywordVal(kwlc => kwlc.Location.Attributes.VerbatimProvince, IndexSetting.None)
                                    )
                                )
                                .Object(o => o.Location.Continent, c => c
                                    .Properties(ps => ps
                                        .KeywordVal(kwlc => kwlc.Location.Continent.Value, IndexSetting.None)
                                        .NumberVal(nr => nr.Location.Continent.Id, IndexSetting.None, NumberType.Byte)
                                    )
                                )
                                .Object(o => o.Location.Country, c => c
                                    .Properties(ps => ps
                                        .KeywordVal(kwlc => kwlc.Location.Country.Value, IndexSetting.SearchOnly)
                                        .NumberVal(nr => nr.Location.Country.Id, IndexSetting.SearchSortAggregate, NumberType.Byte)
                                    )
                                )
                                .Object(o => o.Location.Atlas10x10, c => c
                                    .Properties(ps => ps
                                        .KeywordVal(kwlc => kwlc.Location.Atlas10x10.FeatureId, IndexSetting.SearchSortAggregate)
                                        .KeywordVal(kwlc => kwlc.Location.Atlas10x10.Name, IndexSetting.None)
                                    )
                                )
                                .Object(o => o.Location.Atlas5x5, c => c
                                    .Properties(ps => ps
                                        .KeywordVal(kwlc => kwlc.Location.Atlas5x5.FeatureId, IndexSetting.SearchSortAggregate)
                                        .KeywordVal(kwlc => kwlc.Location.Atlas5x5.Name, IndexSetting.None)
                                    )
                                )
                                .Object(o => o.Location.CountryRegion, c => c
                                    .Properties(ps => ps
                                        .KeywordVal(kwlc => kwlc.Location.CountryRegion.FeatureId, IndexSetting.SearchSortAggregate)
                                        .KeywordVal(kwlc => kwlc.Location.CountryRegion.Name, IndexSetting.SearchOnly)
                                    )
                                )
                                .Object(o => o.Location.County, c => c
                                    .Properties(ps => ps
                                        .KeywordVal(kwlc => kwlc.Location.County.FeatureId, IndexSetting.SearchSortAggregate)
                                        .KeywordVal(kwlc => kwlc.Location.County.Name, IndexSetting.SearchSortAggregate)
                                    )
                                )
                                .Object(o => o.Location.Municipality, c => c
                                    .Properties(ps => ps
                                        .KeywordVal(kwlc => kwlc.Location.Municipality.FeatureId, IndexSetting.SearchSortAggregate)
                                        .KeywordVal(kwlc => kwlc.Location.Municipality.Name, IndexSetting.SearchSortAggregate)
                                    )
                                )
                                .Object(o => o.Location.Parish, c => c
                                    .Properties(ps => ps
                                        .KeywordVal(kwlc => kwlc.Location.Parish.FeatureId, IndexSetting.SearchSortAggregate)
                                        .KeywordVal(kwlc => kwlc.Location.Parish.Name, IndexSetting.SearchSortAggregate)
                                    )
                                )
                                .Object(o => o.Location.Province, c => c
                                    .Properties(ps => ps
                                        .KeywordVal(kwlc => kwlc.Location.Province.FeatureId, IndexSetting.SearchSortAggregate)
                                        .KeywordVal(kwlc => kwlc.Location.Province.Name, IndexSetting.SearchSortAggregate)
                                    )
                                )
                            )
                        )
                        .Object(o => o.DataStewardship, l => l
                            .Properties(ps => ps
                                .KeywordVal(kwlc => kwlc.DataStewardship.DatasetIdentifier)
                                .KeywordVal(kwlc => kwlc.DataStewardship.DatasetTitle)
                            )
                        )
                        .Object(o => o.DiscoveryMethod, t => t
                            .Properties(ps => ps
                                .KeywordVal(kwlc => kwlc.DiscoveryMethod.Value, IndexSetting.SearchOnly)
                                .NumberVal(nr => nr.DiscoveryMethod.Id, IndexSetting.SearchSortAggregate, NumberType.Short)
                            )
                        )
                        .Object(o => o.MeasurementOrFacts, n => n
                            .Properties(ps => ps
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().OccurrenceID)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementRemarks, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementAccuracy, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementDeterminedBy, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementDeterminedDate, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementID, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementMethod, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementType, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementTypeID, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementUnit, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementUnitID, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementValue, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementValueID, IndexSetting.None)
                            )
                        )
                        .Object(o => o.Media, n => n
                            .Properties(ps => ps
                                .KeywordVal(kwlc => kwlc.Media.First().Description, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().Audience, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().Contributor, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().Created, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().Creator, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().DatasetID, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().Format, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().Identifier, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().License, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().Publisher, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().References, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().RightsHolder, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().Source, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().Title, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.Media.First().Type, IndexSetting.None)
                            )
                        )
                        .Object(o => o.RecorderOrganisation, t => t
                            .Properties(ps => ps
                                .KeywordVal(kwlc => kwlc.RecorderOrganisation.First().OrganisationCode, IndexSetting.None)
                                .KeywordVal(kwlc => kwlc.RecorderOrganisation.First().OrganisationID, IndexSetting.None)
                            )
                        )
                        .Object(o => o.Weather, t => t
                            .Properties(ps => ps
                                .Object(o => o.Weather.AirTemperature, o => o
                                    .Properties(ps => ps
                                        .NumberVal(n => n.Weather.AirTemperature.WeatherMeasure, IndexSetting.None, NumberType.Double)
                                        .NumberVal(n => n.Weather.AirTemperature.Unit, IndexSetting.None, NumberType.Byte)
                                    )
                                )
                                .NumberVal(n => n.Weather.Cloudiness, IndexSetting.None, NumberType.Byte)
                                .NumberVal(n => n.Weather.Precipitation, IndexSetting.None, NumberType.Byte)
                                .NumberVal(n => n.Weather.SnowCover, IndexSetting.None, NumberType.Byte)
                                .NumberVal(n => n.Weather.Sunshine, IndexSetting.None, NumberType.Byte)
                                .NumberVal(n => n.Weather.WindDirectionCompass, IndexSetting.None, NumberType.Byte)
                                .Object(o => o.Weather.WindDirectionDegrees, o => o
                                    .Properties(ps => ps
                                        .NumberVal(n => n.Weather.WindDirectionDegrees.WeatherMeasure, IndexSetting.None, NumberType.Double)
                                        .NumberVal(n => n.Weather.WindDirectionDegrees.Unit, IndexSetting.None, NumberType.Byte)
                                    )
                                )
                                .Object(o => o.Weather.WindSpeed, o => o
                                    .Properties(ps => ps
                                        .NumberVal(n => n.Weather.WindSpeed.WeatherMeasure, IndexSetting.None, NumberType.Double)
                                        .NumberVal(n => n.Weather.WindSpeed.Unit, IndexSetting.None, NumberType.Byte)
                                    )
                                )
                               .NumberVal(n => n.Weather.WindStrength, IndexSetting.None, NumberType.Byte)
                            )
                        )
                    )
                )
            );

            return createIndexResponse.Acknowledged && createIndexResponse.IsValidResponse ? true : throw new Exception($"Failed to create ObservationEvent index. Error: {createIndexResponse.DebugInformation}");
        }

        private readonly static JsonSerializerOptions jsonSerializerOptions = new ()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            Converters = {
                new JsonStringEnumConverter(),
                new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
            }
        };

        public async Task<List<Event>> GetEventsByIds(IEnumerable<string> ids, IEnumerable<SortOrderFilter> sortOrders = null)
        {
            if (ids == null || !ids.Any()) throw new ArgumentException("ids is empty");
            
            var sortDescriptor = await Client.GetSortDescriptorAsync<Event, Event>(IndexName, sortOrders);
            var searchResponse = await Client.SearchAsync<Event>(s => s
                .Indices(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f.Terms(t => t
                           .Field("eventId")
                           .Terms(new TermsQueryField(ids.Select(s => FieldValue.String(s)).ToArray()))
                        ))
                    )
                )
                .Size(ids?.Count() ?? 0)
                .Sort(sortDescriptor?.ToArray())
                .TrackTotalHits(new TrackHits(false))
            );

            if (!searchResponse.IsValidResponse) throw new InvalidOperationException(searchResponse.DebugInformation);
            var events = searchResponse.Documents.ToList();
            return events;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="clusterHealthCache"></param>
        /// <param name="logger"></param>
        public EventRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            ILogger<EventRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, logger)
        {
            LiveMode = true;
            _id = nameof(Observation); // The active instance should be the same as the ProcessedObservationRepository which uses the Observation type.
        }

        /// <inheritdoc />
        public async Task<bool> ClearCollectionAsync()
        {
            await DeleteCollectionAsync();
            return await AddCollectionAsync();
        }

        /// <inheritdoc />
        public string UniqueIndexName => IndexHelper.GetIndexName<Event>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

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

        public async Task<List<AggregationItemList<TKey, TValue>>> GetAllAggregationItemsListAsync<TKey, TValue>(EventSearchFilter filter, string aggregationFieldKey, string aggregationFieldList)
        {
            var indexName = IndexName;
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter);
            var aggregationDictionary = new Dictionary<TKey, List<TValue>>();
            IReadOnlyDictionary<Field, FieldValue> nextPageKey = null;
            do
            {
                var searchResponse = await PageAggregationItemListAsync(indexName, aggregationFieldKey, aggregationFieldList, query, excludeQuery, nextPageKey, MaxNrElasticSearchAggregationBuckets);
                var compositeAgg = searchResponse.Aggregations.GetComposite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    TKey keyValue = (TKey)bucket.Key[aggregationFieldKey].Value;
                    TValue listValue = (TValue)bucket.Key[aggregationFieldList].Value;
                    if (!aggregationDictionary.ContainsKey(keyValue))
                        aggregationDictionary[keyValue] = new List<TValue>();
                    aggregationDictionary[keyValue].Add(listValue);
                }

                nextPageKey = compositeAgg.Buckets.Count >= MaxNrElasticSearchAggregationBuckets ? compositeAgg.AfterKey?.ToDictionary(ak => ak.Key.ToField(), ak => ak.Value) : null;
            } while (nextPageKey != null);

            var items = aggregationDictionary.Select(m => new AggregationItemList<TKey, TValue> { AggregationKey = m.Key, Items = m.Value }).ToList();
            return items;
        }

        private async Task<SearchResponse<dynamic>> PageAggregationItemListAsync(
            string indexName,
            string aggregationFieldKey,
            string aggregationFieldList,
            ICollection<Action<QueryDescriptor<dynamic>>> queries,
            ICollection<Action<QueryDescriptor<dynamic>>> excludeQueries,
            IReadOnlyDictionary<Field, FieldValue> nextPage,
            int take)
        {
            var searchResponse = await Client.SearchAsync<dynamic>(indexName, s => s
                
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("compositeAggregation", a => a
                        .Composite(c => c
                            .After(ak => nextPage?.ToFluentDictionary())
                            .Size(take)
                            .Sources(
                                [
                                    CreateCompositeTermsAggregationSource(
                                        (aggregationFieldKey, aggregationFieldKey, SortOrder.Desc)
                                    ),
                                    CreateCompositeTermsAggregationSource(
                                        (aggregationFieldList, aggregationFieldList, SortOrder.Asc)
                                    )
                                ]
                            )
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();
            return searchResponse;
        }

        public async Task<List<AggregationItem>> GetAllAggregationItemsAsync(EventSearchFilter filter, string aggregationField)
        {
            var indexName = IndexName;
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter);
            var items = new List<AggregationItem>();
            IReadOnlyDictionary<Field, FieldValue> nextPageKey = null;
            var take = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageAggregationItemAsync(indexName, aggregationField, query, excludeQuery, nextPageKey, take);
                var compositeAgg = searchResponse.Aggregations.GetComposite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    items.Add(new AggregationItem
                    {
                        AggregationKey = bucket.Key["termAggregation"].Value.ToString(),
                        DocCount = Convert.ToInt32(bucket.DocCount)
                    });
                }

                nextPageKey = compositeAgg.Buckets.Count >= take ? compositeAgg.AfterKey?.ToDictionary(ak => ak.Key.ToField(), ak => ak.Value) : null;
            } while (nextPageKey != null);

            return items;
        }

        private async Task<SearchResponse<dynamic>> PageAggregationItemAsync(
            string indexName,
            string aggregationField,
            ICollection<Action<QueryDescriptor<dynamic>>> queries,
            ICollection<Action<QueryDescriptor<dynamic>>> excludeQueries,
            IReadOnlyDictionary<Field, FieldValue> nextPageKey,
            int take)
        {
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("compositeAggregation", a => a
                        .Composite(c => c
                            .After(npk => nextPageKey.ToFluentDictionary() ?? null)
                            .Size(take)
                            .Sources(
                                [
                                    CreateCompositeTermsAggregationSource(
                                        ("termAggregation", aggregationField, SortOrder.Desc)
                                    )
                                ]
                            )
                        )
                    )

                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse;
        }

        public async Task<PagedResult<dynamic>> GetChunkAsync(EventSearchFilter filter, int skip, int take, bool getAllFields = false)
        {
            string indexName = IndexName;
            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);
            var sortDescriptor = await Client.GetSortDescriptorAsync<Event, Event>(indexName, filter.SortOrders);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexName)
                .Source(getAllFields ? null : (filter.OutputIncludeFields, filter.OutputExcludeFields).ToProjection())
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .Sort(sort => sortDescriptor?.ToArray())
            );

            searchResponse.ThrowIfInvalid();
            var totalCount = searchResponse.Total;
            var includeRealCount = totalCount >= ElasticSearchMaxRecords;

            if (includeRealCount)
            {
                var countResponse = await Client.CountAsync<dynamic>(indexName, s => s
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQueries.ToArray())
                            .Filter(queries.ToArray())
                        )
                    )
                );
                countResponse.ThrowIfInvalid();
                totalCount = countResponse.Count;
            }

            var records = CastDynamicsToEvents(searchResponse.Documents);

            return new PagedResult<dynamic>
            {
                Records = searchResponse.Documents,
                Skip = skip,
                Take = take,
                TotalCount = totalCount
            };
        }

        protected (ICollection<Action<QueryDescriptor<TQueryDescriptor>>>, ICollection<Action<QueryDescriptor<TQueryDescriptor>>>)
            GetCoreQueries<TQueryDescriptor>(EventSearchFilter filter) where TQueryDescriptor : class
        {
            var query = filter.ToQuery<TQueryDescriptor>();
            var excludeQuery = filter.ToExcludeQuery<TQueryDescriptor>();

            return (query, excludeQuery);
        }

        /// <summary>
        /// Cast dynamic to observation
        /// </summary>
        /// <param name="dynamicObjects"></param>
        /// <returns></returns>
        public static List<Event> CastDynamicsToEvents(IEnumerable<dynamic> dynamicObjects)
        {
            if (dynamicObjects == null) return null;

            return JsonSerializer.Deserialize<List<Event>>(
                JsonSerializer.Serialize(dynamicObjects, jsonSerializerOptions), jsonSerializerOptions);
        }

        public async Task WaitForIndexCreation(long expectedRecordsCount, TimeSpan? timeout = null)
        {
            Logger.LogInformation($"Begin waiting for index creation. Index={IndexName}, ExpectedRecordsCount={expectedRecordsCount}, Timeout={timeout}");
            if (timeout == null) timeout = TimeSpan.FromMinutes(10);
            var sleepTime = TimeSpan.FromSeconds(5);
            int nrIterations = (int)(Math.Ceiling(timeout.Value.TotalSeconds / sleepTime.TotalSeconds));
            long docCount = await IndexCountAsync();
            var iterations = 0;

            // Compare number of documents processed with actually db count
            // If docCount is less than process count, indexing is not ready yet
            while (docCount < expectedRecordsCount && iterations < nrIterations)
            {
                iterations++; // Safety to prevent infinite loop.                                
                await Task.Delay(sleepTime);
                docCount = await IndexCountAsync();
            }

            if (iterations == nrIterations)
            {
                Logger.LogError($"Failed waiting for index creation due to timeout. Index={IndexName}. ExpectedRecordsCount={expectedRecordsCount}, DocCount={docCount}");
            }
            else
            {
                Logger.LogInformation($"Finish waiting for index creation. Index={IndexName}.");
            }
        }

        public async Task<long> IndexCountAsync()
        {
            try
            {
                var countResponse = await Client.CountAsync<Event>(c => c.Indices(IndexName));
                countResponse.ThrowIfInvalid();
                return countResponse.Count;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return -1;
            }
        }
    }
}