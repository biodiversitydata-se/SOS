using AgileObjects.AgileMapper.Extensions;
using CSharpFunctionalExtensions;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataQuality;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch.Fluent;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedObservationCoreRepository : ProcessedObservationBaseRepository,
        IProcessedObservationCoreRepository
    {
        private const int ElasticSearchMaxRecords = 10000;
        protected readonly ITaxonManager _taxonManager;

        /// <summary>
        /// Add the collection
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private async Task<bool> AddCollectionAsync(bool protectedIndex)
        {
            var createIndexResponse = await Client.Indices.CreateAsync<Observation>(protectedIndex ? ProtectedIndexName : PublicIndexName, i => i
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Settings(s => s
                    .NumberOfShards(protectedIndex ? NumberOfShardsProtected : NumberOfShards)
                    .NumberOfReplicas(NumberOfReplicas)
                    .MaxTermsCount(110000)
                    .MaxResultWindow(100000)
                )
                .Mappings(map => map
                .Properties(ps => ps
                    .KeywordVal(kwlc => kwlc.Id, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.DynamicProperties, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.InformationWithheld, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.BibliographicCitation, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.CollectionId, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.CollectionCode, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.DataGeneralizations, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.DatasetId, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.DatasetName, IndexSetting.SearchSortAggregate) // WFS
                    .KeywordVal(kwlc => kwlc.InstitutionId, IndexSetting.SearchOnly)
                    .KeywordVal(kwlc => kwlc.Language, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.License, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.OwnerInstitutionCode, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.PrivateCollection, IndexSetting.SearchOnly)
                    .KeywordVal(kwlc => kwlc.PublicCollection, IndexSetting.SearchOnly)
                    .KeywordVal(kwlc => kwlc.References, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.RightsHolder, IndexSetting.None)
                    .KeywordVal(kwlc => kwlc.SpeciesCollectionLabel, IndexSetting.SearchOnly)
                    .ByteNumber(b => b.DiffusionStatus, c => c.Index(true))
                    .BooleanVal(b => b.HasGeneralizedObservationInOtherIndex, IndexSetting.SearchOnly)
                    .BooleanVal(b => b.IsGeneralized, IndexSetting.SearchOnly)
                    .BooleanVal(b => b.Protected, IndexSetting.None)
                    .BooleanVal(b => b.Sensitive, IndexSetting.SearchOnly)
                    .Object(c => c.ProjectsSummary, c => c.Properties(ps => ps
                        .NumberVal(n => n.ProjectsSummary.Project1Id, IndexSetting.SearchOnly, NumberType.Integer) // WFS
                        .NumberVal(n => n.ProjectsSummary.Project2Id, IndexSetting.SearchOnly, NumberType.Integer) // WFS
                        .KeywordVal(kwlc => kwlc.ProjectsSummary.Project1Name, IndexSetting.SearchSortAggregate) // WFS
                        .KeywordVal(kwlc => kwlc.ProjectsSummary.Project1Category, IndexSetting.SearchOnly) // WFS
                        .KeywordVal(kwlc => kwlc.ProjectsSummary.Project1Url, IndexSetting.SearchOnly) // WFS
                        .KeywordVal(kwlc => kwlc.ProjectsSummary.Project1Values, IndexSetting.SearchOnly) // WFS
                        .KeywordVal(kwlc => kwlc.ProjectsSummary.Project2Name, IndexSetting.SearchOnly) // WFS
                        .KeywordVal(kwlc => kwlc.ProjectsSummary.Project2Category, IndexSetting.SearchOnly) // WFS
                        .KeywordVal(kwlc => kwlc.ProjectsSummary.Project2Url, IndexSetting.SearchOnly) // WFS
                        .KeywordVal(kwlc => kwlc.ProjectsSummary.Project2Values, IndexSetting.SearchOnly) // WFS
                    ))
                    .Object(o => o.MeasurementOrFacts, n => n
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementAccuracy, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementDeterminedBy, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementDeterminedDate, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementID, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementMethod, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementRemarks, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementType, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementTypeID, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementUnit, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementUnitID, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementValue, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().MeasurementValueID, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.MeasurementOrFacts.First().OccurrenceID, IndexSetting.None)
                        )
                    )
                    .Object(o => o.Projects, n => n
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.Projects.First().Category, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Projects.First().CategorySwedish, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Projects.First().Name, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Projects.First().Owner, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Projects.First().ProjectURL, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Projects.First().Description, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Projects.First().SurveyMethod, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Projects.First().SurveyMethodUrl, IndexSetting.None)
                            .DateVal(d => d.Projects.First().StartDate, IndexSetting.None)
                            .DateVal(d => d.Projects.First().EndDate, IndexSetting.None)
                            .BooleanVal(b => b.Projects.First().IsPublic, IndexSetting.None)
                            .Object(o => o.Projects.First().ProjectParameters,
                                p => p.Properties(p => p
                                    .KeywordVal(kwlc => kwlc.Projects.First().ProjectParameters.First().DataType, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Projects.First().ProjectParameters.First().Name, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Projects.First().ProjectParameters.First().Unit, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Projects.First().ProjectParameters.First().Description, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Projects.First().ProjectParameters.First().Value, IndexSetting.None)
                                )
                            )
                        )
                    )
                    .Object(o => o.AccessRights, c => c
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.AccessRights.Value, IndexSetting.SearchOnly)
                            .NumberVal(nr => nr.AccessRights.Id, IndexSetting.SearchSortAggregate, NumberType.Byte)
                        )
                    )
                    .Object(o => o.DataStewardship, c => c
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.DataStewardship.DatasetIdentifier, IndexSetting.SearchSortAggregate)
                            .KeywordVal(kwlc => kwlc.DataStewardship.DatasetTitle, IndexSetting.None)
                        )
                    )
                    .Object(o => o.ArtportalenInternal, t => t
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.ArtportalenInternal.SightingBarcodeURL, IndexSetting.SearchOnly)
                            .KeywordVal(kwlc => kwlc.ArtportalenInternal.BirdValidationAreaIds, IndexSetting.SearchOnly)
                            .KeywordVal(kwlc => kwlc.ArtportalenInternal.LocationPresentationNameParishRegion, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.ArtportalenInternal.ParentLocality, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.ArtportalenInternal.ReportedByUserAlias, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.ArtportalenInternal.Summary, IndexSetting.None)
                            .BooleanVal(b => b.ArtportalenInternal.SecondHandInformation, IndexSetting.SearchOnly)
                            .BooleanVal(b => b.ArtportalenInternal.HasAnyTriggeredVerificationRuleWithWarning, IndexSetting.SearchOnly)
                            .BooleanVal(b => b.ArtportalenInternal.HasTriggeredVerificationRules, IndexSetting.SearchOnly)
                            .BooleanVal(b => b.ArtportalenInternal.HasUserComments, IndexSetting.SearchOnly)
                            .BooleanVal(b => b.ArtportalenInternal.IncrementalHarvested, IndexSetting.SearchOnly)
                            .BooleanVal(b => b.ArtportalenInternal.NoteOfInterest, IndexSetting.SearchOnly)
                            .NumberVal(n => n.ArtportalenInternal.ChecklistId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.FieldDiaryGroupId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.ParentLocationId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.ReportedByUserId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.ReportedByUserServiceUserId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.SightingId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.SightingPublishTypeIds, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.SightingTypeId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.SightingTypeSearchGroupId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.SpeciesFactsIds, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.TriggeredObservationRuleFrequencyId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.TriggeredObservationRuleReproductionId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.TriggeredObservationRuleActivityRuleId, IndexSetting.None, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.TriggeredObservationRulePeriodRuleId, IndexSetting.None, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.TriggeredObservationRulePromptRuleId, IndexSetting.None, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.TriggeredObservationRuleRegionalSightingState, IndexSetting.None, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.TriggeredObservationRuleStatusRuleId, IndexSetting.None, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.ActivityCategoryId, IndexSetting.None, NumberType.Integer)
                            .BooleanVal(b => b.ArtportalenInternal.TriggeredObservationRulePrompts, IndexSetting.None)
                            .BooleanVal(b => b.ArtportalenInternal.TriggeredObservationRuleUnspontaneous, IndexSetting.None)
                            .NumberVal(n => n.ArtportalenInternal.SightingSpeciesCollectionItemId, IndexSetting.None, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.DiffusionId, IndexSetting.None, NumberType.Integer)
                            .NumberVal(n => n.ArtportalenInternal.IncludedByLocationId, IndexSetting.None, NumberType.Integer)
                            .Object(o => o.ArtportalenInternal.OccurrenceRecordedByInternal, n => n
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.ArtportalenInternal.OccurrenceRecordedByInternal.First().UserAlias, IndexSetting.None)
                                    .NumberVal(n => n.ArtportalenInternal.OccurrenceRecordedByInternal.First().Id, IndexSetting.SearchOnly, NumberType.Integer)
                                    .NumberVal(n => n.ArtportalenInternal.OccurrenceRecordedByInternal.First().PersonId, IndexSetting.None, NumberType.Integer)
                                    .NumberVal(n => n.ArtportalenInternal.OccurrenceRecordedByInternal.First().UserServiceUserId, IndexSetting.SearchOnly, NumberType.Integer)
                                    .BooleanVal(b => b.ArtportalenInternal.OccurrenceRecordedByInternal.First().Discover, IndexSetting.None)
                                    .BooleanVal(b => b.ArtportalenInternal.OccurrenceRecordedByInternal.First().ViewAccess, IndexSetting.SearchOnly)
                                )
                            )
                            .Object(o => o.ArtportalenInternal.OccurrenceVerifiedByInternal, n => n
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.ArtportalenInternal.OccurrenceVerifiedByInternal.First().UserAlias, IndexSetting.None)
                                    .NumberVal(n => n.ArtportalenInternal.OccurrenceVerifiedByInternal.First().Id, IndexSetting.SearchOnly, NumberType.Integer)
                                    .NumberVal(n => n.ArtportalenInternal.OccurrenceVerifiedByInternal.First().PersonId, IndexSetting.None, NumberType.Integer)
                                    .NumberVal(n => n.ArtportalenInternal.OccurrenceVerifiedByInternal.First().UserServiceUserId, IndexSetting.SearchOnly, NumberType.Integer)
                                    .BooleanVal(b => b.ArtportalenInternal.OccurrenceVerifiedByInternal.First().Discover, IndexSetting.None)
                                    .BooleanVal(b => b.ArtportalenInternal.OccurrenceVerifiedByInternal.First().ViewAccess, IndexSetting.SearchOnly)
                                )
                            )
                        )
                    )
                    .Object(o => o.BasisOfRecord, c => c
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.BasisOfRecord.Value, IndexSetting.SearchOnly)
                            .NumberVal(nr => nr.BasisOfRecord.Id, IndexSetting.SearchSortAggregate, NumberType.Byte)
                        )
                    )
                    .Object(o => o.DataQuality, t => t
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.DataQuality.UniqueKey, IndexSetting.None)
                        )
                    )
                    .Object(o => o.Defects, c => c
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.Defects.Keys, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Defects.Values, IndexSetting.None)
                        )
                    )
                    .Object(o => o.Event, t => t
                        .Properties(ps => ps
                            .Date(d => d.Event.EndDate, c => c.Index(true).DocValues(true))
                            .Date(d => d.Event.StartDate, c => c.Index(true).DocValues(true))
                            .ShortNumber(s => s.Event.EndDayOfYear, c => c.Index(true).DocValues(true))
                            .ShortNumber(s => s.Event.StartDayOfYear, c => c.Index(true).DocValues(true))
                            .ShortNumber(s => s.Event.StartDay, c => c.Index(true).DocValues(true))
                            .ShortNumber(s => s.Event.EndDay, c => c.Index(true).DocValues(true))
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
                    .Object(o => o.GeologicalContext, c => c
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.GeologicalContext.Bed, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.EarliestAgeOrLowestStage, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.EarliestEonOrLowestEonothem, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.EarliestEpochOrLowestSeries, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.EarliestEraOrLowestErathem, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.EarliestGeochronologicalEra, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.EarliestPeriodOrLowestSystem, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.Formation, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.GeologicalContextId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.Group, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.HighestBiostratigraphicZone, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.LatestAgeOrHighestStage, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.LatestEonOrHighestEonothem, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.LatestEpochOrHighestSeries, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.LatestEraOrHighestErathem, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.LatestGeochronologicalEra, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.LatestPeriodOrHighestSystem, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.LithostratigraphicTerms, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.LowestBiostratigraphicZone, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.GeologicalContext.Member, IndexSetting.None)
                        )
                    )
                    .Object(o => o.Identification, c => c
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.Identification.IdentificationRemarks, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Identification.ConfirmedBy, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Identification.ConfirmedDate, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Identification.DateIdentified, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Identification.IdentificationId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Identification.IdentificationQualifier, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Identification.IdentificationReferences, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Identification.IdentifiedBy, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Identification.TypeStatus, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Identification.VerifiedBy, IndexSetting.None)
                            .BooleanVal(b => b.Identification.UncertainIdentification, IndexSetting.SearchOnly)
                            .BooleanVal(b => b.Identification.Validated, IndexSetting.None)
                            .BooleanVal(b => b.Identification.Verified, IndexSetting.SearchSortAggregate)
                            .Object(o => o.Identification.DeterminationMethod, c => c
                            .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Identification.DeterminationMethod.Value, IndexSetting.SearchOnly)
                                    .NumberVal(nr => nr.Identification.DeterminationMethod.Id, IndexSetting.SearchSortAggregate, NumberType.Byte)
                                )
                            )
                            .Object(o => o.Identification.ValidationStatus, c => c
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Identification.ValidationStatus.Value, IndexSetting.SearchOnly)
                                    .NumberVal(nr => nr.Identification.ValidationStatus.Id, IndexSetting.SearchSortAggregate, NumberType.Byte)
                                )
                            )
                            .Object(o => o.Identification.VerificationStatus, c => c
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Identification.VerificationStatus.Value, IndexSetting.SearchOnly)
                                    .NumberVal(nr => nr.Identification.VerificationStatus.Id, IndexSetting.SearchSortAggregate, NumberType.Byte)
                                )
                            )
                        )
                    )
                    .Object(o => o.InstitutionCode, c => c
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.AccessRights.Value, IndexSetting.SearchOnly)
                            .NumberVal(nr => nr.AccessRights.Id, IndexSetting.SearchSortAggregate, NumberType.Short)
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
                    .Object(o => o.MaterialSample, c => c
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.MaterialSample.MaterialSampleId, IndexSetting.None)
                        )
                    )
                    .Object(o => o.Occurrence, t => t
                        .Properties(ps => ps
                            .DateVal(d => d.Occurrence.ReportedDate, IndexSetting.SearchSortAggregate)
                            .KeywordVal(kwlc => kwlc.Occurrence.AssociatedMedia, IndexSetting.SearchOnly)
                            .KeywordVal(kwlc => kwlc.Occurrence.AssociatedOccurrences, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Occurrence.AssociatedReferences, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Occurrence.AssociatedSequences, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Occurrence.AssociatedTaxa, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Occurrence.BiotopeDescription, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Occurrence.RecordedBy, IndexSetting.SearchSortAggregate)
                            .KeywordVal(kwlc => kwlc.Occurrence.CatalogNumber, IndexSetting.SearchSortAggregate)
                            .KeywordVal(kwlc => kwlc.Occurrence.Disposition, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Occurrence.IndividualCount, IndexSetting.SearchSortAggregate) // Can we set this to: IndexSetting.None? Printobs2 references this property but should rather use OrganismQuantityInt or OrganismQuantity.
                            .KeywordVal(kwlc => kwlc.Occurrence.OccurrenceId, IndexSetting.SearchSortAggregate)
                            .KeywordVal(kwlc => kwlc.Occurrence.OrganismQuantity, IndexSetting.SearchSortAggregate)
                            .KeywordVal(kwlc => kwlc.Occurrence.OtherCatalogNumbers, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Occurrence.Preparations, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Occurrence.RecordNumber, IndexSetting.SearchOnly)
                            .KeywordVal(kwlc => kwlc.Occurrence.ReportedBy, IndexSetting.SearchSortAggregate)
                            .KeywordVal(kwlc => kwlc.Occurrence.Url, IndexSetting.None)
                            .NumberVal(n => n.Occurrence.SensitivityCategory, IndexSetting.SearchSortAggregate, NumberType.Integer)
                            .NumberVal(n => n.Occurrence.BirdNestActivityId, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.Occurrence.Length, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.Occurrence.Weight, IndexSetting.SearchOnly, NumberType.Integer)
                            .NumberVal(n => n.Occurrence.CatalogId, IndexSetting.SearchSortAggregate, NumberType.Integer)
                            .NumberVal(n => n.Occurrence.OrganismQuantityInt, IndexSetting.SearchSortAggregate, NumberType.Integer)
                            .BooleanVal(b => b.Occurrence.IsNaturalOccurrence, IndexSetting.SearchOnly)
                            .BooleanVal(b => b.Occurrence.IsNeverFoundObservation, IndexSetting.SearchOnly)
                            .BooleanVal(b => b.Occurrence.IsNotRediscoveredObservation, IndexSetting.SearchOnly)
                            .BooleanVal(b => b.Occurrence.IsPositiveObservation, IndexSetting.SearchOnly)
                            .Object(o => o.Occurrence.Media, n => n
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Description, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Audience, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Contributor, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Created, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Creator, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().DatasetID, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Format, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Identifier, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().License, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Publisher, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().References, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().RightsHolder, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Source, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Title, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Type, IndexSetting.None)
                                    .Object(o => o.Occurrence.Media.First().Comments, mc => mc
                                        .Properties(ps => ps
                                            .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Comments.First().Comment, IndexSetting.None)
                                            .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Comments.First().CommentBy, IndexSetting.None)
                                            .KeywordVal(kwlc => kwlc.Occurrence.Media.First().Comments.First().Created, IndexSetting.None)
                                        )
                                    )
                                )
                            )
                            .Object(o => o.Occurrence.OccurrenceStatus, c => c
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.OccurrenceStatus.Value, IndexSetting.None)
                                    .NumberVal(nr => nr.Occurrence.OccurrenceStatus.Id, IndexSetting.None, NumberType.Byte)
                                )
                            )
                            .Object(o => o.Occurrence.Activity, c => c
                            .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.Activity.Value, IndexSetting.SearchOnly)
                                    .NumberVal(nr => nr.Occurrence.Activity.Id, IndexSetting.SearchSortAggregate, NumberType.Integer)
                                )
                            )
                            .Object(o => o.Occurrence.Behavior, c => c
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.Behavior.Value, IndexSetting.None)
                                    .NumberVal(nr => nr.Occurrence.Behavior.Id, IndexSetting.None, NumberType.Integer)
                                )
                            )
                            .Object(o => o.Occurrence.Biotope, c => c
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.Biotope.Value, IndexSetting.None)
                                    .NumberVal(nr => nr.Occurrence.Biotope.Id, IndexSetting.SearchSortAggregate, NumberType.Integer)
                                )
                            )
                            .Object(o => o.Occurrence.EstablishmentMeans, c => c
                            .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.EstablishmentMeans.Value, IndexSetting.None)
                                    .NumberVal(nr => nr.Occurrence.EstablishmentMeans.Id, IndexSetting.None, NumberType.Integer)
                                )
                            )
                            .Object(o => o.Occurrence.LifeStage, c => c
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.LifeStage.Value, IndexSetting.SearchOnly)
                                    .NumberVal(nr => nr.Occurrence.LifeStage.Id, IndexSetting.SearchSortAggregate, NumberType.Integer)
                                )
                            )
                            .Object(o => o.Occurrence.OrganismQuantityUnit, c => c
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.OrganismQuantityUnit.Value, IndexSetting.SearchOnly)
                                    .NumberVal(nr => nr.Occurrence.OrganismQuantityUnit.Id, IndexSetting.None, NumberType.Integer)
                                )
                            )
                            .Object(o => o.Occurrence.ReproductiveCondition, c => c
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.ReproductiveCondition.Value, IndexSetting.None)
                                    .NumberVal(nr => nr.Occurrence.ReproductiveCondition.Id, IndexSetting.None, NumberType.Integer)
                                )
                            )
                            .Object(o => o.Occurrence.Sex, c => c
                            .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.Sex.Value, IndexSetting.SearchOnly)
                                    .NumberVal(nr => nr.Occurrence.Sex.Id, IndexSetting.SearchSortAggregate, NumberType.Integer)
                                )
                            )
                            .Object(o => o.Occurrence.Substrate, c => c
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Occurrence.Substrate.SpeciesScientificName, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Substrate.Description, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Substrate.SpeciesDescription, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Substrate.SubstrateDescription, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Occurrence.Substrate.SpeciesVernacularName, IndexSetting.None)
                                    .NumberVal(n => n.Occurrence.Substrate.Quantity, IndexSetting.None, NumberType.Integer)
                                    .NumberVal(n => n.Occurrence.Substrate.SpeciesId, IndexSetting.SearchOnly, NumberType.Integer)
                                    .NumberVal(n => n.Occurrence.Substrate.Id, IndexSetting.SearchOnly, NumberType.Integer)
                                    .Object(o => o.Occurrence.Substrate.Name, c => c
                                        .Properties(ps => ps
                                            .KeywordVal(kwlc => kwlc.Occurrence.Substrate.Name.Value, IndexSetting.SearchOnly)
                                            .NumberVal(nr => nr.Occurrence.Substrate.Name.Id, IndexSetting.SearchSortAggregate, NumberType.Integer)
                                        )
                                    )
                                )
                            )
                            .Text(o => o.Occurrence.OccurrenceRemarks, t => t
                                .IndexOptions(IndexOptions.Docs)
                            )
                        )
                    )
                    .Object(o => o.Organism, c => c
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.Organism.AssociatedOrganisms, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Organism.OrganismId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Organism.OrganismName, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Organism.OrganismRemarks, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Organism.OrganismScope, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Organism.PreviousIdentifications, IndexSetting.None)
                        )
                    )
                    .Object(o => o.Taxon, t => t
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.Taxon.AcceptedNameUsage, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.AcceptedNameUsageId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.Class, IndexSetting.None)
                            .KeywordVal(kw => kw.Taxon.DisplayName, IndexSetting.None, Normalizer.None)
                            .KeywordVal(kwlc => kwlc.Taxon.NomenclaturalCode, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.NomenclaturalStatus, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.Order, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.TaxonId, IndexSetting.SearchSortAggregate)
                            .KeywordVal(kwlc => kwlc.Taxon.TaxonRemarks, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.VerbatimTaxonRank, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.Family, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.Genus, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.HigherClassification, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.InfraspecificEpithet, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.Kingdom, IndexSetting.SearchOnly)
                            .KeywordVal(kwlc => kwlc.Taxon.NameAccordingTo, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.NameAccordingToId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.NamePublishedIn, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.NamePublishedInId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.NamePublishedInYear, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.OriginalNameUsage, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.OriginalNameUsageId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.ParentNameUsage, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.ParentNameUsageId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.Phylum, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.ScientificName, IndexSetting.SearchSortAggregate)
                            .KeywordVal(kwlc => kwlc.Taxon.ScientificNameAuthorship, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.ScientificNameId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.SpecificEpithet, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.Subgenus, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.TaxonConceptId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.TaxonomicStatus, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.TaxonRank, IndexSetting.SearchOnly)
                            .KeywordVal(kwlc => kwlc.Taxon.VerbatimId, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.VerbatimName, IndexSetting.None)
                            .KeywordVal(kwlc => kwlc.Taxon.VernacularName, IndexSetting.SearchSortAggregate)
                            .NumberVal(n => n.Taxon.Id, IndexSetting.SearchSortAggregate, NumberType.Integer)
                            .NumberVal(n => n.Taxon.SecondaryParentDyntaxaTaxonIds, IndexSetting.None, NumberType.Integer)
                            .BooleanVal(b => b.Taxon.BirdDirective, IndexSetting.SearchOnly)
                            .Object(o => o.Taxon.Attributes, a => a
                                .Properties(ps => ps
                                    .KeywordVal(kwlc => kwlc.Taxon.Attributes.ActionPlan, IndexSetting.SearchOnly) // WFS
                                    .KeywordVal(kwlc => kwlc.Taxon.Attributes.OrganismGroup, IndexSetting.SearchSortAggregate) // WFS
                                    .KeywordVal(kwlc => kwlc.Taxon.Attributes.InvasiveRiskAssessmentCategory, IndexSetting.SearchOnly) // WFS
                                    .KeywordVal(kwlc => kwlc.Taxon.Attributes.RedlistCategory, IndexSetting.SearchSortAggregate) // WFS
                                    .KeywordVal(kwlc => kwlc.Taxon.Attributes.RedlistCategoryDerived, IndexSetting.SearchSortAggregate)
                                    .KeywordVal(kwlc => kwlc.Taxon.Attributes.SwedishOccurrence, IndexSetting.None)
                                    .KeywordVal(kwlc => kwlc.Taxon.Attributes.SwedishHistory, IndexSetting.None)
                                    .NumberVal(n => n.Taxon.Attributes.DyntaxaTaxonId, IndexSetting.None, NumberType.Integer)
                                    .NumberVal(n => n.Taxon.Attributes.GbifTaxonId, IndexSetting.None, NumberType.Integer)
                                    .NumberVal(n => n.Taxon.Attributes.ParentDyntaxaTaxonId, IndexSetting.None, NumberType.Integer)
                                    .NumberVal(n => n.Taxon.Attributes.DisturbanceRadius, IndexSetting.None, NumberType.Integer)
                                    .NumberVal(n => n.Taxon.Attributes.SortOrder, IndexSetting.SearchSortAggregate, NumberType.Integer)
                                    .NumberVal(n => n.Taxon.Attributes.SpeciesGroup, IndexSetting.None, NumberType.Byte)
                                    .BooleanVal(n => n.Taxon.Attributes.IsRedlisted, IndexSetting.SearchOnly) // WFS
                                    .BooleanVal(n => n.Taxon.Attributes.IsInvasiveInSweden, IndexSetting.SearchOnly) // WFS
                                    .BooleanVal(n => n.Taxon.Attributes.IsInvasiveAccordingToEuRegulation, IndexSetting.SearchOnly) // WFS
                                    .BooleanVal(n => n.Taxon.Attributes.Natura2000HabitatsDirectiveArticle2, IndexSetting.SearchOnly) // WFS
                                    .BooleanVal(n => n.Taxon.Attributes.Natura2000HabitatsDirectiveArticle4, IndexSetting.SearchOnly) // WFS
                                    .BooleanVal(n => n.Taxon.Attributes.Natura2000HabitatsDirectiveArticle5, IndexSetting.SearchOnly) // WFS
                                    .BooleanVal(n => n.Taxon.Attributes.ProtectedByLaw, IndexSetting.SearchOnly) // WFS                        
                                    .Object(o => o.Taxon.Attributes.ProtectionLevel, t => t
                                        .Properties(ps => ps
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.ProtectionLevel.Value, IndexSetting.None)
                                            .NumberVal(n => n.Taxon.Attributes.ProtectionLevel.Id, IndexSetting.None, NumberType.Byte)
                                        )
                                    )
                                   
                                    .Object(o => o.Taxon.Attributes.ScientificNames, n => n
                                        .Properties(ps => ps
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.ScientificNames.First().Author, IndexSetting.None)
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.ScientificNames.First().Name, IndexSetting.None)
                                            .BooleanVal(b => b.Taxon.Attributes.ScientificNames.First().IsPreferredName, IndexSetting.None)
                                            .BooleanVal(b => b.Taxon.Attributes.ScientificNames.First().ValidForSighting, IndexSetting.None)
                                        )
                                    )
                                    .Object(o => o.Taxon.Attributes.SensitivityCategory, t => t
                                        .Properties(ps => ps
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.SensitivityCategory.Value, IndexSetting.SearchOnly)
                                            .NumberVal(n => n.Taxon.Attributes.SensitivityCategory.Id, IndexSetting.SearchSortAggregate, NumberType.Byte)
                                        )
                                    )
                                    .Object(o => o.Taxon.Attributes.Synonyms, n => n
                                        .Properties(ps => ps
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.Synonyms.First().Author, IndexSetting.None)
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.Synonyms.First().Name, IndexSetting.None)
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.Synonyms.First().NomenclaturalStatus, IndexSetting.None)
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.Synonyms.First().TaxonomicStatus, IndexSetting.None)
                                        )
                                    )
                                    .Object(o => o.Taxon.Attributes.TaxonCategory, c => c
                                        .Properties(ps => ps
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.TaxonCategory.Value, IndexSetting.SearchOnly)
                                            .NumberVal(nr => nr.Taxon.Attributes.TaxonCategory.Id, IndexSetting.None, NumberType.Byte)
                                        )
                                    )
                                    .Object(o => o.Taxon.Attributes.VernacularNames, n => n
                                        .Properties(ps => ps
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.VernacularNames.First().CountryCode, IndexSetting.None)
                                            .BooleanVal(b => b.Taxon.Attributes.VernacularNames.First().IsPreferredName, IndexSetting.None)
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.VernacularNames.First().Name, IndexSetting.None)
                                            .KeywordVal(kwlc => kwlc.Taxon.Attributes.VernacularNames.First().Language, IndexSetting.None)
                                            .BooleanVal(b => b.Taxon.Attributes.VernacularNames.First().ValidForSighting, IndexSetting.None)
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .Object(o => o.Type, c => c
                        .Properties(ps => ps
                            .KeywordVal(kwlc => kwlc.Type.Value, IndexSetting.None)
                            .NumberVal(nr => nr.Type.Id, IndexSetting.None, NumberType.Byte)
                        )
                    )
                    )
                 )
             );

            return createIndexResponse.Acknowledged && createIndexResponse.IsValidResponse ? true : throw new Exception($"Failed to create observation index. Error: {createIndexResponse.DebugInformation}");
        }

        /// <summary>
        /// Make sure Elasticserach nodes are up
        /// </summary>
        /// <param name="clusterCount"></param>
        private async Task CheckNodesAsync(int clusterCount)
        {
            var checkNodeTasks = new[]
            {
                CheckNodeAsync(PublicIndexName, Math.Max(1, clusterCount - 1)), // Subtract 1 since we are using replicas in prod
                CheckNodeAsync(ProtectedIndexName, Math.Max(1, clusterCount - 1)) // Subtract 1 since we are using replicas in prod
            };
            await Task.WhenAll(checkNodeTasks);
        }

        /// <summary>
        /// Make sure all clusters are available
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="minClusterCount"></param>
        /// <exception cref="Exception"></exception>
        private async Task CheckNodeAsync(string indexName, int minClusterCount)
        {
            var clusterHealthDictionary = _clusterHealthCache.Get();
            if (clusterHealthDictionary == null)
            {
                clusterHealthDictionary = new ConcurrentDictionary<string, HealthResponse>();
                _clusterHealthCache.Set(clusterHealthDictionary);
            }

            HealthResponse health;
            if (clusterHealthDictionary.TryGetValue(indexName, out var clusterHealth))
            {
                health = clusterHealth;
            }
            else
            {
                health = await Client.Cluster.HealthAsync(indexName);
                clusterHealthDictionary.TryAdd(indexName, health);
            }

            if (health.NumberOfDataNodes < minClusterCount)
            {
                throw new Exception($"Expected at least {minClusterCount} nodes, found {health.NumberOfDataNodes}.");
            }
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

        public async Task<bool> DeleteAsync(ICollection<Action<QueryDescriptor<Observation>>> queries, bool protectedIndex)
        {
            try
            {
                // Create the collection
                var res = await Client.DeleteByQueryAsync<Observation>(protectedIndex ? ProtectedIndexName : PublicIndexName, q => q
                    .Query(q => q
                        .Bool(b => b
                            .Filter(queries.ToArray())
                        )
                    )
                );

                return res.IsValidResponse;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
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
                        .Indices(protectedIndex ? ProtectedIndexName : PublicIndexName)
                        .Query(q => q
                            .Terms(t => t
                                .Field("occurrence.occurrenceId")
                                .Terms(duplicates.ToTermsQueryField())
                            )
                        )
                        .Sort(s => s
                            .Field("occurrence.occurrenceId".ToField(), c => c.Order(SortOrder.Asc))
                            .Field("modified".ToField(), c => c.Order(SortOrder.Desc))
                        )
                        .Size(duplicates.Count() * 10) // It's not likely that average numbers of duplicates exceeds 10
                        .Source((Includes: new[] { "occurrence.occurrenceId" }, Excludes: Array.Empty<string>()).ToProjection())          
                        .TrackTotalHits(new TrackHits(false))
                    );

                    searchResponse.ThrowIfInvalid();
                    var observations = searchResponse.Documents.Cast<IDictionary<string, object>>().ToArray();
                    var idsToRemove = new HashSet<string>();
                    var prevOccurrenceId = string.Empty;
                    foreach (var hit in searchResponse.Hits)
                    {
                        var occurrenceId = hit.Sort.First().Value.ToString();
                        // Remove all but first occurrence of occurrenceId (latest data)
                        if (occurrenceId == prevOccurrenceId)
                        {
                            idsToRemove.Add(hit.Id);
                        }
                        prevOccurrenceId = occurrenceId;
                    }
                    var deleteQueries = new List<Action<QueryDescriptor<Observation>>>();
                    deleteQueries.TryAddTermCriteria("_id", idsToRemove);

                    await DeleteAsync(deleteQueries, protectedIndex);
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
                var res = await Client.SearchAsync<Observation>(protectedIndex ? ProtectedIndexName : PublicIndexName, s => s
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.DataProviderId)
                            .Value(providerId)
                        )
                    )
                    .Aggregations(a => a
                        .Add("latestModified", agg => agg
                        .Max(m => m
                                .Field(f => f.Modified)
                            )
                        )
                    )
                    .AddDefaultAggrigationSettings()
                );

                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var latestModified = epoch.AddMilliseconds(res.Aggregations?.GetMax("latestModified").Value ?? 0);
                // If there is incorrect data from the futeure, get data from 5 min back in time
                return latestModified > DateTime.UtcNow ? DateTime.UtcNow.AddMinutes(-5) : latestModified;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to get last modified date for provider: {providerId}, index: {(protectedIndex ? ProtectedIndexName : PublicIndexName)}");
                return DateTime.MinValue;
            }
        }

        private FluentDescriptorDictionary<Field, RuntimeFieldDescriptor<dynamic>> GetRuntimeMappingScriptField(string fieldName, string scriptFieldName)
        {
            var runtimeFieldDescriptor = new RuntimeFieldDescriptor<dynamic>();
            runtimeFieldDescriptor.Script(new Script()
            {
                Id = scriptFieldName,
                Source = @$"
                    if (!doc['{fieldName}'].empty){{  
                        String value = '' + doc['{fieldName}'].value; 
                        if (value != '') {{ 
                            emit(value); 
                        }} 
                    }}",
            });
            runtimeFieldDescriptor.Type(RuntimeFieldType.Keyword);
            return new FluentDescriptorDictionary<Field, RuntimeFieldDescriptor<dynamic>>
            {
                { scriptFieldName.ToField(), runtimeFieldDescriptor }
            };
        }

        private async Task<SearchResponse<dynamic>> PageAggregationItemAsync(
           string indexName,
           string aggregationField,
           ICollection<Action<QueryDescriptor<dynamic>>> queryDescriptors,
           ICollection<Action<QueryDescriptor<object>>> excludeQueryDescriptors,
           IReadOnlyDictionary<Field, FieldValue> nextPageKey,
           int take)
        {
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
               .Indices(indexName)
               .Query(q => q
                   .Bool(b => b
                       .MustNot(excludeQueryDescriptors?.ToArray())
                       .Filter(queryDescriptors.ToArray())
                   )
               )
               .Aggregations(a => a
                   .Add("compositeAggregation", ca => ca
                       .Composite(c => c
                           .After(a => nextPageKey.ToFluentDictionary())
                           .Size(take)
                           .Sources(
                                [
                                    CreateCompositeTermsAggregationSource(
                                        ("termAggregation", "aggregationField", SortOrder.Asc)
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

        private async Task<SearchResponse<dynamic>> PageAggregationItemListAsync(
            string indexName,
            string aggregationFieldKey,
            string aggregationFieldList,
            ICollection<Action<QueryDescriptor<dynamic>>> queryDescriptors,
            ICollection<Action<QueryDescriptor<object>>> excludeQueryDescriptors,
            IReadOnlyDictionary<Field, FieldValue> nextPageKey,
            int take)
        {
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueryDescriptors?.ToArray())
                        .Filter(queryDescriptors?.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("compositeAggregation", ca => ca
                        .Composite(c => c
                            .After(a => nextPageKey.ToFluentDictionary())
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="queryDescriptors"></param>
        /// <param name="excludeQueryDescriptors"></param>
        /// <param name="nextPageKey"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        private async Task<SearchResponse<dynamic>> PageEventOccurrenceItemAsync(
            string indexName,
            ICollection<Action<QueryDescriptor<dynamic>>> queryDescriptors,
            ICollection<Action<QueryDescriptor<object>>> excludeQueryDescriptors,
            IReadOnlyDictionary<Field, FieldValue> nextPageKey,
            int take)
        {
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                 .Indices(indexName)
                 .Query(q => q
                     .Bool(b => b
                         .MustNot(excludeQueryDescriptors?.ToArray())
                         .Filter(queryDescriptors?.ToArray())
                     )
                 )
                 .Aggregations(a => a
                      .Add("compositeAggregation", ca => ca
                         .Composite(c => c
                             .After(a => nextPageKey.ToFluentDictionary())
                             .Size(take)
                             .Sources(
                                [
                                    CreateCompositeTermsAggregationSource(
                                        ("eventId", "event.eventId", SortOrder.Asc)
                                    ),
                                     CreateCompositeTermsAggregationSource(
                                        ("occurrenceId", "occurrence.occurrenceId", SortOrder.Asc)
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

        /// <summary>
        /// Populate sortable fields
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="sortableFields"></param>
        /// <param name="parents"></param>
        private void PopulateSortableFields(Properties properties, ref HashSet<string> sortableFields, string parents)
        {
            foreach (var property in properties)
            {
                var name = $"{(string.IsNullOrEmpty(parents) ? "" : $"{parents}.")}{property.Key.Name}";

                if (property.Value is ObjectProperty op)
                {
                    PopulateSortableFields(op.Properties, ref sortableFields, name);
                }
                if (
                    property.Value is BooleanProperty bp && bp.Index == null && bp.DocValues == null ||
                    property.Value is DateProperty dp && dp.Index == null && dp.DocValues == null ||
                    property.Value is KeywordProperty kwp && kwp.Index == null && kwp.DocValues == null ||
                    property.Value is ByteNumberProperty bnp && bnp.Index == null && bnp.DocValues == null ||
                    property.Value is DoubleNumberProperty dnp && dnp.Index == null && dnp.DocValues == null ||
                    property.Value is FloatNumberProperty fnp && fnp.Index == null && fnp.DocValues == null ||
                    property.Value is IntegerNumberProperty inp && inp.Index == null && inp.DocValues == null ||
                    property.Value is LongNumberProperty lnp && lnp.Index == null && lnp.DocValues == null ||
                    property.Value is ShortNumberProperty snp && snp.Index == null && snp.DocValues == null
                )
                {
                    sortableFields.Add(name);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="taxonManager"></param>
        /// <param name="clusterHealthCache"></param>
        /// <param name="logger"></param>
        public ProcessedObservationCoreRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ITaxonManager taxonManager,
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            ILogger<ProcessedObservationCoreRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, logger)
        {
            if (elasticConfiguration.Clusters != null)
            {
                Task.Run(async () => await CheckNodesAsync(elasticConfiguration.Clusters.First().Hosts?.Count() ?? 0));
            }

            _taxonManager = taxonManager;
        }

        /// <inheritdoc />
        public async Task<int> AddManyAsync(IEnumerable<Observation> items, bool protectedIndex, bool refreshIndex = false)
        {
            return await AddManyAsync(items, protectedIndex ? ProtectedIndexName : PublicIndexName, refreshIndex);
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
            return await DeleteCollectionAsync(protectedIndex ? ProtectedIndexName : PublicIndexName);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAllDocumentsAsync(bool protectedIndex, bool waitForCompletion = false)
        {
            return await DeleteAllDocumentsAsync(protectedIndex ? ProtectedIndexName : PublicIndexName, waitForCompletion);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteByOccurrenceIdAsync(IEnumerable<string> occurenceIds, bool protectedIndex)
        {
            var deleteQueries = new List<Action<QueryDescriptor<Observation>>>();
            deleteQueries.TryAddTermsCriteria("occurrence.occurrenceId", occurenceIds);

            return await DeleteAsync(deleteQueries, protectedIndex);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteProviderDataAsync(DataProvider dataProvider, bool protectedIndex)
        {
            var deleteQueries = new List<Action<QueryDescriptor<Observation>>>();
            deleteQueries.TryAddTermCriteria("dataProviderId", dataProvider.Id);

            return await DeleteAsync(deleteQueries, protectedIndex);
        }

        /// <inheritdoc />
        public async Task<bool> DisableIndexingAsync(bool protectedIndex)
        {
            return await SetIndexRefreshIntervalAsync(protectedIndex ? ProtectedIndexName : PublicIndexName, Duration.MinusOne);
        }

        /// <inheritdoc />
        public async Task EnableIndexingAsync(bool protectedIndex)
        {
            await SetIndexRefreshIntervalAsync(protectedIndex ? ProtectedIndexName : PublicIndexName, new Duration(5000.0));
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
        public async Task<PagedResult<AggregationItem>> GetAggregationItemsAsync(
            SearchFilter filter,
            string aggregationField,
            int skip = 0,
            int take = 65536,
            int? precisionThreshold = null,
            AggregationSortOrder? sortOrder = AggregationSortOrder.CountDescending,
            bool? useScript = false,
            bool? aggregateCardinality = false,
            bool? aggregateOrganismQuantity = false,
            string fieldType = "string"
        )
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter);
            int size = Math.Max(1, Math.Min(65536, skip + take));
            var termsOrder = sortOrder.HasValue ? sortOrder.Value.GetTermsOrder() : null;
            FluentDescriptorDictionary<Field, RuntimeFieldDescriptor<dynamic>> runtimeMapping = null;
            ;

            if (useScript ?? true)
            {
                var scriptFieldName = "scriptField";
                runtimeMapping = GetRuntimeMappingScriptField(aggregationField, scriptFieldName);
                aggregationField = scriptFieldName;
            }
           
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery.ToArray())
                        .Filter(query.ToArray())
                    )
                )
                .RuntimeMappings(rm => runtimeMapping)
                .Aggregations(a =>
                {
                    a.Add("termAggregation", a =>
                    {
                        a.Terms(t => t
                            .Size(size)
                            .Field(aggregationField)
                            .ValueType(fieldType ?? "string")
                            .Order(termsOrder)
                        );

                        if (aggregateOrganismQuantity ?? false)
                        {
                            a.Aggregations(a => a
                                .Add("totalOrganismQuantity", a => a
                                    .Sum(sa => sa
                                        .Field("occurrence.organismQuantityAggregation")
                                    )
                                )
                            );
                        }
                    });
                    if (aggregateCardinality ?? false)
                    {
                        a.Add("cardinalityAggregation", a => a
                            .Cardinality(c => c
                                .Field(aggregationField)
                                .PrecisionThreshold(precisionThreshold ?? 40000)
                            )
                        );
                    }

                    return a;
                })
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();
            var records = fieldType switch
            {
                "double" => searchResponse.Aggregations.GetDoubleTerms("termAggregation")
                    .Buckets
                        .Select(b => new AggregationItem
                        {
                            AggregationKey = b.Key.ToString(),
                            DocCount = (int)b.DocCount,
                            OrganismQuantity = aggregateOrganismQuantity ?? false ? (int)(b.Aggregations.GetSum("totalOrganismQuantity")?.Value ?? 0) : 0
                        }),
                "long" => searchResponse.Aggregations.GetLongTerms("termAggregation")
                    .Buckets
                        .Select(b => new AggregationItem
                        {
                            AggregationKey = b.Key.ToString(),
                            DocCount = (int)b.DocCount,
                            OrganismQuantity = aggregateOrganismQuantity ?? false ? (int)(b.Aggregations.GetSum("totalOrganismQuantity")?.Value ?? 0) : 0
                        }),
                _ => searchResponse.Aggregations.GetStringTerms("termAggregation")
                    .Buckets
                        .Select(b => new AggregationItem
                        {
                            AggregationKey = b.Key.Value.ToString(),
                            DocCount = (int)b.DocCount,
                            OrganismQuantity = aggregateOrganismQuantity ?? false ? (int)(b.Aggregations.GetSum("totalOrganismQuantity")?.Value ?? 0) : 0
                        })
            };

            var totalCount = 0L;
            totalCount = aggregateCardinality ?? false ? searchResponse.Aggregations.GetCardinality("cardinalityAggregation").Value : searchResponse.Total;

            var result = new PagedResult<AggregationItem>()
            {
                Records = records
                    .Skip(skip)
                    .Take(take),
                Skip = skip,
                Take = take,
                TotalCount = totalCount
            };

            return result;
        }

        public async Task<IEnumerable<AggregationItem>> GetAggregationItemsAggregateOrganismQuantityAsync(SearchFilter filter,
            string aggregationField,
            int? precisionThreshold,
            int size = 65536,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter);
            var termsOrder = sortOrder.GetTermsOrder();
            size = Math.Max(1, size);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery.ToArray())
                        .Filter(query.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("termAggregation", a => a
                        .Terms(t => t
                            .Size(size)
                            .Field(aggregationField)
                            .ValueType("string")
                            .Order(termsOrder)
                        )
                        .Aggregations(a => a
                            .Add("totalOrganismQuantity", a => a
                                .Sum(sa => sa
                                    .Field("occurrence.organismQuantityAggregation")
                                )
                            )
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();
            var result = searchResponse.Aggregations
                .GetStringTerms("termAggregation")
                .Buckets
                    .Select(b => new AggregationItem
                    {
                        AggregationKey = b.Key.Value.ToString(),
                        DocCount = (int)b.DocCount,
                        OrganismQuantity = (int)(b.Aggregations.GetSum("totalOrganismQuantity")?.Value ?? 0)
                    });

            return result;
        }

        /// <inheritdoc />
        public async Task<List<AggregationItem>> GetAllAggregationItemsAsync(SearchFilter filter, string aggregationField)
        {
            var indexName = GetCurrentIndex(filter);
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
                        AggregationKey = bucket.Key["termAggregation"].ToString(),
                        DocCount = (int)bucket.DocCount
                    });
                }

                nextPageKey = compositeAgg.Buckets.Count >= take ? compositeAgg.AfterKey?.ToDictionary(ak => ak.Key.ToField(), ak => ak.Value) : null;
            } while (nextPageKey != null);

            return items;
        }

        /// <inheritdoc />
        public async Task<List<AggregationItemList<TKey, TValue>>> GetAllAggregationItemsListAsync<TKey, TValue>(SearchFilter filter,
            string aggregationFieldKey,
            string aggregationFieldList)
        {
            var indexName = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter);
            var aggregationDictionary = new Dictionary<TKey, List<TValue>>();
            IReadOnlyDictionary<Field, FieldValue> nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageAggregationItemListAsync(indexName, aggregationFieldKey, aggregationFieldList, query, excludeQuery, nextPageKey, pageTaxaAsyncTake);
                var compositeAgg = searchResponse.Aggregations.GetComposite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    TKey keyValue = (TKey)bucket.Key[aggregationFieldKey].Value;
                    TValue listValue = (TValue)bucket.Key[aggregationFieldList].Value;
                    if (!aggregationDictionary.ContainsKey(keyValue))
                        aggregationDictionary[keyValue] = new List<TValue>();
                    aggregationDictionary[keyValue].Add(listValue);
                }

                nextPageKey = compositeAgg.Buckets.Count >= pageTaxaAsyncTake ? compositeAgg.AfterKey?.ToDictionary(ak => ak.Key.ToField(), ak => ak.Value) : null;
            } while (nextPageKey != null);

            var items = aggregationDictionary.Select(m => new AggregationItemList<TKey, TValue> { AggregationKey = m.Key, Items = m.Value }).ToList();
            return items;
        }

        /// <inheritdoc />
        public async Task<PagedResult<T>> GetChunkAsync<T>(SearchFilter filter, int skip, int take, bool getAllFields = false) where T : class
        {
            var indexNames = GetCurrentIndex(filter);

            var (query, excludeQuery) = GetCoreQueries<T>(filter);
            var sortDescriptor = await Client.GetSortDescriptorAsync<T, Observation>(indexNames, filter?.Output?.SortOrders);
            var searchResponse = await Client.SearchAsync<T>(s => s
                .Indices(indexNames)
                .Source(getAllFields ? new SourceConfig(true) : filter.Output?.Fields.ToProjection(filter is SearchFilterInternal))
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery.ToArray())
                        .Filter(query.ToArray())
                    )
                )
                .Sort(sortDescriptor?.ToArray())
            );

            searchResponse.ThrowIfInvalid();

            var totalCount = searchResponse.Total;
            var includeRealCount = totalCount >= ElasticSearchMaxRecords;

            if (filter is SearchFilterInternal internalFilter)
            {
                includeRealCount = internalFilter.IncludeRealCount ?? false;
            }

            if (includeRealCount)
            {
                var countResponse = await Client.CountAsync<T>(indexNames, s => s
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery.ToArray())
                            .Filter(query.ToArray())
                        )
                    )
                );
                countResponse.ThrowIfInvalid();

                totalCount = countResponse.Count;
            }
            return new PagedResult<T>
            {
                Records = searchResponse.Documents,
                Skip = skip,
                Take = take,
                TotalCount = totalCount
            };
        }

        /// <inheritdoc />
        public async Task<List<EventOccurrenceAggregationItem>> GetEventOccurrenceItemsAsync(SearchFilter filter)
        {
            var indexName = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter);
            var occurrencesByEventId = new Dictionary<string, List<string>>();
            IReadOnlyDictionary<Field, FieldValue> nextPageKey = null;
            var take = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageEventOccurrenceItemAsync(indexName, query, excludeQuery, nextPageKey, take);
                var compositeAgg = searchResponse.Aggregations.GetComposite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    string eventId = bucket.Key["eventId"].ToString();
                    string occurrenceId = bucket.Key["occurrenceId"].ToString();
                    if (!occurrencesByEventId.ContainsKey(eventId))
                        occurrencesByEventId[eventId] = new List<string>();
                    occurrencesByEventId[eventId].Add(occurrenceId);
                }


                nextPageKey = compositeAgg.Buckets.Count >= take ? compositeAgg.AfterKey?.ToDictionary(ak => ak.Key.ToField(), ak => ak.Value) : null;
            } while (nextPageKey != null);


            var eventIdItems = occurrencesByEventId.Select(m => new EventOccurrenceAggregationItem { EventId = m.Key, OccurrenceIds = m.Value }).ToList();
            return eventIdItems;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<long>> GetProjectIdsNestedAsync(SearchFilter filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery.ToArray())
                        .Filter(query.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("projects", a => a
                        .Nested(n => n
                            .Path("projects")
                        )
                        .Aggregations(a => a
                            .Add("projectId", a => a
                                .Terms(t => t
                                    .Field("projects.id")
                                )
                            )
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();
            var result = searchResponse.Aggregations
                .GetNested("projects")
                    .Aggregations
                        .GetLongTerms("projectId")
                            .Buckets
                                .Select(b => b.Key);

            return result;
        }

        public async Task<IEnumerable<int>> GetProjectIdsAsync(SearchFilter filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery.ToArray())
                        .Filter(query.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("projectId", a => a
                        .Terms(t => t
                            .Field("projects.id")
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();
            var result = searchResponse.Aggregations
                .GetLongTerms("projectId")
                    .Buckets
                        .Select(b => (int)b.Key);

            return result;
        }

        /// <inheritdoc />
        public async Task<DataQualityReport> GetDataQualityReportAsync(string organismGroup)
        {
            var index = PublicIndexName;

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(index)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f.Term(t => t
                            .Field("taxon.attributes.organismGroup")
                            .Value(organismGroup?.ToLower())))
                    )
                )
                .Aggregations(a => a
                    .Add("uniqueKeyCount", a => a
                        .Terms(t => t
                            .Field("dataQuality.uniqueKey")
                            .MinDocCount(2)
                            .Size(65536)
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();

            var duplicates = searchResponse
                .Aggregations
                    .GetStringTerms("uniqueKeyCount")
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
                        .Indices(index)
                        .Query(q => q
                            .Bool(b => b
                                .Filter(f => f.Term(t => t
                                    .Field("dataQuality.uniqueKey")
                                    .Value(duplicate.UniqueKey)))
                            )
                        )
                        .Sort(sort => sort.Field("dataProviderId".ToField()))
                        .Size(10000)
                        .Source(
                            (
                                Includes: new[] {
                                    "dataProviderId",
                                    "occurrence.occurrenceId",
                                    "location.locality",
                                    "event.startDate",
                                    "event.endDate",
                                    "taxon.id",
                                    "taxon.scientificName"
                                }, 
                                Excludes: Array.Empty<string>()).ToProjection()
                            )
                        .TrackTotalHits(new TrackHits(false))
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
                        UniqueKey = duplicate.UniqueKey.Value?.ToString()
                    };

                    report.Records.Add(record);
                    rowCount += docCount;
                }
            }

            return report;
        }

        /// <inheritdoc />
        public async Task<HealthStatus> GetHealthStatusAsync(HealthStatus waitForStatus, int waitForSeconds)
        {
            try
            {
                var response = await Client.Cluster
                        .HealthAsync(new[] { Indices.Index(PublicIndexName), Indices.Index(ProtectedIndexName) }, chr => chr
                            .Level(Level.Indices)
                            .Timeout(TimeSpan.FromSeconds(waitForSeconds))
                            .WaitForStatus(waitForStatus)
                        );

                return response.IsValidResponse ? response.Status : HealthStatus.Red;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get ElasticSearch health");
                return HealthStatus.Red;
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
        public async Task<long> GetMatchCountAsync(SearchFilterBase filter, bool skipAuthorizationFilters = false)
        {
           var indexNames = GetCurrentIndex(filter, skipAuthorizationFilters);
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter, skipAuthorizationFilters);

            var countResponse = await Client.CountAsync<dynamic>(indexNames, s => s
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery.ToArray())
                        .Filter(query?.ToArray())
                    )
                )
            );
            countResponse.ThrowIfInvalid();

            return countResponse.Count;
        }

        /// <inheritdoc />
        public async Task<GeoGridMetricResult> GetMetricGridAggregationAsync(
            SearchFilter filter,
            int gridCellSizeInMeters,
            MetricCoordinateSys metricCoordinateSys,
            bool skipAuthorizationFilters = false,
            int? maxBuckets = null,
            IReadOnlyDictionary<string, FieldValue> afterKey = null,
            TimeSpan? timeout = null)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter, skipAuthorizationFilters: skipAuthorizationFilters);

            // Max buckets can't exceed MaxNrElasticSearchAggregationBuckets
            if (maxBuckets.HasValue && maxBuckets.Value > MaxNrElasticSearchAggregationBuckets)
            {
                maxBuckets = MaxNrElasticSearchAggregationBuckets;
            }

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery.ToArray())
                        .Filter(query.ToArray())
                    )
                )
                .Aggregations(a => a
                    .Add("gridCells", a => a
                        .Composite(c => c
                            .Size(maxBuckets ?? MaxNrElasticSearchAggregationBuckets + 1)
                            .After(a => afterKey?.ToDictionary(ak => ak.Key.ToField(), ak => ak.Value).ToFluentDictionary())
                            .Sources(
                                [
                                    CreateCompositeTermsAggregationSource(
                                        ("metric_x", $"(Math.floor(doc['location.{(metricCoordinateSys.Equals(MetricCoordinateSys.ETRS89) || metricCoordinateSys.Equals(MetricCoordinateSys.ETRS89_LAEA_Europe) ? "etrs89X" : "sweref99TmX")}'].value / {gridCellSizeInMeters}) * {gridCellSizeInMeters}).intValue()", SortOrder.Asc, false, true)
                                    ),
                                    CreateCompositeTermsAggregationSource(
                                        ("metric_y", $"(Math.floor(doc['location.{(metricCoordinateSys.Equals(MetricCoordinateSys.ETRS89) || metricCoordinateSys.Equals(MetricCoordinateSys.ETRS89_LAEA_Europe) ? "etrs89Y" : "sweref99TmY")}'].value / {gridCellSizeInMeters}) * {gridCellSizeInMeters}).intValue()", SortOrder.Asc, false, true)
                                    )
                                ]
                            )
                        ).Aggregations(a => a
                            .Add("taxa_count", a => a
                                .Cardinality(c => c
                                    .Field("taxon.id")
                                )
                            )
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
                .RequestConfiguration(r => r
                    .RequestTimeout(timeout.GetValueOrDefault(TimeSpan.FromSeconds(_elasticConfiguration.RequestTimeout.GetValueOrDefault(60))))
                )
            );

            if (!searchResponse.IsValidResponse)
            {
                if (searchResponse.ElasticsearchServerError?.Error?.CausedBy?.Type == "too_many_buckets_exception")
                {
                    throw new ArgumentOutOfRangeException($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
                }

                searchResponse.ThrowIfInvalid();
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations?.GetComposite("gridCells")?.Buckets?.Count ?? 0;
            if (nrOfGridCells > MaxNrElasticSearchAggregationBuckets)
            {
                throw new ArgumentOutOfRangeException($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
            }

            var gridResult = new GeoGridMetricResult()
            {
                BoundingBox = filter.Location?.Geometries?.BoundingBox,
                GridCellSizeInMeters = gridCellSizeInMeters,
                GridCellCount = nrOfGridCells,
                GridCells = searchResponse.Aggregations.GetComposite("gridCells").Buckets.Select(b =>
                    new GridCell
                    {
                        MetricBoundingBox = new XYBoundingBox
                        {
                            BottomRight = new XYCoordinate(double.Parse(b.Key["metric_x"].ToString()) + gridCellSizeInMeters, double.Parse(b.Key["metric_y"].ToString())),
                            TopLeft = new XYCoordinate(double.Parse(b.Key["metric_x"].ToString()), double.Parse(b.Key["metric_y"].ToString()) + gridCellSizeInMeters)
                        },
                        ObservationsCount = b.DocCount,
                        TaxaCount = (long?)b.Aggregations?.GetCardinality("taxa_count")?.Value
                    }
                ),
                AfterKey = searchResponse.Aggregations.GetComposite("gridCells").AfterKey
            };

            // When operation is disposed, telemetry item is sent.
            return gridResult;
        }

        /// <inheritdoc />
        public async Task<SearchAfterResult<ExtendedMeasurementOrFactRow, IReadOnlyCollection<FieldValue>>> GetMeasurementOrFactsBySearchAfterAsync(
           SearchFilterBase filter,
           string pointInTimeId = null,
           IReadOnlyCollection<FieldValue> afterKey = null)
        {
            var searchIndex = GetCurrentIndex(filter);
            var queries = filter.ToMeasurementOrFactsQuery<dynamic>();
            var searchResponse = await SearchAfterAsync<dynamic>(searchIndex, new SearchRequestDescriptor<dynamic>()
                .Indices(searchIndex)
                .Query(query => query
                    .Bool(b => b
                        .Filter(queries.ToArray())
                    )
                )
                .Source(
                    (Includes: new[] { "occurrence.occurrenceId", "event.eventId", "measurementOrFacts" }, Excludes: Array.Empty<string>()).ToProjection()
                ),
                pointInTimeId,
                afterKey?.ToArray()
            );

            return new SearchAfterResult<ExtendedMeasurementOrFactRow, IReadOnlyCollection<FieldValue>>
            {
                Records = searchResponse.Documents.ToObservations()?.ToExtendedMeasurementOrFactRows(),
                PointInTimeId = searchResponse.PitId,
                SearchAfter = searchResponse.Hits?.LastOrDefault()?.Sort
            };
        }

        /// <inheritdoc />
        public async Task<SearchAfterResult<SimpleMultimediaRow, IReadOnlyCollection<FieldValue>>> GetMultimediaBySearchAfterAsync(
            SearchFilterBase filter,
            string pointInTimeId = null,
            IReadOnlyCollection<FieldValue> afterKey = null)
        {
            var searchIndex = GetCurrentIndex(filter);
            var queries = filter.ToMultimediaQuery<dynamic>();
            var searchResponse = await SearchAfterAsync<dynamic>(searchIndex, new SearchRequestDescriptor<dynamic>()
                .Indices(searchIndex)
                .Query(query => query
                    .Bool(boolQueryDescriptor => boolQueryDescriptor
                        .Filter(queries.ToArray())
                )
            )
                .Source(
                    (Includes: new[] { "occurrence.occurrenceId", "media" }, Excludes: Array.Empty<string>()).ToProjection()
                ),
                pointInTimeId,
                afterKey?.ToArray());

            return new SearchAfterResult<SimpleMultimediaRow, IReadOnlyCollection<FieldValue>>
            {
                Records = searchResponse.Documents?.ToObservations().ToSimpleMultimediaRows(),
                PointInTimeId = searchResponse.PitId,
                SearchAfter = searchResponse.Hits?.LastOrDefault()?.Sort
            };
        }

        /// <inheritdoc/>
        public async Task<T> GetObservationAsync<T>(string occurrenceId, SearchFilter filter, bool getAllFields = false) where T : class
        {
            var indexNames = GetCurrentIndex(filter);
            var queries = filter.ToQuery<T>(skipSightingTypeFilters: true);
            queries.TryAddTermCriteria("occurrence.occurrenceId", occurrenceId);

            var searchResponse = await Client.SearchAsync<T>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .Filter(queries.ToArray())
                    )
                )
                .Size(1)
                .Source(getAllFields ? new SourceConfig(true) : filter.Output?.Fields.ToProjection(filter is SearchFilterInternal))
                .TrackTotalHits(new TrackHits(false))
            );
            searchResponse.ThrowIfInvalid();

            return searchResponse.Documents?.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetObservationsAsync(IEnumerable<string> occurrenceIds, bool protectedIndex)
        {
            return await GetObservationsAsync(occurrenceIds, ["occurrence", "location"], protectedIndex);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetObservationsAsync(
            IEnumerable<string> occurrenceIds,
            IEnumerable<string> outputFields,
            bool protectedIndex)
        {
            try
            {
                var searchResponse = await Client.SearchAsync<Observation>(s => s
                    .Indices(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .Terms(t => t
                            .Field(f => f.Occurrence.OccurrenceId)
                            .Terms(occurrenceIds?.ToTermsQueryField())
                        )
                    )
                    .Size(occurrenceIds?.Count() ?? 0)
                    .Source((Includes: outputFields?.ToArray(), Excludes: Array.Empty<string>()).ToProjection())
                    .TrackTotalHits(new TrackHits(false))
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

        /// <inheritdoc/>
        public async Task<ScrollResult<T>> GetObservationsByScrollAsync<T>(
            SearchFilter filter,
            int take,
            string scrollId) where T : class
        {
            var indexNames = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries<T>(filter);

            var sortDescriptor = await Client.GetSortDescriptorAsync<T, Observation>(indexNames, filter?.Output?.SortOrders);

            // Retry policy by Polly
            var response = await PollyHelper.GetRetryPolicy(3, 100).ExecuteAsync(async () =>
            {
                if (string.IsNullOrEmpty(scrollId))
                {
                    var searchResponse = await Client.SearchAsync<T>(s => s
                        .Indices(indexNames)
                        .Query(q => q
                            .Bool(b => b
                                .MustNot(excludeQueries.ToArray())
                                .Filter(queries.ToArray())
                            )
                        )
                        .Sort(sortDescriptor?.ToArray())
                        .Size(take)
                        .Source(filter.Output?.Fields.ToProjection(filter is SearchFilterInternal))
                        .Scroll(ScrollTimeout)
                    );

                    searchResponse.ThrowIfInvalid();
                    return (searchResponse.Documents, TotalCount: searchResponse.Total, searchResponse.ScrollId);
                }
                else
                {
                    var scrollResult = await Client.ScrollAsync<T>(new ScrollRequest()
                    {
                        ScrollId = scrollId
                    }
                    );
                    return (scrollResult.Documents, TotalCount: scrollResult.Total, scrollResult.ScrollId);
                }
            });

            return new ScrollResult<T>
            {
                Records = response.Documents,
                ScrollId = response.Documents.Count < take ? null : response.ScrollId?.Id,
                Take = take,
                TotalCount = response.TotalCount
            };
        }

        /// <inheritdoc />
        public async Task<SearchAfterResult<T, IReadOnlyCollection<FieldValue>>> GetObservationsBySearchAfterAsync<T>(
            SearchFilter filter,
            string pointInTimeId = null,
            ICollection<FieldValue> afterKey = null)
        {
            var searchIndex = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries<dynamic>(filter);

            // Retry policy by Polly
            var searchResponse = await PollyHelper.GetRetryPolicy(3, 100).ExecuteAsync(async () =>
            {
                var queryResponse = await SearchAfterAsync<dynamic>(searchIndex, new SearchRequestDescriptor<dynamic>()
                .Indices(searchIndex)
                .Source(filter.Output?.Fields.ToProjection(filter is SearchFilterInternal))
                    .Query(q => q
                        .Bool(b => b
                            .Filter(query.ToArray())
                            .MustNot(excludeQuery.ToArray())
                        )
                    ),
                pointInTimeId,
                afterKey);
                queryResponse.ThrowIfInvalid();

                return queryResponse;
            });

            return new SearchAfterResult<T, IReadOnlyCollection<FieldValue>>
            {
                Records = (IEnumerable<T>)(typeof(T).Equals(typeof(Observation)) ? searchResponse.Documents?.ToObservationsArray() : searchResponse.Documents),
                PointInTimeId = searchResponse.PitId,
                SearchAfter = searchResponse.Hits?.LastOrDefault()?.Sort
            };
        }

        /// <inheritdoc /> 
        public async Task<(DateTime? firstSpotted, DateTime? lastSpotted, GeoBounds geographicCoverage)> GetProviderMetaDataAsync(int providerId, bool protectedIndex)
        {
            var res = await Client.SearchAsync<Observation>(s => s
                .Indices(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.DataProviderId)
                        .Value(providerId)))
                .Aggregations(a => a
                    .Add("firstSpotted", a => a
                        .Min(m => m.Field("firstSpotted"))
                    )
                    .Add("lastSpotted", a => a
                        .Max(m => m.Field("lastSpotted"))
                    )
                    .Add("geographicCoverage", a => a
                        .GeoBounds(gb => gb
                            .Field(f => f.Location.PointLocation)
                            .WrapLongitude()
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            var defaultGeoBounds = GeoBounds.TopLeftBottomRight(new TopLeftBottomRightGeoBounds
            {
                BottomRight = new LatLonGeoLocation { Lat = 0.0, Lon = 0.0 },
                TopLeft = new LatLonGeoLocation { Lat = 0.0, Lon = 0.0 }
            });
            if (!res.IsValidResponse)
            {
                return (null, null, defaultGeoBounds);
            }

            var firstSpotted = res.Aggregations?.GetMin("firstSpotted")?.Value;
            var lastSpotted = res.Aggregations?.GetMax("lastSpotted")?.Value;
            var geographicCoverage = res.Aggregations?.GetGeoBounds("geographicCoverage")?.Bounds;

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            geographicCoverage.TryGetTopLeftBottomRight(out var topLeftBottomRight);

            return (epoch.AddMilliseconds(firstSpotted ?? 0).ToUniversalTime(), epoch.AddMilliseconds(lastSpotted ?? 0).ToUniversalTime(), topLeftBottomRight ?? defaultGeoBounds);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetRandomObservationsAsync(int take, bool protectedIndex)
        {
            try
            {
                var searchResponse = await Client.SearchAsync<Observation>(s => s
                    .Indices(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q
                        .FunctionScore(fs => fs
                            .Functions(f => f
                                .RandomScore(rs => rs
                                    .Seed(DateTime.Now.ToBinary())
                                    .Field(p => p.Occurrence.OccurrenceId)))))
                    .Size(take)
                    .Source((Includes: new[] { "occurrence", "location" }, Excludes: Array.Empty<string>()).ToProjection())
                    .TrackTotalHits(new TrackHits(false))
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
        public async Task<HashSet<string>> GetSortableFieldsAsync()
        {
            var sortableFields = new HashSet<string>();
            var mappings = await Client.Indices.GetMappingAsync<Observation>(o => o
                .Indices(PublicIndexName)
            );
            if (mappings.IsValidResponse)
            {
                foreach (var value in mappings.Indices.Values)
                {
                    PopulateSortableFields(value.Mappings.Properties, ref sortableFields, "");
                }
            }

            return sortableFields;
        }

        /// <inheritdoc />
        public Uri HostUrl => Client.ElasticsearchClientSettings.NodePool.Nodes.FirstOrDefault().Uri;

        //Client.Nodes.Info(i => i.Human(true)).Nodes.First().Value.Http.PublishAddress
        //  Client.Nodes.Info(i => i.Human(true)).ConnectionSettings.ConnectionPool.Nodes.FirstOrDefault().Uri;

        /// <inheritdoc />
        public async Task<long> IndexCountAsync(bool protectedIndex)
        {
            try
            {
                var countResponse = await Client.CountAsync<dynamic>(c => c.Indices(protectedIndex ? ProtectedIndexName : PublicIndexName));
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
        public async Task<IEnumerable<string>> TryToGetOccurenceIdDuplicatesAsync(bool protectedIndex, int maxReturnedItems)
        {
            var searchResponse = await Client.SearchAsync<dynamic>(protectedIndex ? ProtectedIndexName : PublicIndexName, s => s
                .Aggregations(a => a
                    .Add("OccurrenceIdDuplicatesExists", a => a
                        .Terms(t => t
                            .Field("occurrence.occurrenceId")
                            .MinDocCount(2)
                            .Size(maxReturnedItems)
                        )
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse.Aggregations.GetStringTerms("OccurrenceIdDuplicatesExists").Buckets.Select(b => b.Key.Value.ToString());
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
                    await Client.CountAsync<Observation>(ProtectedIndexName, s => s
                        .Query(q => q
                            .Bool(b => b
                                .MustNot(mn => mn.Term(t => t
                                    .Field(f => f.DiffusionStatus).Value(FieldValue.Long((long)DiffusionStatus.NotDiffused)))
                                )
                            )
                        )
                    )
                    :
                    await Client.CountAsync<Observation>(PublicIndexName, s => s
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Term(t => t
                                    .Field(f => f.AccessRights.Id).Value((int)AccessRightsId.NotForPublicUsage)
                                ), f => f
                                .Term(t => t
                                    .Field(f => f.DiffusionStatus).Value(FieldValue.Long((long)DiffusionStatus.NotDiffused)))
                                )
                            )
                        )
                    );

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

        /// <inheritdoc />
        public async Task WaitForPublicIndexCreationAsync(long expectedRecordsCount, TimeSpan? timeout = null, bool protectedIndex = false)
        {
            Logger.LogInformation($"Begin waiting for index creation. Index={IndexName}, ExpectedRecordsCount={expectedRecordsCount}, Timeout={timeout}");
            if (timeout == null) timeout = TimeSpan.FromMinutes(10);
            var sleepTime = TimeSpan.FromSeconds(5);
            int nrIterations = (int)(Math.Ceiling(timeout.Value.TotalSeconds / sleepTime.TotalSeconds));
            long docCount = await IndexCountAsync(false);
            var iterations = 0;

            // Compare number of documents processed with actually db count
            // If docCount is less than process count, indexing is not ready yet
            while (docCount < expectedRecordsCount && iterations < nrIterations)
            {
                iterations++; // Safety to prevent infinite loop.                                
                await Task.Delay(sleepTime);
                docCount = await IndexCountAsync(protectedIndex);
            }

            if (iterations == nrIterations)
            {
                Logger.LogError($"Failed waiting for index creation due to timeout. Index={PublicIndexName}. ExpectedRecordsCount={expectedRecordsCount}, DocCount={docCount}");
            }
            else
            {
                Logger.LogInformation($"Finish waiting for index creation. Index={PublicIndexName}.");
            }
        }

        public async Task<SearchAfterResult<dynamic, IReadOnlyDictionary<string, FieldValue>>> AggregateByUserFieldAsync(SearchFilter filter,
            string aggregationField,
            bool aggregateOrganismQuantity,
            int? precisionThreshold,
            IReadOnlyDictionary<string, FieldValue>? afterKey = null,
            int? take = 10,
            bool? useScript = true)
        {
            var indexNames = GetCurrentIndex(filter);
            var (queries, excludeQueries) = GetCoreQueries<dynamic>(filter);

            FluentDescriptorDictionary<Field, RuntimeFieldDescriptor<dynamic>> runtimeMapping = null;
            if (useScript ?? true)
            {
                var scriptFieldName = "scriptField";
                runtimeMapping = GetRuntimeMappingScriptField(aggregationField, scriptFieldName);
                aggregationField = scriptFieldName;
            }

            var searchResponse =
                await Client.SearchAsync<dynamic>(s => s
                .Indices(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQueries.ToArray())
                        .Filter(queries.ToArray())
                    )
                )
                .RuntimeMappings(g => runtimeMapping)
                .Aggregations(a => a
                    .Add("aggregation", a => a
                        .Composite(c => c
                            .After(ak => afterKey?.ToFluentFieldDictionary())
                            .Size(take)
                            .Sources(
                                [
                                    CreateCompositeTermsAggregationSource(
                                        ("termAggregation", aggregationField, SortOrder.Asc, true)
                                    )
                                ]
                            )
                        )
                        .Aggregations(a => 
                        {
                            var aggregations = new FluentDescriptorDictionary<string, AggregationDescriptor<dynamic>> {
                                {
                                    "unique_taxonids", a => a
                                        .Cardinality(c => c
                                            .Field("taxon.id")
                                            .PrecisionThreshold(precisionThreshold ?? 3000)
                                        )
                                }
                            };
                            if (aggregateOrganismQuantity)
                            {
                                aggregations.Add("totalOrganismQuantity", a => a
                                        .Sum(s => s
                                            .Field("occurrence.organismQuantityAggregation")
                                        )
                                    );
                            }
                            return aggregations;
                        })
                    )
                )
                .AddDefaultAggrigationSettings()
            );

            searchResponse.ThrowIfInvalid();
            afterKey = searchResponse
               .Aggregations
                .GetComposite("aggregation")
                    .AfterKey;

            return new SearchAfterResult<dynamic, IReadOnlyDictionary<string, FieldValue>>
            {
                SearchAfter = afterKey,
                Records = searchResponse
                    .Aggregations
                    .GetComposite("aggregation")
                    .Buckets?
                        .Select(b =>
                            new
                            {
                                AggregationField = b.Key.Values.First().Value,
                                b.DocCount,
                                UniqueTaxon = b.Aggregations.GetCardinality("unique_taxonids").Value,
                                OrganismQuantity = aggregateOrganismQuantity ? b.Aggregations.GetSum("totalOrganismQuantity")?.Value : 0
                            }
                        )?.ToArray()
            };
        }
    }
}
