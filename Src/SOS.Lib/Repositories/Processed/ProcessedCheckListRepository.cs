using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Checklist;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed;

/// <summary>
///     Species data service
/// </summary>
public class ProcessedChecklistRepository : ProcessRepositoryBase<Checklist, string>,
    IProcessedChecklistRepository
{
    /// <summary>
    /// Add the collection
    /// </summary>
    /// <returns></returns>
    private async Task<bool> AddCollectionAsync()
    {
        var createIndexResponse = await Client.Indices.CreateAsync<Checklist>(IndexName, i => i
            .Index(IndexName)
            .Settings(s => s
                .NumberOfShards(NumberOfShards)
                .NumberOfReplicas(NumberOfReplicas)
                .MaxTermsCount(110000)
                .MaxResultWindow(100000)
            )
            .Mappings(map => map
                 .Properties(ps => ps
                    .DateVal(d => d.Modified, IndexSetting.SearchOnly)
                    .DateVal(d => d.RegisterDate, IndexSetting.SearchOnly)
                    .KeywordVal(kwlc => kwlc.Id, IndexSetting.SearchOnly)
                    .KeywordVal(kwlc => kwlc.Name, IndexSetting.SearchOnly)
                    .KeywordVal(kwlc => kwlc.OccurrenceIds, IndexSetting.SearchOnly)
                    .KeywordVal(kwlc => kwlc.RecordedBy, IndexSetting.SearchOnly)
                    .KeywordVal(kwlc => kwlc.SamplingEffortTime, IndexSetting.SearchOnly)
                    .NumberVal(nm => nm.DataProviderId, IndexSetting.SearchSortAggregate, NumberType.Integer)
                    .NumberVal(nm => nm.TaxonIds, IndexSetting.SearchOnly, NumberType.Integer)
                    .NumberVal(nm => nm.TaxonIdsFound, IndexSetting.SearchOnly, NumberType.Integer)
                    .Object(o => o.ArtportalenInternal, t => t
                         .Properties(ps => ps
                             .NumberVal(n => n.ArtportalenInternal.ChecklistId, IndexSetting.SearchSortAggregate, NumberType.Integer)
                             .NumberVal(n => n.ArtportalenInternal.ParentTaxonId, IndexSetting.SearchOnly, NumberType.Integer)
                             .NumberVal(n => n.ArtportalenInternal.UserId, IndexSetting.SearchSortAggregate, NumberType.Integer) 
                         )
                     )
                    .Object(o => o.Event, t => t

                         .Properties(ps => ps
                             .Date(d => d.Event.EndDate, c => c.Index(true).DocValues(true))
                             .Date(d => d.Event.StartDate, c => c.Index(true).DocValues(true))
                             .ShortNumber(s => s.Event.EndDayOfYear, c => c.Index(true).DocValues(true))
                             .ShortNumber(s => s.Event.StartDayOfYear, c => c.Index(true).DocValues(true))
                             .ShortNumber(s => s.Event.StartDay, c => c.Index(true))
                             .ShortNumber(s => s.Event.EndDay, c => c.Index(true))
                             .ByteNumber(s => s.Event.StartMonth, c => c.Index(true).DocValues(true))
                             .ByteNumber(s => s.Event.EndMonth, c => c.Index(true).DocValues(true))
                             .ByteNumber(s => s.Event.StartHistogramWeek, c => c.Index(true).DocValues(true))
                             .ByteNumber(s => s.Event.EndHistogramWeek, c => c.Index(true).DocValues(true))
                             .ShortNumber(s => s.Event.StartYear, c => c.Index(true).DocValues(true))
                             .ShortNumber(s => s.Event.EndYear, c => c.Index(true).DocValues(true))
                             .KeywordVal(kwlc => kwlc.Event.EventId, IndexSetting.SearchSortAggregate)
                             .KeywordVal(kwlc => kwlc.Event.EventRemarks, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.FieldNumber, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.FieldNotes, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.Habitat, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.ParentEventId, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.PlainEndDate, IndexSetting.SearchOnly) // WFS
                             .KeywordVal(kwlc => kwlc.Event.PlainEndTime, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.PlainStartDate, IndexSetting.SearchOnly) // WFS
                             .KeywordVal(kwlc => kwlc.Event.PlainStartTime, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.SampleSizeUnit, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.SampleSizeValue, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.SamplingEffort, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.SamplingProtocol, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Event.VerbatimEventDate, IndexSetting.None)
                             .Object(o => o.Event.Media, c => c
                                 .Properties(ps => ps
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Description, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Audience, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Contributor, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Created, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Creator, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().DatasetID, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Format, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Identifier, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().License, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Publisher, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().References, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().RightsHolder, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Source, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Title, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.Media.First().Type, IndexSetting.None)
                                     .Object(o => o.Event.Media.First().Comments, mc => mc
                                         .Properties(ps => ps
                                             .KeywordVal(kwlc => kwlc.Event.Media.First().Comments.First().Comment, IndexSetting.None)
                                             .KeywordVal(kwlc => kwlc.Event.Media.First().Comments.First().CommentBy, IndexSetting.None)
                                             .KeywordVal(kwlc => kwlc.Event.Media.First().Comments.First().Created, IndexSetting.None)
                                         )
                                     )
                                 )
                             )
                             .Object(o => o.Event.MeasurementOrFacts, n => n
                                 .Properties(ps => ps
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().OccurrenceID, IndexSetting.SearchSortAggregate)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementRemarks, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementAccuracy, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementDeterminedBy, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementDeterminedDate, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementID, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementMethod, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementType, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementTypeID, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementUnit, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementUnitID, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementValue, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Event.MeasurementOrFacts.First().MeasurementValueID, IndexSetting.None)
                                 )
                             )
                             .Object(o => o.Event.Weather, n => n
                                 .Properties(ps => ps
                                     .NumberVal(kwlc => kwlc.Event.Weather.SnowCover, IndexSetting.None, NumberType.Byte)
                                     .NumberVal(kwlc => kwlc.Event.Weather.WindDirection, IndexSetting.None, NumberType.Byte)
                                     .NumberVal(kwlc => kwlc.Event.Weather.WindStrength, IndexSetting.None, NumberType.Byte)
                                     .NumberVal(kwlc => kwlc.Event.Weather.Precipitation, IndexSetting.None, NumberType.Byte)
                                     .NumberVal(kwlc => kwlc.Event.Weather.Visibility, IndexSetting.None, NumberType.Byte)
                                     .NumberVal(kwlc => kwlc.Event.Weather.Cloudiness, IndexSetting.None, NumberType.Byte)
                                     .Object(o => o.Event.Weather.Sunshine, n => n
                                         .Properties(ps => ps
                                             .NumberVal(n => n.Event.Weather.Sunshine.Value, IndexSetting.None, NumberType.Double)
                                             .NumberVal(n => n.Event.Weather.Sunshine.Unit, IndexSetting.None, NumberType.Byte)
                                         )
                                     )
                                     .Object(o => o.Event.Weather.AirTemperature, n => n
                                         .Properties(ps => ps
                                             .NumberVal(n => n.Event.Weather.AirTemperature.Value, IndexSetting.None, NumberType.Double)
                                             .NumberVal(n => n.Event.Weather.AirTemperature.Unit, IndexSetting.None, NumberType.Byte)
                                         )
                                     )
                                     .Object(o => o.Event.Weather.WindDirectionDegrees, n => n
                                         .Properties(ps => ps
                                             .NumberVal(n => n.Event.Weather.WindDirectionDegrees.Value, IndexSetting.None, NumberType.Double)
                                             .NumberVal(n => n.Event.Weather.WindDirectionDegrees.Unit, IndexSetting.None, NumberType.Byte)
                                         )
                                     )
                                     .Object(o => o.Event.Weather.WindSpeed, n => n
                                         .Properties(ps => ps
                                             .NumberVal(n => n.Event.Weather.WindSpeed.Value, IndexSetting.None, NumberType.Double)
                                             .NumberVal(n => n.Event.Weather.WindSpeed.Unit, IndexSetting.None, NumberType.Byte)
                                         )
                                     )
                                 )
                             )
                             .Object(o => o.Event.DiscoveryMethod, t => t
                                 .Properties(ps => ps
                                     .KeywordVal(kwlc => kwlc.Event.DiscoveryMethod.Value, IndexSetting.SearchOnly)
                                     .NumberVal(n => n.Event.DiscoveryMethod.Id, IndexSetting.SearchSortAggregate, NumberType.Short)
                                 )
                             )
                         )
                     )
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
                    .Object(o => o.Project, n => n
                         .Properties(ps => ps
                             .KeywordVal(kwlc => kwlc.Project.Category, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Project.CategorySwedish, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Project.Name, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Project.Owner, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Project.ProjectURL, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Project.Description, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Project.SurveyMethod, IndexSetting.None)
                             .KeywordVal(kwlc => kwlc.Project.SurveyMethodUrl, IndexSetting.None)
                             .DateVal(d => d.Project.StartDate, IndexSetting.None)
                             .DateVal(d => d.Project.EndDate, IndexSetting.None)
                             .BooleanVal(b => b.Project.IsPublic, IndexSetting.None)
                             .Object(o => o.Project.ProjectParameters,
                                 p => p.Properties(p => p
                                     .KeywordVal(kwlc => kwlc.Project.ProjectParameters.First().DataType, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Project.ProjectParameters.First().Name, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Project.ProjectParameters.First().Unit, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Project.ProjectParameters.First().Description, IndexSetting.None)
                                     .KeywordVal(kwlc => kwlc.Project.ProjectParameters.First().Value, IndexSetting.None)
                                 )
                             )
                         )
                     )
                )
            )
        );

        return createIndexResponse.Acknowledged && createIndexResponse.IsValidResponse ? true : throw new Exception($"Failed to create checklist index. Error: {createIndexResponse.DebugInformation}");
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="elasticClientManager"></param>
    /// <param name="elasticConfiguration"></param>
    /// <param name="processedConfigurationCache"></param>
    /// <param name="clusterHealthCache"></param>
    /// <param name="memoryCache"></param>
    /// <param name="logger"></param>
    public ProcessedChecklistRepository(
        IElasticClientManager elasticClientManager,
        ElasticSearchConfiguration elasticConfiguration,
        ICache<string, ProcessedConfiguration> processedConfigurationCache,
        IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
        IMemoryCache memoryCache,
        ILogger<ProcessedChecklistRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, memoryCache, logger)
    {
        LiveMode = true;
    }


    /// <inheritdoc />
    public async Task<bool> ClearCollectionAsync()
    {
        await DeleteCollectionAsync();
        return await AddCollectionAsync();
    }

    /// <inheritdoc />
    public async Task<Checklist> GetAsync(string id, bool internalCall)
    {
        var searchResponse = await Client.SearchAsync<Checklist>(IndexName, s => s
            .Query(q => q
            
                .Bool(b => b
                    .Filter(f => f.Term(t => t
                        .Field("_id")
                        .Value(id))))
                )

            .Size(1)
            .Source((Includes: Array.Empty<string>(), Excludes: internalCall ? null : new[] { "artportalenInternal" }).ToProjection())
            .TrackTotalHits(new TrackHits(false))
        );

        searchResponse.ThrowIfInvalid();

        return searchResponse.Documents?.FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<PagedResult<Checklist>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
        SearchSortOrder sortOrder)
    {
        var searchResponse = await Client.SearchAsync<Checklist>(IndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Filter(f => f.Exists(e => e.Field(f => f.Id)))
                )
            )
            .Sort(sort => sort
                .Field(f => f.Name)
            )
            .From(skip)
            .Size(take)
        );

        searchResponse.ThrowIfInvalid();

        return new PagedResult<Checklist>
        {
            Records = searchResponse.Documents,
            Skip = skip,
            Take = take,
            TotalCount = searchResponse.Total
        };

        // When operation is disposed, telemetry item is sent.
    }


    /// <summary>
    /// Count number of checklists matching the search filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<int> GetChecklistCountAsync(ChecklistSearchFilter filter)
    {
        var query = filter.ToQuery<Checklist>();

        var countResponse = await Client.CountAsync<Checklist>(IndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Filter(query.ToArray())
                )
            )
        );
        countResponse.ThrowIfInvalid();

        return Convert.ToInt32(countResponse.Count);
    }

    /// <summary>
    /// Count number of present observations matching the search filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<int> GetPresentCountAsync(ChecklistSearchFilter filter)
    {
        var queries = filter.ToQuery<Checklist>();
        queries.TryAddTermsCriteria("taxonIdsFound", filter.Taxa?.Ids);

        var countResponse = await Client.CountAsync<Checklist>(IndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Filter(queries.ToArray())
                )
            )
        );
        countResponse.ThrowIfInvalid();

        return Convert.ToInt32(countResponse.Count);
    }

    /// <summary>
    /// Count number of absent observations (Using taxonIdsFound property) matching the search filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<int> GetAbsentCountAsync(ChecklistSearchFilter filter)
    {
        var queries = filter.ToQuery<Checklist>();
        var nonQueries = new List<Action<QueryDescriptor<Checklist>>>();
        nonQueries.TryAddTermsCriteria("taxonIdsFound", filter.Taxa?.Ids);

        var countResponse = await Client.CountAsync<Checklist>(IndexName, s => s
            .Query(q => q
                .Bool(b => b
                    .Filter(queries.ToArray())
                    .MustNot(nonQueries.ToArray())
                )
            )
        );
        countResponse.ThrowIfInvalid();

        return Convert.ToInt32(countResponse.Count);
    }

    /// <inheritdoc />
    public string UniqueIndexName => IndexHelper.GetIndexName<Checklist>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

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