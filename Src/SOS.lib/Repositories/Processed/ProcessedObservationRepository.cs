using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Elasticsearch.Net;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataQuality;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.AggregatedResult;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Processed.Interfaces;
using DateTime = System.DateTime;
using Result = CSharpFunctionalExtensions.Result;
using Area = SOS.Lib.Models.Processed.Observation.Area;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedObservationRepository : ProcessRepositoryBase<Observation, string>,
        IProcessedObservationRepository
    {
        private const int ElasticSearchMaxRecords = 10000;
        private readonly IElasticClientManager _elasticClientManager;
        private readonly ElasticSearchConfiguration _elasticConfiguration;
        private readonly TelemetryClient _telemetry;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly ITaxonManager _taxonManager;

        /// <summary>
        /// Http context accessor.
        /// </summary>
        public IHttpContextAccessor HttpContextAccessor
        {
            get => _httpContextAccessor;
            set => _httpContextAccessor = value;
        }

        private IElasticClient Client => _elasticClientManager.Clients.Length == 1 ? _elasticClientManager.Clients.FirstOrDefault() : _elasticClientManager.Clients[CurrentInstance];

        private IElasticClient InActiveClient => _elasticClientManager.Clients.Length == 1 ? _elasticClientManager.Clients.FirstOrDefault() : _elasticClientManager.Clients[InActiveInstance];

        /// <summary>
        /// Add the collection
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private async Task<bool> AddCollectionAsync(bool protectedIndex)
        {
            var createIndexResponse = await Client.Indices.CreateAsync(protectedIndex ? ProtectedIndexName : PublicIndexName, s => s
                .Settings(s => s
                    .NumberOfShards(_elasticConfiguration.NumberOfShards)
                    .NumberOfReplicas(_elasticConfiguration.NumberOfReplicas)
                    .Setting("max_terms_count", 110000)
                    .Setting(UpdatableIndexSettings.MaxResultWindow, 100000)
                )
                
                .Map<Observation>(m => m
                    .AutoMap<Observation>()
                    .Properties(ps => ps
                        .Keyword(kw => kw
                            .Name(nm => nm.Id)
                            .Index(true) // todo - index=false?
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.DynamicProperties)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.InformationWithheld)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.BibliographicCitation)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.CollectionId)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.CollectionCode)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.DataGeneralizations)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.DatasetId)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.DatasetName)
                            .Index(true) // WFS
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.InstitutionId)
                            .Index(true)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.Language)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.License)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.OwnerInstitutionCode)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.PrivateCollection)
                            .Index(true)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.PublicCollection)
                            .Index(true)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.References)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.RightsHolder)
                            .Index(false)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.SpeciesCollectionLabel)
                            .Index(true)
                        )
                        .Nested<ExtendedMeasurementOrFact>(n => n
                            .Name(nm => nm.MeasurementOrFacts)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementAccuracy)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementDeterminedBy)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementDeterminedDate)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementID)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementMethod)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementRemarks)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementType)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementTypeID)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementUnit)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementUnitID)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementValue)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.MeasurementValueID)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OccurrenceID)
                                    .Index(false)
                                )
                            )
                        )
                        .Object<ProjectsSummary>(t => t
                            .AutoMap()
                            .Name(nm => nm.ProjectsSummary)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project1Name)
                                    .Index(true) // WFS
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project1Category)
                                    .Index(true) // WFS
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project1Url)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project1Values)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project2Name)
                                    .Index(true) // WFS
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project2Category)
                                    .Index(true) // WFS
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project2Url)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project2Values)
                                    .Index(false)
                                )
                            )
                        )
                        .Nested<Project>(n => n
                            .AutoMap()
                            .Name(nm => nm.Projects)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.Category)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.CategorySwedish)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Name)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Owner)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ProjectURL)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Description)
                                    .Index(false)
                                )                                
                                .Keyword(kw => kw
                                    .Name(nm => nm.SurveyMethod)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.SurveyMethodUrl)
                                    .Index(false)
                                )
                                .Nested<ProjectParameter>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.ProjectParameters)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.DataType)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Name)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Unit)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Description)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                            .Index(false)
                                        )
                                    )
                                )
                            )
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.AccessRights)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.Value)
                                )
                                .Number(nr => nr
                                    .Name(nm => nm.Id)
                                    .Type(NumberType.Integer)
                                )
                            )
                        )
                        .Object<ArtportalenInternal>(t => t
                            .AutoMap()
                            .Name(nm => nm.ArtportalenInternal)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.LocationExternalId)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LocationPresentationNameParishRegion)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ParentLocality)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ReportedByUserAlias)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.SightingBarcodeURL)
                                )
                                .Nested<UserInternal>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.OccurrenceRecordedByInternal)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.UserAlias)
                                        )
                                    )
                                )
                                .Nested<UserInternal>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.OccurrenceVerifiedByInternal)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.UserAlias)
                                        )
                                    )
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.BirdValidationAreaIds)
                                )                                
                            )
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.BasisOfRecord)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.Value)
                                )
                                .Number(nr => nr
                                    .Name(nm => nm.Id)
                                    .Type(NumberType.Integer)
                                )
                            )
                        )
                        .Object<DataQuality>(t => t
                            .AutoMap()
                            .Name(nm => nm.DataQuality)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.UniqueKey)
                                )
                            )
                        )
                        .Object<IDictionary<string, string>>(c => c
                            .AutoMap()
                            .Name(nm => nm.Defects)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.Keys)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Values)
                                    .Index(false)
                                )
                            )
                        )
                        .Object<Event>(t => t
                            .AutoMap()
                            .Name(nm => nm.Event)
                            .Properties(ps => ps
                                .Date(d => d
                                    .Name(nm => nm.EndDate)
                                )
                                .Date(d => d
                                    .Name(nm => nm.StartDate)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.EventId)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.EventRemarks)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.FieldNumber)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.FieldNotes)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Habitat)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ParentEventId)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.PlainEndDate)
                                    .Index(true) // WFS
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.PlainEndTime)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.PlainStartDate)
                                    .Index(true) // WFS
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.PlainStartTime)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.SampleSizeUnit)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.SampleSizeValue)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.SamplingEffort)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.SamplingProtocol)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimEventDate)
                                )
                                .Nested<Multimedia>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.Media)                                    
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Description)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Audience)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Contributor)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Created)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Creator)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.DatasetID)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Format)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Identifier)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.License)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Publisher)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.References)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.RightsHolder)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Source)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Title)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Type)
                                            .Index(false)
                                        )
                                    )
                                )
                                .Nested<ExtendedMeasurementOrFact>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.MeasurementOrFacts)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.OccurrenceID)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementRemarks)
                                            .Index(false)
                                        )                                        
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementAccuracy)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementDeterminedBy)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementDeterminedDate)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementID)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementMethod)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementType)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementTypeID)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementUnit)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementUnitID)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementValue)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.MeasurementValueID)
                                            .Index(false)
                                        )
                                    )
                                )
                                .Number(x => x
                                    .Name(nm => nm.EndDay)
                                    .Type(NumberType.Integer)
                                )
                                .Number(x => x
                                    .Name(nm => nm.EndMonth)
                                    .Type(NumberType.Integer)
                                )
                                .Number(x => x
                                    .Name(nm => nm.EndYear)
                                    .Type(NumberType.Integer)
                                )
                                .Number(x => x
                                    .Name(nm => nm.StartDay)
                                    .Type(NumberType.Integer)
                                )
                                .Number(x => x
                                    .Name(nm => nm.StartMonth)
                                    .Type(NumberType.Integer)
                                )
                                .Number(x => x
                                    .Name(nm => nm.StartYear)
                                    .Type(NumberType.Integer)
                                )
                                .Object<VocabularyValue>(t => t
                                    .Name(nm => nm.DiscoveryMethod)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                            )
                        )
                        .Object<GeologicalContext>(c => c
                            .Name(nm => nm.GeologicalContext)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.Bed)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.EarliestAgeOrLowestStage)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.EarliestEonOrLowestEonothem)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.EarliestEpochOrLowestSeries)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.EarliestEraOrLowestErathem)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.EarliestGeochronologicalEra)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.EarliestPeriodOrLowestSystem)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Formation)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.GeologicalContextId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Group)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.HighestBiostratigraphicZone)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LatestAgeOrHighestStage)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LatestEonOrHighestEonothem)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LatestEpochOrHighestSeries)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LatestEraOrHighestErathem)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LatestGeochronologicalEra)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LatestPeriodOrHighestSystem)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LithostratigraphicTerms)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LowestBiostratigraphicZone)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Member)
                                    .Index(false)
                                )
                            )
                        )
                        .Object<Identification>(c => c
                            .AutoMap()
                            .Name(nm => nm.Identification)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.IdentificationRemarks)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ConfirmedBy)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ConfirmedDate)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.DateIdentified)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.IdentificationId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.IdentificationQualifier)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.IdentificationReferences)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.IdentifiedBy)
                                    .Index(false)
                                )
                                 .Keyword(kw => kw
                                    .Name(nm => nm.TypeStatus)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerifiedBy)
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.DeterminationMethod)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.ValidationStatus)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.VerificationStatus)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                            )
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.InstitutionCode)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.Value)
                                )
                                .Number(nr => nr
                                    .Name(nm => nm.Id)
                                    .Type(NumberType.Integer)
                                )
                            )
                        )
                        .Object<Location>(l => l
                            .AutoMap()
                            .Name(nm => nm.Location)
                            .Properties(ps => ps
                                .GeoShape(gs => gs
                                    .Name(nn => nn.Point)
                                )
                                .GeoPoint(gp => gp
                                    .Name(nn => nn.PointLocation)
                                )
                                .GeoShape(gs => gs
                                    .Name(nn => nn.PointWithBuffer)
                                )
                                .GeoShape(gs => gs
                                    .Name(nn => nn.PointWithDisturbanceBuffer)
                                )                                
                                .Keyword(kw => kw
                                    .Name(nm => nm.CountryCode)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.FootprintSRS)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.GeodeticDatum)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.GeoreferencedBy)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.GeoreferencedDate)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.GeoreferenceProtocol)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.GeoreferenceSources)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.GeoreferenceVerificationStatus)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.HigherGeography)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.HigherGeographyId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Island)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.IslandGroup)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LocationRemarks)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LocationAccordingTo)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LocationId)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.FootprintSpatialFit)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.FootprintWKT)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.GeoreferenceRemarks)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.PointRadiusSpatialFit)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimCoordinates)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimCoordinateSystem)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimDepth)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimElevation)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimLatitude)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimLocality)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimLongitude)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimSRS)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.WaterBody)
                                    .Index(false)
                                )
                                .Object<LocationAttributes>(c => c
                                    .Name(nm => nm.Attributes)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.ExternalId)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.CountyPartIdByCoordinate)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.ProvincePartIdByCoordinate)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.VerbatimMunicipality)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.VerbatimProvince)
                                            .Index(false)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Continent)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                            .Index(false)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Country)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                            .Index(false)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<Area>(c => c
                                    .AutoMap()
                                    .Name(nm => nm.County)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.FeatureId)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Name)
                                        )
                                    )
                                )
                                .Object<Area>(c => c
                                    .AutoMap()
                                    .Name(nm => nm.Municipality)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.FeatureId)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Name)
                                        )
                                    )
                                )
                                .Object<Area>(c => c
                                    .AutoMap()
                                    .Name(nm => nm.Parish)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.FeatureId)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Name)
                                        )
                                    )
                                )
                                .Object<Area>(c => c
                                    .AutoMap()
                                    .Name(nm => nm.Province)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.FeatureId)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Name)
                                        )
                                    )
                                )
                                .Wildcard(wc => wc
                                    .Name(nm => nm.Locality)
                                )
                            )
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
                                .Keyword(kw => kw
                                    .Name(nm => nm.AssociatedMedia)
                                    .Index(true)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OccurrenceRemarks)
                                    .Index(true) // Because there is a OnlyWithNote search parameter. Todo - Add bool property, HasOccurrenceRemarks and set index to false?
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.AssociatedOccurrences)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.AssociatedReferences)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.AssociatedSequences)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.AssociatedTaxa)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.BiotopeDescription)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.IndividualId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.RecordedBy)
                                )                                
                                .Keyword(kw => kw
                                    .Name(nm => nm.CatalogNumber)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Disposition)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.IndividualCount)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OccurrenceId)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OccurrenceStatus)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OrganismQuantity)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OtherCatalogNumbers)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Preparations)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.RecordNumber)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ReportedBy)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Url)
                                    .Index(false)
                                )                                
                                .Nested<Multimedia>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.Media)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Description)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Audience)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Contributor)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Created)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Creator)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.DatasetID)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Format)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Identifier)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.License)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Publisher)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.References)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.RightsHolder)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Source)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Title)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Type)
                                            .Index(false)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Activity)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Behavior)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Biotope)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.EstablishmentMeans)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.LifeStage)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.OccurrenceStatus)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.OrganismQuantityUnit)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.ReproductiveCondition)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Sex)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Value)
                                        )
                                        .Number(nr => nr
                                            .Name(nm => nm.Id)
                                            .Type(NumberType.Integer)
                                        )
                                    )
                                )
                                .Object<Substrate>(c => c
                                    .AutoMap()
                                    .Name(nm => nm.Substrate)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.SpeciesScientificName)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.Description)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.SpeciesDescription)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.SubstrateDescription)
                                            .Index(false)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.SpeciesVernacularName)
                                            .Index(false)
                                        )
                                        .Object<VocabularyValue>(c => c
                                            .Name(nm => nm.Name)
                                            .Properties(ps => ps
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.Value)
                                                )
                                                .Number(nr => nr
                                                    .Name(nm => nm.Id)
                                                    .Type(NumberType.Integer)
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                        .Object<Organism>(c => c
                            .AutoMap()
                            .Name(nm => nm.Organism)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.AssociatedOrganisms)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OrganismId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OrganismName)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OrganismRemarks)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OrganismScope)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.PreviousIdentifications)
                                    .Index(false)
                                )
                            )
                        )
                        .Object<Taxon>(t => t
                            .AutoMap()
                            .Name(nm => nm.Taxon)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.AcceptedNameUsage)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.AcceptedNameUsageId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.NomenclaturalCode)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.NomenclaturalStatus)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Class)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Order)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.TaxonId)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.TaxonRemarks)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimTaxonRank)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Family)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Genus)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.HigherClassification)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.InfraspecificEpithet)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Kingdom)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.NameAccordingTo)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.NameAccordingToId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.NamePublishedIn)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.NamePublishedInId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.NamePublishedInYear)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OriginalNameUsage)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OriginalNameUsageId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ParentNameUsage)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ParentNameUsageId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Phylum)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ScientificName)                                    
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ScientificNameAuthorship)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ScientificNameId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.SpecificEpithet)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Subgenus)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.TaxonConceptId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.TaxonomicStatus)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.TaxonRank)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VerbatimId)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.VernacularName)
                                )
                                .Object<TaxonAttributes>(c => c
                                    .AutoMap()
                                    .Name(nm => nm.Attributes)
                                    .Properties(ps => ps
                                        .Keyword(kw => kw
                                            .Name(nm => nm.ActionPlan)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.OrganismGroup)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.InvasiveRiskAssessmentCategory)
                                        )                                                            
                                        .Keyword(kw => kw
                                            .Name(nm => nm.RedlistCategory)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.SwedishOccurrence)
                                        )
                                        .Keyword(kw => kw
                                            .Name(nm => nm.SwedishHistory)
                                        )
                                        .Nested<TaxonSynonymName>(n => n
                                            .Name(nm => nm.Synonyms)
                                            .Properties(ps => ps
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.Author)
                                                    .Index(false)
                                                )
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.Name)
                                                    .Index(false)
                                                )
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.NomenclaturalStatus)
                                                    .Index(false)
                                                )
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.TaxonomicStatus)
                                                    .Index(false)
                                                )
                                            )
                                        )
                                        .Nested<TaxonVernacularName>(n => n
                                            .Name(nm => nm.VernacularNames)
                                            .Properties(ps => ps
                                                .Boolean(b => b
                                                    .Name(nm => nm.IsPreferredName)
                                                    .Index(false)
                                                )
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.CountryCode)
                                                    .Index(false)
                                                )
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.Name)
                                                    .Index(false)
                                                )
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.Language)
                                                    .Index(false)
                                                )
                                            )
                                        )
                                        .Object<VocabularyValue>(c => c
                                            .Name(nm => nm.ProtectionLevel)
                                            .Properties(ps => ps
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.Value)
                                                )
                                                .Number(nr => nr
                                                    .Name(nm => nm.Id)
                                                    .Type(NumberType.Integer)
                                                )
                                            )
                                        )
                                        .Object<VocabularyValue>(c => c
                                            .Name(nm => nm.SensitivityCategory)
                                            .Properties(ps => ps
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.Value)
                                                )
                                                .Number(nr => nr
                                                    .Name(nm => nm.Id)
                                                    .Type(NumberType.Integer)
                                                )
                                            )
                                        )
                                        .Object<VocabularyValue>(c => c
                                            .Name(nm => nm.TaxonCategory)
                                            .Properties(ps => ps
                                                .Keyword(kw => kw
                                                    .Name(nm => nm.Value)
                                                )
                                                .Number(nr => nr
                                                    .Name(nm => nm.Id)
                                                    .Type(NumberType.Integer)
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.Type)
                            .Properties(ps => ps
                                .Keyword(kw => kw
                                    .Name(nm => nm.Value)
                                )
                                .Number(nr => nr
                                    .Name(nm => nm.Id)
                                    .Type(NumberType.Integer)
                                )
                            )
                        )
                    )
                )
            );

            return createIndexResponse.Acknowledged && createIndexResponse.IsValid ? true : throw new Exception($"Failed to create observation index. Error: {createIndexResponse.DebugInformation}");
        }

        /// <summary>
        /// Add geo tile taxon result to dictionary
        /// </summary>
        /// <param name="compositeAgg"></param>
        /// <param name="taxaByGeoTile"></param>
        /// <returns></returns>
        private static int AddGeoTileTaxonResultToDictionary(
            CompositeBucketAggregate compositeAgg,
            Dictionary<string, Dictionary<int, long?>> taxaByGeoTile)
        {
            foreach (var bucket in compositeAgg.Buckets)
            {
                var geoTile = (string)bucket.Key["geoTile"];
                var taxonId = Convert.ToInt32((long)bucket.Key["taxon"]);
                if (!taxaByGeoTile.ContainsKey(geoTile)) taxaByGeoTile.Add(geoTile, new Dictionary<int, long?>());
                taxaByGeoTile[geoTile].Add(taxonId, bucket.DocCount);
            }

            return compositeAgg.Buckets.Count;
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
        /// Delete collection
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <returns></returns>
        private async Task<bool> DeleteCollectionAsync(bool protectedIndex)
        {
            var res = await Client.Indices.DeleteAsync(protectedIndex ? ProtectedIndexName : PublicIndexName);
            return res.IsValid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="query"></param>
        /// <param name="excludeQuery"></param>
        /// <returns></returns>
        private async Task<Dictionary<int, int>> GetAllObservationCountByTaxonIdAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery)
        {
            var observationCountByTaxonId = new Dictionary<int, int>();
            CompositeKey nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageTaxaCompositeAggregationAsync(indexName, query, excludeQuery, nextPageKey, pageTaxaAsyncTake);
                var compositeAgg = searchResponse.Aggregations.Composite("taxonComposite");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    var taxonId = Convert.ToInt32((long)bucket.Key["taxonId"]);
                    observationCountByTaxonId.Add(taxonId, Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0)));
                }

                nextPageKey = compositeAgg.Buckets.Count >= pageTaxaAsyncTake ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);

            return observationCountByTaxonId;
        }

        private class TaxonProvinceItem
        {
            public int TaxonId { get; set; }
            public string ProvinceId { get; set; }
            public int ObservationCount { get; set; }
        }

        private class TaxonProvinceAgg
        {
            public int TaxonId { get; set; }
            public List<string> ProvinceIds { get; set; } = new List<string>();
            public Dictionary<string, int> ObservationCountByProvinceId { get; set; } = new Dictionary<string, int>();
            public int ObservationCount { get; set; }
        }

        private async Task<Dictionary<int, TaxonProvinceAgg>> GetElasticTaxonSumAggregationByTaxonIdAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery)
        {
            List<TaxonProvinceItem> items = new List<TaxonProvinceItem>();            
            CompositeKey nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageElasticTaxonSumAggregationAsync(indexName, query, excludeQuery, nextPageKey, pageTaxaAsyncTake);
                var compositeAgg = searchResponse.Aggregations.Composite("taxonComposite");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    var taxonId = Convert.ToInt32((long)bucket.Key["taxonId"]);
                    var provinceId = bucket.Key["provinceId"].ToString();
                    var observationCount = Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0));
                    items.Add(new TaxonProvinceItem
                    {
                        TaxonId = taxonId,
                        ProvinceId = provinceId,
                        ObservationCount = observationCount
                    });                    
                }

                nextPageKey = compositeAgg.Buckets.Count >= pageTaxaAsyncTake ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);

            var dic = new Dictionary<int, TaxonProvinceAgg>();
            foreach (var item in items)
            {
                if (!dic.TryGetValue(item.TaxonId, out var taxonProvinceAgg))
                {
                    taxonProvinceAgg = new TaxonProvinceAgg() { TaxonId = item.TaxonId };
                    dic.Add(item.TaxonId, taxonProvinceAgg);                    
                }

                taxonProvinceAgg.ObservationCount += item.ObservationCount;
                taxonProvinceAgg.ProvinceIds.Add(item.ProvinceId);
                taxonProvinceAgg.ObservationCountByProvinceId.Add(item.ProvinceId, item.ObservationCount);
            }

            return dic;
        }

        /// <summary>
        /// Get core queries
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private Tuple<ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>>> GetCoreQueries(SearchFilterBase filter)
        {
            var query = filter.ToQuery();
            var excludeQuery = filter.ToExcludeQuery();

            return new Tuple<ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>,
                ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>>>(query, excludeQuery);
        }

        /// <summary>
        /// Aggregate observations by GeoTile and Taxon.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="excludeQuery"></param>
        /// <param name="zoom">The precision to use in the GeoTileGrid aggregation.</param>
        /// <param name="nextPage">The key is a combination of GeoTile string and TaxonId. Should be null in the first request.</param>
        /// <returns></returns>
        private async Task<ISearchResponse<dynamic>> PageGeoTileAndTaxaAsync(
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
            int zoom,
            CompositeKey nextPage)
        {
            ISearchResponse<dynamic> searchResponse;

            if (nextPage == null) // First request
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(PublicIndexName)
                    .Size(0)
                    .Aggregations(a => a.Composite("geoTileTaxonComposite", g => g
                        .Size(MaxNrElasticSearchAggregationBuckets + 1)
                        .Sources(src => src
                            .GeoTileGrid("geoTile", h => h
                                .Field("location.pointLocation")
                                .Precision((GeoTilePrecision)zoom).Order(SortOrder.Ascending))
                            .Terms("taxon", tt => tt
                                .Field("taxon.id").Order(SortOrder.Ascending)
                            ))))
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    ));
            }
            else
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(PublicIndexName)
                    .Size(0)
                    .Aggregations(a => a.Composite("geoTileTaxonComposite", g => g
                        .Size(MaxNrElasticSearchAggregationBuckets + 1)
                        .After(nextPage)
                        .Sources(src => src
                            .GeoTileGrid("geoTile", h => h
                                .Field("location.pointLocation")
                                .Precision((GeoTilePrecision)zoom).Order(SortOrder.Ascending))
                            .Terms("taxon", tt => tt
                                .Field("taxon.id").Order(SortOrder.Ascending)
                            ))))
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    ));
            }

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse;
        }

        private async Task<ISearchResponse<dynamic>> PageTaxaCompositeAggregationAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
            CompositeKey nextPage,
            int take)
        {
            ISearchResponse<dynamic> searchResponse;

            if (nextPage == null) // First request
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexName)
                    .Size(0)
                    .Aggregations(a => a.Composite("taxonComposite", g => g
                        .Size(take)
                        .Sources(src => src
                            .Terms("taxonId", tt => tt
                                .Field("taxon.id")
                            ))))
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    ));
            }
            else
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexName)
                    .Size(0)
                    .Aggregations(a => a.Composite("taxonComposite", g => g
                        .Size(take)
                        .After(nextPage)
                        .Sources(src => src
                            .Terms("taxonId", tt => tt
                                .Field("taxon.id")
                            ))))
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    ));
            }

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse;
        }

        private async Task<ISearchResponse<dynamic>> PageElasticTaxonSumAggregationAsync(
            string indexName,
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query,
            ICollection<Func<QueryContainerDescriptor<object>, QueryContainer>> excludeQuery,
            CompositeKey nextPage,
            int take)
        {
            ISearchResponse<dynamic> searchResponse;

            if (nextPage == null) // First request
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexName)
                    .Size(0)
                    .Aggregations(a => a.Composite("taxonComposite", g => g
                        .Size(take)
                        .Sources(src => src
                            .Terms("taxonId", tt => tt
                                .Field("taxon.id"))
                            .Terms("provinceId", p => p
                                .Field("location.province.featureId"))
                            )))
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    ));
            }
            else
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexName)
                    .Size(0)
                    .Aggregations(a => a.Composite("taxonComposite", g => g
                        .Size(take)
                        .After(nextPage)
                        .Sources(src => src
                            .Terms("taxonId", tt => tt
                                .Field("taxon.id"))
                            .Terms("provinceId", p => p
                                .Field("location.province.featureId"))
                            )))                            
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    ));
            }

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse;
        }

        /// <summary>
        /// Get public index name and also protected index name if user is authorized
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private string GetCurrentIndex(SearchFilterBase filter)
        {
            if (((filter?.ExtendedAuthorization.ObservedByMe ?? false) || (filter?.ExtendedAuthorization.ReportedByMe ?? false) || (filter?.ExtendedAuthorization.ProtectedObservations ?? false)) &&
                (filter?.ExtendedAuthorization.UserId ?? 0) == 0)
            {
                throw new AuthenticationRequiredException("Not authenticated");
            }

            if (!filter?.ExtendedAuthorization.ProtectedObservations ?? true)
            {
                return PublicIndexName;
            }

            if (!_httpContextAccessor?.HttpContext?.User?.HasAccessToScope(_elasticConfiguration.ProtectedScope) ?? true)
            {
                throw new AuthenticationRequiredException("Not authorized");
            }

            return ProtectedIndexName;
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
                );

                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                return epoch.AddMilliseconds(res.Aggregations?.Max("latestModified")?.Value ?? 0).ToUniversalTime();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to get last modified date for provider: { providerId }, index: { (protectedIndex ? ProtectedIndexName : PublicIndexName) }");
                return DateTime.MinValue;
            }
        }

        private async Task<ScrollResult<Observation>> ScrollObservationsWithCompleteObjectAsync(int dataProviderId, bool protectedIndex,
            string scrollId)
        {
            ISearchResponse<Observation> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                searchResponse = await Client
                    .SearchAsync<Observation>(s => s
                        .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                        .Query(query => query.Term(term => term.Field(obs => obs.DataProviderId).Value(dataProviderId)))
                        .Sort(s => s.Ascending(new Field("_doc")))
                        .Scroll(_elasticConfiguration.ScrollTimeout)
                        .Size(_elasticConfiguration.ScrollBatchSize)
                    );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<Observation>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            return new ScrollResult<Observation>
            {
                Records = searchResponse.Documents,
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
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
                    next => { 
                        Logger.LogDebug($"Indexing item for search:{count += next.Items.Count}"); 
                    });
        }

        /// <summary>
        /// Constructor used in public mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="client"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="telemetry"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClientManager elasticClientManager,
            IProcessClient client,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            TelemetryClient telemetry,
            IHttpContextAccessor httpContextAccessor,
            ITaxonManager taxonManager,
            ILogger<ProcessedObservationRepository> logger) : base(client, true, processedConfigurationCache, elasticConfiguration, logger)
        {
            LiveMode = true;

            _elasticConfiguration = elasticConfiguration ?? throw new ArgumentNullException(nameof(elasticConfiguration));
            _elasticClientManager = elasticClientManager ?? throw new ArgumentNullException(nameof(elasticClientManager));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Constructor used in admin mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="client"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClientManager elasticClientManager,
            IProcessClient client,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ITaxonManager taxonManager,
            ILogger<ProcessedObservationRepository> logger) : base(client, true, processedConfigurationCache, elasticConfiguration, logger)
        {
            LiveMode = false;

            _elasticConfiguration = elasticConfiguration ?? throw new ArgumentNullException(nameof(elasticConfiguration));
            _elasticClientManager = elasticClientManager ?? throw new ArgumentNullException(nameof(elasticClientManager));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
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
        public async Task<bool> CopyProviderDataAsync(DataProvider dataProvider, bool protectedIndex)
        {
            var scrollResult = await ScrollObservationsWithCompleteObjectAsync(dataProvider.Id, protectedIndex, null);

            while (scrollResult?.Records?.Any() ?? false)
            {
                var processedObservations = scrollResult.Records;
                var indexResult = WriteToElastic(processedObservations, false);

                if (indexResult.TotalNumberOfFailedBuffers != 0)
                {
                    return false;
                }

                scrollResult = await ScrollObservationsWithCompleteObjectAsync(dataProvider.Id, protectedIndex, scrollResult.ScrollId);
            }

            return true;
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
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddAggregationFilter(aggregationType);

            // Aggregation for distinct count
            static IAggregationContainer AggregationCardinality(AggregationContainerDescriptor<dynamic> agg) => agg
                .Cardinality("species_count", c => c
                    .Field("taxon.scientificName")
                );

            // Result-aggregation on taxon.id
            static IAggregationContainer Aggregation(AggregationContainerDescriptor<dynamic> agg, int size) => agg
                .Terms("species", t => t
                    .Script(s => s
                        // Build a sortable key
                        .Source("doc['taxon.attributes.sortOrder'].value + '-' + doc['taxon.scientificName'].value")
                    )
                    .Order(o => o.KeyAscending())
                    .Aggregations(thAgg => thAgg
                        .TopHits("info", info => info
                            .Size(1)
                            .Source(src => src
                                .Includes(inc => inc
                                    .Fields("taxon.id", "taxon.scientificName", "taxon.vernacularName", "taxon.scientificNameAuthorship", "taxon.attributes.redlistCategory")
                                )
                            )
                        )
                    )
                    .Order(o => o.KeyAscending())
                    .Size(size)
                );

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_Aggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            // Get number of distinct values
            var searchResponseCount = await Client.SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(indexNames)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(AggregationCardinality)
            );

            // Calculate size to fetch. If zero, get all
            var maxResult = (int?)searchResponseCount.Aggregations.Cardinality("species_count").Value ?? 0;
            var size = skip + take < maxResult ? skip + take : maxResult == 0 ? 1 : maxResult;
            if (skip == 0 && take == -1)
            {
                size = maxResult == 0 ? 1 : maxResult;
                take = maxResult;
            }

            if (aggregationType == AggregationType.SpeciesSightingsListTaxonCount)
            {
                return new PagedResult<dynamic>
                {
                    Records = new List<string>(),
                    Skip = 0,
                    Take = 0,
                    TotalCount = maxResult
                };
            }

            // Get the real result
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(indexNames)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => Aggregation(a, size))
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            _telemetry.StopOperation(operation);

            var result = searchResponse
                .Aggregations
                .Terms("species")
                .Buckets?
                .Select(b =>
                    new AggregatedSpecies
                    {
                        TaxonId = b.TopHits("info").Documents<AggregatedSpeciesInfo>().FirstOrDefault()?.Taxon.Id ?? 0,
                        DocCount = b.DocCount,
                        VernacularName = b.TopHits("info").Documents<AggregatedSpeciesInfo>().FirstOrDefault()?.Taxon.VernacularName ?? "",
                        ScientificNameAuthorship = b.TopHits("info").Documents<AggregatedSpeciesInfo>().FirstOrDefault()?.Taxon.ScientificNameAuthorship ?? "",
                        ScientificName = b.TopHits("info").Documents<AggregatedSpeciesInfo>().FirstOrDefault()?.Taxon.ScientificName ?? "",
                        RedlistCategory = b.TopHits("info").Documents<AggregatedSpeciesInfo>().FirstOrDefault()?.Taxon.RedlistCategory ?? ""
                    })?
                .Skip(skip)
                .Take(take);

            return new PagedResult<dynamic>
            {
                Records = result,
                Skip = skip,
                Take = take,
                TotalCount = maxResult
            };

            // When operation is disposed, telemetry item is sent.
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedHistogramChunkAsync(SearchFilter filter, AggregationType aggregationType)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddAggregationFilter(aggregationType);

            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            IAggregationContainer Aggregation(AggregationContainerDescriptor<dynamic> agg) => agg
                .DateHistogram("aggregation", dh => dh
                    .Field("event.startDate")
                    .CalendarInterval(DateInterval.Day)
                    .TimeZone($"{(tz.TotalMinutes > 0 ? "+" : "")}{tz.Hours:00}:{tz.Minutes:00}")
                    .Format("yyyy-MM-dd")
                    .Aggregations(a => a
                        .Sum("quantity", sum => sum
                            .Field("occurrence.organismQuantityInt")
                        )
                    )
                );

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_Aggregated_Histogram");

            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(indexNames)
                .Source(s => s.ExcludeAll())
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(Aggregation)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            var totalCount = searchResponse.HitsMetadata.Total.Value;

            _telemetry.StopOperation(operation);

            var result = searchResponse
                .Aggregations
                .DateHistogram("aggregation")
                .Buckets?
                .Select(b =>
                    new
                    {
                        Date = DateTime.Parse(b.KeyAsString),
                        b.DocCount,
                        Quantity = b.Sum("quantity").Value
                    }).ToList();

            return new PagedResult<dynamic>
            {
                Records = result,
                Skip = 0,
                Take = result?.Count ?? 0,
                TotalCount = totalCount
            };

            // When operation is disposed, telemetry item is sent.
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var sortDescriptor = await Client.GetSortDescriptorAsync<Observation>(indexNames, sortBy, sortOrder);
            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search");

            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
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

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

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
                if (!countResponse.IsValid) throw new InvalidOperationException(countResponse.DebugInformation);
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

            // When operation is disposed, telemetry item is sent.
        }

        /// <summary>
        /// Aggregate observations by GeoTile and Taxa. This method handles all paging and returns the complete result.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="zoom">The precision to use in the GeoTileGrid aggregation.</param>
        /// <returns></returns>
        public async Task<Result<IEnumerable<GeoGridTileTaxaCell>>> GetCompleteGeoTileTaxaAggregationAsync(
            SearchFilter filter,
            int zoom)
        {
            var (query, excludeQuery) = GetCoreQueries(filter);

            var taxaByGeoTile = new Dictionary<string, Dictionary<int, long?>>();
            CompositeKey nextPageKey = null;

            do
            {
                var searchResponse = await PageGeoTileAndTaxaAsync(query, excludeQuery, zoom, nextPageKey);
                var compositeAgg = searchResponse.Aggregations.Composite("geoTileTaxonComposite");
                nextPageKey = compositeAgg.AfterKey;
                AddGeoTileTaxonResultToDictionary(compositeAgg, taxaByGeoTile);
            } while (nextPageKey != null);

            var georesult = taxaByGeoTile
                .Select(b => GeoGridTileTaxaCell.Create(
                    b.Key,
                    b.Value.Select(m => new GeoGridTileTaxonObservationCount()
                    {
                        ObservationCount = (int)m.Value.GetValueOrDefault(0),
                        TaxonId = m.Key
                    }).ToList()));

            return Result.Success(georesult);
        }

        /// <inheritdoc />
        public async Task<DataQualityReport> GetDataQualityReportAsync(string organismGroup)
        {
            var index = PublicIndexName;

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Size(0)
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
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

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
                        .Query(q => q
                            .Bool(b => b
                                .Filter(f => f.Term(t => t
                                    .Field("dataQuality.uniqueKey")
                                    .Value(duplicate.UniqueKey)))
                            )
                        )
                        .Sort(sort => sort.Field(f => f.Field("dataProviderId")))
                    );

                    if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);
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
        public async Task<Result<GeoGridResult>> GetGeogridAggregationAsync(
                SearchFilter filter,
                int precision)
        {

            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_GeoAggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Aggregations(a => a.GeoHash("geohash_grid", g => g
                    .Field("location.pointLocation")
                    .Size(MaxNrElasticSearchAggregationBuckets + 1)
                    .GeoHashPrecision((GeoHashPrecision)precision)
                    .Bounds(b => b.TopLeft(filter.Location.Geometries.BoundingBox.TopLeft.ToGeoLocation()).BottomRight(filter.Location.Geometries.BoundingBox.BottomRight.ToGeoLocation()))
                    .Aggregations(b => b
                        .Cardinality("taxa_count", t => t
                            .Field("taxon.id")))
                    //.Terms("taxa_unique", t => t
                    //    .Field("taxon.id")))
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );
            if (!searchResponse.IsValid)
            {
                if (searchResponse.ServerError?.Error?.CausedBy?.Type == "too_many_buckets_exception")
                {
                    return Result.Failure<GeoGridResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower precision or a smaller bounding box.");
                }

                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations.GeoHash("geohash_grid").Buckets?.Count ?? 0;
            if (nrOfGridCells > MaxNrElasticSearchAggregationBuckets)
            {
                return Result.Failure<GeoGridResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower precision or a smaller bounding box.");
            }

            _telemetry.StopOperation(operation);

            var georesult = searchResponse
                .Aggregations
                .Terms("geohash_grid")
                .Buckets?
                .Select(b =>
                    new GridCellGeohash()
                    {
                        ObservationsCount = b.DocCount,
                        TaxaCount = (long?)b.Cardinality("taxa_count")?.Value,
                        BoundingBox = LatLonGeohashBoundingBox.CreateFromGeohash(b.Key).Value
                    });

            var gridResult = new GeoGridResult()
            {
                BoundingBox = filter.Location.Geometries.BoundingBox,
                Precision = precision,
                GridCellCount = nrOfGridCells,
                GridCells = georesult
            };

            // When operation is disposed, telemetry item is sent.
            return Result.Success(gridResult);
        }

        /// <inheritdoc />
        public async Task<Result<GeoGridTileResult>> GetGeogridTileAggregationAsync(
                SearchFilter filter,
                int zoom)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_GeoAggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Aggregations(a => a.Filter("geotile_filter", g => g
                        .Filter(f => f.GeoBoundingBox(bb => bb
                        .Field("location.pointLocation")
                        .BoundingBox(b => b.TopLeft(filter.Location.Geometries.BoundingBox.TopLeft.ToGeoLocation()).BottomRight(filter.Location.Geometries.BoundingBox.BottomRight.ToGeoLocation()))
                   ))
                    .Aggregations(ab => ab.GeoTile("geotile_grid", gg => gg
                        .Field("location.pointLocation")
                        .Size(MaxNrElasticSearchAggregationBuckets + 1)
                        .Precision((GeoTilePrecision)zoom)
                        .Aggregations(b => b
                            .Cardinality("taxa_count", t => t
                                .Field("taxon.id"))
                        )))
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );


            if (!searchResponse.IsValid)
            {
                if (searchResponse.ServerError?.Error?.CausedBy?.Type == "too_many_buckets_exception")
                {
                    return Result.Failure<GeoGridTileResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
                }

                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations?.Filter("geotile_filter")?.GeoTile("geotile_grid")?.Buckets?.Count ?? 0;
            if (nrOfGridCells > MaxNrElasticSearchAggregationBuckets)
            {
                return Result.Failure<GeoGridTileResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
            }

            _telemetry.StopOperation(operation);

            var georesult = searchResponse
                .Aggregations
                .Filter("geotile_filter")
                .GeoTile("geotile_grid")
                .Buckets?
                .Select(b => GridCellTile.Create(b.Key, b.DocCount, (long?)b.Cardinality("taxa_count").Value));

            var gridResult = new GeoGridTileResult()
            {
                BoundingBox = filter.Location.Geometries.BoundingBox,
                Zoom = zoom,
                GridCellTileCount = nrOfGridCells,
                GridCellTiles = georesult
            };

            // When operation is disposed, telemetry item is sent.
            return Result.Success(gridResult);
        }

        /// <inheritdoc />
        public async Task<Result<GeoGridMetricResult>> GetMetricGridAggregationAsync(
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
                .Size(0)
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
            );

            if (!searchResponse.IsValid)
            {
                if (searchResponse.ServerError?.Error?.CausedBy?.Type == "too_many_buckets_exception")
                {
                    return Result.Failure<GeoGridMetricResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
                }

                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            var nrOfGridCells = (int?)searchResponse.Aggregations?.Composite("gridCells")?.Buckets?.Count ?? 0;
            if (nrOfGridCells > MaxNrElasticSearchAggregationBuckets)
            {
                return Result.Failure<GeoGridMetricResult>($"The number of cells that will be returned is too large. The limit is {MaxNrElasticSearchAggregationBuckets} cells. Try using lower zoom or a smaller bounding box.");
            }

            _telemetry.StopOperation(operation);

            var gridResult = new GeoGridMetricResult()
            {
                BoundingBox = filter.Location.Geometries.BoundingBox,
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
            return Result.Success(gridResult);
        }

        /// <inheritdoc />
        public async Task<WaitForStatus> GetHealthStatusAsync(WaitForStatus waitForStatus)
        {
            try
            {
                var response = await Client.Cluster.HealthAsync(new ClusterHealthRequest() { WaitForStatus = waitForStatus });

                var healthColor = response.Status.ToString().ToLower();

                return healthColor switch
                {
                    "green" => WaitForStatus.Green,
                    "yellow" => WaitForStatus.Yellow,
                    "red" => WaitForStatus.Red,
                    _ => WaitForStatus.Red
                };
            }
            catch(Exception e)
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
        public async Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<string> locationIds)
        {
            if (!locationIds?.Any() ?? true)
            {
                return null;
            }

            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Index($"{PublicIndexName}, {ProtectedIndexName}")
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Terms(t => t
                                .Field("location.locationId")
                                .Terms(locationIds)
                            )
                        )
                    )
                )
                .Collapse(c => c.Field("location.locationId"))
               .Source(s => s
                    .Includes(i => i
                        .Field("location")
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse.Documents?.Select(d => d.Location);
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
            if (!countResponse.IsValid) throw new InvalidOperationException(countResponse.DebugInformation);

            return countResponse.Count;
        }

        /// <inheritdoc />
        public async Task<int> GetProvinceCountAsync(SearchFilterBase filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Size(0)
                .Aggregations(a => a.Cardinality("provinceCount", c => c
                    .Field("location.province.featureId")))
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                ));

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);
            int provinceCount = Convert.ToInt32(searchResponse.Aggregations.Cardinality("provinceCount").Value);
            return provinceCount;
        }

        public async Task<dynamic> GetObservationAsync(string occurrenceId, SearchFilter filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.TryAddTermCriteria("occurrence.occurrenceId", occurrenceId);
            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Get");

            operation.Telemetry.Properties["OccurrenceId"] = occurrenceId;
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            // Optional: explicitly send telemetry item:
            _telemetry.StopOperation(operation);

            return searchResponse.Documents;

        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetObservationsAsync(IEnumerable<string> occurrenceIds, bool protectedIndex)
        {
            try
            {
                var searchResponse = await Client.SearchAsync<Observation>(s => s
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Source(s => s
                        .Includes(i => i.Fields(f => f.Occurrence, f => f.Location))
                    )
                    .Query(q => q
                        .Terms(t => t
                            .Field(f => f.Occurrence.OccurrenceId)
                            .Terms(occurrenceIds)
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    throw new InvalidOperationException(searchResponse.DebugInformation);
                }

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
                .Source(p => p
                    .Includes(i => i
                        .Fields(outputFields
                            .Select(f => new Field(f)))))
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return searchResponse.Documents;
        }

        public async Task<ScrollResult<dynamic>> GetObservationsByScrollAsync(
            SearchFilter filter,
            int take,
            string sortBy,
            SearchSortOrder sortOrder,
            string scrollId)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var sortDescriptor = await Client.GetSortDescriptorAsync<Observation>(indexNames, sortBy, sortOrder);
            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search");

            operation.Telemetry.Properties["Filter"] = filter.ToString();
            ISearchResponse<dynamic> searchResponse;

            if (string.IsNullOrEmpty(scrollId))
            {
                searchResponse = await Client.SearchAsync<dynamic>(s => s
                    .Index(indexNames)
                    .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                    .Size(take)
                    .Scroll(_elasticConfiguration.ScrollTimeout)
                    .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    )
                    .Sort(sort => sortDescriptor)
                );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<dynamic>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);
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

        /// <summary>
        /// Aggregate observations by GeoTile and Taxa. This method uses paging.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="zoom">The precision to use in the GeoTileGrid aggregation.</param>
        /// <param name="geoTilePage">The GeoTile key. Should be null in the first request.</param>
        /// <param name="taxonIdPage">The TaxonId key. Should be null in the first request.</param>
        /// <returns></returns>
        public async Task<Result<GeoGridTileTaxonPageResult>> GetPageGeoTileTaxaAggregationAsync(
                SearchFilter filter,
                int zoom,
                string geoTilePage,
                int? taxonIdPage)
        {
            int maxNrBucketsInPageResult = MaxNrElasticSearchAggregationBuckets * 3;
            var (query, excludeQuery) = GetCoreQueries(filter);

            int nrAdded = 0;
            var taxaByGeoTile = new Dictionary<string, Dictionary<int, long?>>();
            CompositeKey nextPageKey = null;
            if (!string.IsNullOrEmpty(geoTilePage) && taxonIdPage.HasValue)
            {
                nextPageKey = new CompositeKey(new Dictionary<string, object> { { "geoTile", geoTilePage }, { "taxon", taxonIdPage } });
            }

            do
            {
                var searchResponse = await PageGeoTileAndTaxaAsync(query, excludeQuery, zoom, nextPageKey);
                var compositeAgg = searchResponse.Aggregations.Composite("geoTileTaxonComposite");
                nextPageKey = compositeAgg.AfterKey;
                nrAdded += AddGeoTileTaxonResultToDictionary(compositeAgg, taxaByGeoTile);
            } while (nrAdded < maxNrBucketsInPageResult && nextPageKey != null);

            var georesult = taxaByGeoTile
                .Select(b => GeoGridTileTaxaCell.Create(
                    b.Key,
                    b.Value.Select(m => new GeoGridTileTaxonObservationCount()
                    {
                        ObservationCount = (int)m.Value.GetValueOrDefault(0),
                        TaxonId = m.Key
                    }).ToList())).ToList();

            var result = new GeoGridTileTaxonPageResult
            {
                NextGeoTilePage = nextPageKey?["geoTile"].ToString(),
                NextTaxonIdPage = nextPageKey == null ? null : (int?)Convert.ToInt32((long)nextPageKey["taxon"]),
                HasMorePages = nextPageKey != null,
                GridCells = georesult
            };

            return Result.Success(result);
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
                    .Source(s => s
                        .Includes(i => i.Fields(f => f.Occurrence, f => f.Location))
                    )
                    .Size(take)
                );

                if (!searchResponse.IsValid)
                {
                    throw new InvalidOperationException(searchResponse.DebugInformation);
                }

                return searchResponse.Documents;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return null;
            }
        }

        private class TaxonAggregationTreeNodeSum
        {   
            public int TopologicalIndex { get; set; }
            public TaxonTreeNode<IBasicTaxon> TreeNode { get; set; }
            public int ObservationCount { get; set; }
            public int SumObservationCount { get; set; }
            public Dictionary<string, int> ObservationCountByProvinceId { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> SumObservationCountByProvinceId { get; set; } = new Dictionary<string, int>();
            public int ProvinceCount { get; set; }
            public int SumProvinceCount => DependentProvinceIds == null ? 0 : DependentProvinceIds.Count;
            public HashSet<string> DependentProvinceIds { get; set; }
            public HashSet<int> DependentTaxonIds { get; set; }
            //public TaxonAggregationTreeNodeSum MainParent { get; set; } // Uncomment to use for debug purpose
            //public HashSet<TaxonAggregationTreeNodeSum> SecondaryParents { get; set; } = new HashSet<TaxonAggregationTreeNodeSum>(); // Uncomment to use for debug purpose
            //public HashSet<TaxonAggregationTreeNodeSum> MainChildren { get; set; } = new HashSet<TaxonAggregationTreeNodeSum>(); // Uncomment to use for debug purpose
            //public HashSet<TaxonAggregationTreeNodeSum> SecondaryChildren { get; set; } = new HashSet<TaxonAggregationTreeNodeSum>(); // Uncomment to use for debug purpose

            public override bool Equals(object obj)
            {
                return obj is TaxonAggregationTreeNodeSum sum &&
                       EqualityComparer<TaxonTreeNode<IBasicTaxon>>.Default.Equals(TreeNode, sum.TreeNode);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(TreeNode);
            }

            public override string ToString()
            {
                if (TreeNode != null) return $"TaxonId: {TreeNode.TaxonId}, Count: {ObservationCount:N0}, SumCount: {SumObservationCount:N0}";
                return base.ToString();
            }
        }
       
        /// <inheritdoc />
        public async Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            SearchFilter filter,
            int? skip,
            int? take,
            bool sumUnderlyingTaxa = false)
        {
            Dictionary<int, int> observationCountByTaxonId = null;
            if (sumUnderlyingTaxa)
            {
                observationCountByTaxonId = await GetTaxonAggregationSumAsync(filter);
            }
            else
            {
                observationCountByTaxonId = await GetTaxonAggregationAsync(filter);                
            }
            
            // Update skip and take
            if (skip == null)
            {
                skip = 0;
            }
            if (skip > observationCountByTaxonId.Count)
            {
                skip = observationCountByTaxonId.Count;
            }
            if (take == null)
            {
                take = observationCountByTaxonId.Count - skip;
            }
            else
            {
                take = Math.Min(observationCountByTaxonId.Count - skip.Value, take.Value);
            }

            var taxaResult = observationCountByTaxonId
                .Select(b => TaxonAggregationItem.Create(
                    b.Key,
                    b.Value))
                .OrderByDescending(m => m.ObservationCount)
                .ThenBy(m => m.TaxonId)
                .Skip(skip.Value)
                .Take(take.Value)
                .ToList();

            var pagedResult = new PagedResult<TaxonAggregationItem>
            {
                Records = taxaResult,
                Skip = skip.Value,
                Take = take.Value,
                TotalCount = observationCountByTaxonId.Count
            };

            return Result.Success(pagedResult);
        }

        private async Task<Dictionary<int, int>> GetTaxonAggregationAsync(SearchFilter filter)
        {
            var indexName = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            var observationCountByTaxonId = await GetAllObservationCountByTaxonIdAsync(
                indexName,
                query,
                excludeQuery);
            return observationCountByTaxonId;
        }

        private async Task<Dictionary<int, int>> GetTaxonAggregationSumAsync(SearchFilter filter)
        {
            var indexName = GetCurrentIndex(filter);
            Dictionary<int, int> observationCountByTaxonId = null;
            Dictionary<int, int> outputCountByTaxonId = null;
            if (filter.HasTaxonFilter())
            {
                var filterWithoutTaxaFilter = filter.Clone();
                filterWithoutTaxaFilter.Taxa = null;
                var (queryWithoutTaxaFilter, excludeQueryWithoutTaxaFilter) = GetCoreQueries(filterWithoutTaxaFilter);
                observationCountByTaxonId = await GetAllObservationCountByTaxonIdAsync(
                indexName,
                queryWithoutTaxaFilter,
                excludeQueryWithoutTaxaFilter);

                var (query, excludeQuery) = GetCoreQueries(filter);
                outputCountByTaxonId = await GetAllObservationCountByTaxonIdAsync(
                    indexName,
                    query,
                    excludeQuery);

                if (filter.Taxa.IncludeUnderlyingTaxa && (filter.Taxa.Ids == null || !filter.Taxa.Ids.Any()))
                {
                    filter.Taxa.Ids = new int[] { 0 }; // Add Biota if IncludeUnderlyingTaxa and there are no Taxon Ids.
                }

                if (filter.Taxa.Ids != null && filter.Taxa.Ids.Any())
                {
                    IEnumerable<int> taxonIds = filter.Taxa.IncludeUnderlyingTaxa ?
                        _taxonManager.TaxonTree.GetUnderlyingTaxonIds(filter.Taxa.Ids, true) : filter.Taxa.Ids;

                    foreach (var taxonId in taxonIds)
                    {
                        outputCountByTaxonId.TryAdd(taxonId, 0);
                    }                    
                }
            }
            else
            {
                var (query, excludeQuery) = GetCoreQueries(filter);
                outputCountByTaxonId = await GetAllObservationCountByTaxonIdAsync(
                    indexName,
                    query,
                    excludeQuery);
                observationCountByTaxonId = outputCountByTaxonId;
            }
            
            var treeNodeSumByTaxonId = new Dictionary<int, TaxonAggregationTreeNodeSum>();
            var tree = _taxonManager.TaxonTree;            
            foreach (var item in tree.TreeNodeById.Values)
            {
                int observationCount = observationCountByTaxonId.GetValueOrDefault(item.TaxonId);                
                var sumNode = new TaxonAggregationTreeNodeSum
                {
                    TopologicalIndex = tree.ReverseTopologicalSortById[item.TaxonId],
                    TreeNode = item,
                    ObservationCount = observationCount,
                    SumObservationCount = observationCount,
                    DependentTaxonIds = new HashSet<int>() { item.TaxonId }
                };                
                treeNodeSumByTaxonId.Add(item.TaxonId, sumNode);
            }
            
            var orderedTreeNodeSum = treeNodeSumByTaxonId.Values.OrderBy(m => m.TopologicalIndex).ToList();
            foreach (var sumNode in orderedTreeNodeSum)
            {                
                // Main parent
                if (sumNode.TreeNode.Parent != null)
                {
                    if (treeNodeSumByTaxonId.TryGetValue(sumNode.TreeNode.Parent.TaxonId, out var parentSumNode))
                    {
                        // sumNode.MainParent = parentSumNode; // Uncomment to use for debug purpose
                        // parentSumNode.MainChildren.Add(sumNode); // Uncomment to use for debug purpose
                        var newDependedntTaxonIds = sumNode.DependentTaxonIds.Except(parentSumNode.DependentTaxonIds).ToList();
                        parentSumNode.DependentTaxonIds.UnionWith(newDependedntTaxonIds);
                        foreach (var taxonId in newDependedntTaxonIds)
                        {
                            parentSumNode.SumObservationCount += treeNodeSumByTaxonId[taxonId].ObservationCount;
                        }
                    }                    
                }

                // Secondary parent
                if (sumNode.TreeNode.SecondaryParents != null && sumNode.TreeNode.SecondaryParents.Count > 0)
                {
                    foreach (var secondaryParent in sumNode.TreeNode.SecondaryParents)
                    {
                        if (treeNodeSumByTaxonId.TryGetValue(secondaryParent.TaxonId, out var secondaryParentSumNode))
                        {
                            // sumNode.SecondaryParents.Add(secondaryParentSumNode); // Uncomment to use for debug purpose
                            // secondaryParentSumNode.SecondaryChildren.Add(sumNode); // Uncomment to use for debug purpose
                            var newDependentTaxonIds = sumNode.DependentTaxonIds.Except(secondaryParentSumNode.DependentTaxonIds).ToList();
                            secondaryParentSumNode.DependentTaxonIds.UnionWith(newDependentTaxonIds);
                            foreach (var taxonId in newDependentTaxonIds)
                            {
                                secondaryParentSumNode.SumObservationCount += treeNodeSumByTaxonId[taxonId].ObservationCount;
                            }
                        }                        
                    }
                }
            }

            // Replace observation count with accumulated sum. Remove nodes with 0 observations.
            foreach (var taxonId in outputCountByTaxonId.Keys)
            {
                if (treeNodeSumByTaxonId.TryGetValue(taxonId, out var sumNode))
                {
                    if (sumNode.SumObservationCount > 0)
                    {
                        outputCountByTaxonId[taxonId] = sumNode.SumObservationCount;
                    }
                    else
                    {
                        outputCountByTaxonId.Remove(taxonId);
                    }
                }
            }
            
            return outputCountByTaxonId;
        }

        /// <summary>
        /// Get taxon sum aggregation. Including underlying taxa and province count.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<Dictionary<int, TaxonSumAggregationItem>> GetTaxonSumAggregationAsync(SearchFilter filter)
        {
            var indexName = GetCurrentIndex(filter);            
            Dictionary<int, TaxonProvinceAgg> observationCountByTaxonId = null;            
            
            var filterWithoutTaxaFilter = filter.Clone();
            filterWithoutTaxaFilter.Taxa = null;
            var (queryWithoutTaxaFilter, excludeQueryWithoutTaxaFilter) = GetCoreQueries(filterWithoutTaxaFilter);
            observationCountByTaxonId = await GetElasticTaxonSumAggregationByTaxonIdAsync(
                indexName,
                queryWithoutTaxaFilter,
                excludeQueryWithoutTaxaFilter);                       
            var treeNodeSumByTaxonId = new Dictionary<int, TaxonAggregationTreeNodeSum>();
            var tree = _taxonManager.TaxonTree;
            foreach (var item in tree.TreeNodeById.Values)
            {
                var taxonProvinceAgg = observationCountByTaxonId.GetValueOrDefault(item.TaxonId);
                var sumNode = new TaxonAggregationTreeNodeSum
                {
                    TopologicalIndex = tree.ReverseTopologicalSortById[item.TaxonId],
                    TreeNode = item,
                    ObservationCount = taxonProvinceAgg == null ? 0 : taxonProvinceAgg.ObservationCount,
                    SumObservationCount = taxonProvinceAgg == null ? 0 : taxonProvinceAgg.ObservationCount,
                    ProvinceCount = taxonProvinceAgg == null ? 0 : taxonProvinceAgg.ProvinceIds.Count,
                    DependentProvinceIds = new HashSet<string>() { },
                    DependentTaxonIds = new HashSet<int>() { item.TaxonId }
                };
                if (taxonProvinceAgg != null)
                {
                    sumNode.DependentProvinceIds.UnionWith(taxonProvinceAgg.ProvinceIds);
                    foreach (var pair in taxonProvinceAgg.ObservationCountByProvinceId)
                    {
                        sumNode.ObservationCountByProvinceId.Add(pair.Key, pair.Value);
                        sumNode.SumObservationCountByProvinceId.Add(pair.Key, pair.Value);
                    }
                }

                treeNodeSumByTaxonId.Add(item.TaxonId, sumNode);
            }

            var orderedTreeNodeSum = treeNodeSumByTaxonId.Values.OrderBy(m => m.TopologicalIndex).ToList();
            foreach (var sumNode in orderedTreeNodeSum)
            {
                // Main parent
                if (sumNode.TreeNode.Parent != null)
                {
                    if (treeNodeSumByTaxonId.TryGetValue(sumNode.TreeNode.Parent.TaxonId, out var parentSumNode))
                    {
                        //sumNode.MainParent = parentSumNode; // Uncomment to use for debug purpose
                        //parentSumNode.MainChildren.Add(sumNode); // Uncomment to use for debug purpose
                        var newDependentTaxonIds = sumNode.DependentTaxonIds.Except(parentSumNode.DependentTaxonIds).ToList();
                        parentSumNode.DependentTaxonIds.UnionWith(newDependentTaxonIds);
                        parentSumNode.DependentProvinceIds.UnionWith(sumNode.DependentProvinceIds);
                        foreach (var taxonId in newDependentTaxonIds)
                        {
                            parentSumNode.SumObservationCount += treeNodeSumByTaxonId[taxonId].ObservationCount;

                            foreach (var pair in treeNodeSumByTaxonId[taxonId].ObservationCountByProvinceId)
                            {
                                if (!parentSumNode.SumObservationCountByProvinceId.TryAdd(pair.Key, pair.Value))
                                {
                                    parentSumNode.SumObservationCountByProvinceId[pair.Key] += pair.Value;
                                }                                                           
                            }
                        }
                    }
                }

                // Secondary parent
                if (sumNode.TreeNode.SecondaryParents != null && sumNode.TreeNode.SecondaryParents.Count > 0)
                {
                    foreach (var secondaryParent in sumNode.TreeNode.SecondaryParents)
                    {
                        if (treeNodeSumByTaxonId.TryGetValue(secondaryParent.TaxonId, out var secondaryParentSumNode))
                        {
                            //sumNode.SecondaryParents.Add(secondaryParentSumNode); // Uncomment to use for debug purpose
                            //secondaryParentSumNode.SecondaryChildren.Add(sumNode); // Uncomment to use for debug purpose
                            var newDependentTaxonIds = sumNode.DependentTaxonIds.Except(secondaryParentSumNode.DependentTaxonIds).ToList();
                            secondaryParentSumNode.DependentTaxonIds.UnionWith(newDependentTaxonIds);
                            secondaryParentSumNode.DependentProvinceIds.UnionWith(sumNode.DependentProvinceIds);
                            foreach (var taxonId in newDependentTaxonIds)
                            {
                                secondaryParentSumNode.SumObservationCount += treeNodeSumByTaxonId[taxonId].ObservationCount;
                                foreach (var pair in treeNodeSumByTaxonId[taxonId].ObservationCountByProvinceId)
                                {
                                    if (!secondaryParentSumNode.SumObservationCountByProvinceId.TryAdd(pair.Key, pair.Value))
                                    {
                                        secondaryParentSumNode.SumObservationCountByProvinceId[pair.Key] += pair.Value;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            var result = new Dictionary<int, TaxonSumAggregationItem>();
            foreach (var node in orderedTreeNodeSum)
            {
                var agg = new TaxonSumAggregationItem()
                {
                    TaxonId = node.TreeNode.TaxonId,
                    ObservationCount = node.ObservationCount,
                    SumObservationCount = node.SumObservationCount,
                    ProvinceCount = node.ProvinceCount,
                    SumProvinceCount = node.SumProvinceCount,
                    SumObservationCountByProvinceId = node.SumObservationCountByProvinceId                    
                };

                result.Add(node.TreeNode.TaxonId, agg);
            }
            
            return result;
        }


        /// <inheritdoc />
        public async Task<IEnumerable<TaxonAggregationItem>> GetTaxonExistsIndicationAsync(
            SearchFilter filter)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("taxon_group", t => t
                        .Size(filter.Taxa?.Ids?.Count()) // Size can never be grater than number of taxon id's
                        .Field("taxon.id")
                    )
                )
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return searchResponse.Aggregations
                .Terms("taxon_group")
                .Buckets
                .Select(b => new TaxonAggregationItem { TaxonId = int.Parse(b.Key), ObservationCount = (int)(b.DocCount ?? 0) });
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearMonthCountResult>> GetUserYearMonthCountAsync(SearchFilter filter)
        {
            try
            {
                var (query, excludeQuery) = GetCoreQueries(filter);

                var searchResponse = await Client.SearchAsync<dynamic>(s => s
                   .Index(new[] { PublicIndexName, ProtectedIndexName })
                   .Size(0)
                   .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    )
                    .Aggregations(a => a
                        .Composite("observationByYearMonth", c => c
                            .Size(1200) // 12 months * 100 year
                            .Sources(s => s
                                .Terms("startYear", t => t
                                    .Field("event.startYear")
                                    .Order(SortOrder.Descending)
                                )
                                .Terms("startMonth", t => t
                                    .Field("event.startMonth")
                                    .Order(SortOrder.Descending)
                                )
                            )
                            .Aggregations(a => a
                                .Cardinality("unique_taxonids", c => c
                                    .Field("taxon.id")
                                )
                            )
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    throw new InvalidOperationException(searchResponse.DebugInformation);
                }

                var result = new HashSet<YearMonthCountResult>();
                foreach (var bucket in searchResponse.Aggregations.Composite("observationByYearMonth").Buckets)
                {
                    var key = bucket.Key;

                    key.TryGetValue("startYear", out int startYear);
                    key.TryGetValue("startMonth", out int startMonth);
                    var count = bucket.DocCount;
                    var taxonCount = (long)bucket.Cardinality("unique_taxonids").Value;

                    result.Add(new YearMonthCountResult
                    {
                        Count = count ?? 0,
                        Month = startMonth,
                        TaxonCount = taxonCount,
                        Year = startYear
                    });
                }


                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get user year month count");
                return null!;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearMonthDayCountResult>> GetUserYearMonthDayCountAsync(SearchFilter filter, int skip, int take)
        {
            try
            {
                var (query, excludeQuery) = GetCoreQueries(filter);

                // First get observations count and taxon count group by day
                var searchResponse = await Client.SearchAsync<dynamic>(s => s
                   .Index(new[] { PublicIndexName, ProtectedIndexName })
                   .Size(0)
                   .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    )
                    .Aggregations(a => a
                        .Composite("observationByYearMonth", c => c
                            .Size(skip + take) // Take as few as possible
                            .Sources(s => s
                                .Terms("startYear", t => t
                                    .Field("event.startYear")
                                    .Order(SortOrder.Descending)
                                )
                                .Terms("startMonth", t => t
                                    .Field("event.startMonth")
                                    .Order(SortOrder.Descending)
                                )
                                .Terms("startDay", t => t
                                    .Field("event.startDay")
                                    .Order(SortOrder.Descending)
                                )
                            )
                            .Aggregations(a => a
                                .Cardinality("unique_taxonids", c => c
                                    .Field("taxon.id")
                                )
                            )
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    throw new InvalidOperationException(searchResponse.DebugInformation);
                }

                var result = new Dictionary<string, YearMonthDayCountResult>();
                foreach (var bucket in searchResponse.Aggregations.Composite("observationByYearMonth").Buckets.Skip(skip))
                {
                    var key = bucket.Key;

                    key.TryGetValue("startYear", out int startYear);
                    key.TryGetValue("startMonth", out int startMonth);
                    key.TryGetValue("startDay", out int startDay);
                    var count = bucket.DocCount;
                    var taxonCount = (long)bucket.Cardinality("unique_taxonids").Value;

                    result.Add($"{startYear}-{startMonth}-{startDay}", new YearMonthDayCountResult
                    {
                        Count = count ?? 0,
                        Day = startDay,
                        Localities = new HashSet<IdName<string>>(),
                        Month = startMonth,
                        TaxonCount = taxonCount,
                        Year = startYear
                    });
                }

                if (result.Any())
                {
                    var firstItem = result.First().Value;
                    var maxDate = new DateTime(firstItem.Year, firstItem.Month, firstItem.Day);
                    var lastItem = result.Last().Value;
                    var minDate = new DateTime(lastItem.Year, lastItem.Month, lastItem.Day);

                    // Limit search to only include time span we are interested in
                    filter.Date = filter.Date ?? new DateFilter();
                    filter.Date.StartDate = minDate;
                    filter.Date.EndDate = maxDate;
                    filter.Date.DateFilterType = DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate;

                    // Second, get all locations group by day
                    var searchResponseLocality = await Client.SearchAsync<dynamic>(s => s
                       .Index(new[] { PublicIndexName, ProtectedIndexName })
                       .Size(0)
                       .Query(q => q
                            .Bool(b => b
                                .MustNot(excludeQuery)
                                .Filter(query)
                            )
                        )
                        .Aggregations(a => a
                            .Composite("localityByYearMonth", c => c
                                .Size((skip + take) * 10) // 10 locations for one day must be enought
                                .Sources(s => s
                                    .Terms("startYear", t => t
                                        .Field("event.startYear")
                                        .Order(SortOrder.Descending)
                                    )
                                    .Terms("startMonth", t => t
                                        .Field("event.startMonth")
                                        .Order(SortOrder.Descending)
                                    )
                                    .Terms("startDay", t => t
                                        .Field("event.startDay")
                                        .Order(SortOrder.Descending)
                                    )
                                     .Terms("locationId", t => t
                                        .Field("location.locationId")
                                        .Order(SortOrder.Descending)
                                    )
                                      .Terms("locality", t => t
                                        .Field("location.locality")
                                        .Order(SortOrder.Descending)
                                    )
                                )

                            )
                        )
                    );

                    if (!searchResponseLocality.IsValid)
                    {
                        throw new InvalidOperationException(searchResponseLocality.DebugInformation);
                    }

                    // Add locations to result
                    foreach (var bucket in searchResponseLocality.Aggregations.Composite("localityByYearMonth").Buckets)
                    {
                        var key = bucket.Key;

                        key.TryGetValue("startYear", out int startYear);
                        key.TryGetValue("startMonth", out int startMonth);
                        key.TryGetValue("startDay", out int startDay);
                        key.TryGetValue("locationId", out string locationId);
                        key.TryGetValue("locality", out string locality);
                        var itemKey = $"{startYear}-{startMonth}-{startDay}";

                        if (result.TryGetValue(itemKey, out var item))
                        {
                            item.Localities.Add(new IdName<string> { Id = locationId, Name = locality });
                        }                  
                    }
                }

                return result.Values;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get user year month day count");
                return null!;
            }
        }

        /// <inheritdoc />
        public async Task<bool> HasIndexOccurrenceIdDuplicatesAsync(bool protectedIndex)
        {
            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Size(0)
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Aggregations(a => a
                    .Terms("uniqueOccurrenceIdCount", t => t
                        .Field(f => f.Occurrence.OccurrenceId)
                        .MinimumDocumentCount(2)
                        .Size(1)
                    )
                )
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return searchResponse.Aggregations
                .Terms("uniqueOccurrenceIdCount")
                .Buckets.Count > 0;
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

                if (!countResponse.IsValid)
                {
                    throw new InvalidOperationException(countResponse.DebugInformation);
                }

                return countResponse.Count;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return -1;
            }
        }

        public int MaxNrElasticSearchAggregationBuckets => _elasticConfiguration.MaxNrAggregationBuckets;

        /// <inheritdoc />
        public string PublicIndexName => IndexHelper.GetIndexName<Observation>(_elasticConfiguration.IndexPrefix, _elasticClientManager.Clients.Length == 1, LiveMode ? ActiveInstance : InActiveInstance, false);

        /// <inheritdoc />
        public string ProtectedIndexName => IndexHelper.GetIndexName<Observation>(_elasticConfiguration.IndexPrefix, _elasticClientManager.Clients.Length == 1, LiveMode ? ActiveInstance : InActiveInstance, true);

        /// <inheritdoc />
        public async Task<ScrollResult<ExtendedMeasurementOrFactRow>> ScrollMeasurementOrFactsAsync(
            SearchFilterBase filter,
            string scrollId)
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
                    .Scroll(_elasticConfiguration.ScrollTimeout)
                    .Size(_elasticConfiguration.ScrollBatchSize)
                );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<Observation>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return new ScrollResult<ExtendedMeasurementOrFactRow>
            {
                Records = CastDynamicsToObservations(searchResponse.Documents)?.ToExtendedMeasurementOrFactRows(),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <inheritdoc />
        public async Task<ScrollResult<SimpleMultimediaRow>> ScrollMultimediaAsync(
            SearchFilterBase filter,
            string scrollId)
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
                    .Sort(s => s.Ascending(new Field("_doc")))
                    .Scroll(_elasticConfiguration.ScrollTimeout)
                    .Size(_elasticConfiguration.ScrollBatchSize)
                );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<dynamic>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);


            return new ScrollResult<SimpleMultimediaRow>
            {
                Records = CastDynamicsToObservations(searchResponse.Documents)?.ToSimpleMultimediaRows(),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <inheritdoc />
        public async Task<ScrollResult<Observation>> ScrollObservationsAsync(
            SearchFilterBase filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;

            if (string.IsNullOrEmpty(scrollId))
            {
                var query = filter.ToQuery();
                var projection = new SourceFilterDescriptor<dynamic>()
                    .Excludes(e => e
                        .Field("artportalenInternal")
                        .Field("location.point")
                        .Field("location.pointLocation")
                        .Field("location.pointWithBuffer")
                        .Field("location.pointWithDisturbanceBuffer")
                    );
                var indexNames = GetCurrentIndex(filter);
                searchResponse = await Client
                    .SearchAsync<dynamic>(s => s
                        .Index(indexNames)
                        .Source(p => projection)
                        .Query(q => q
                            .Bool(b => b
                                .Filter(query)
                            )
                        )
                        .Sort(s => s.Ascending(new Field("_doc")))
                        .Scroll(_elasticConfiguration.ScrollTimeout)
                        .Size(_elasticConfiguration.ScrollBatchSize)
                    );

            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<Observation>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return new ScrollResult<Observation>
            {
                Records = CastDynamicsToObservations(searchResponse.Documents),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata?.Total?.Value ?? 0
            };
        }

        public async Task<ScrollResult<dynamic>> ScrollObservationsAsDynamicAsync(
            SearchFilter filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;

            if (string.IsNullOrEmpty(scrollId))
            {
                var query = filter.ToQuery();
                var projection = new SourceFilterDescriptor<dynamic>()
                    .Excludes(e => e
                        .Field("artportalenInternal")
                        .Field("location.point")
                        .Field("location.pointLocation")
                        .Field("location.pointWithBuffer")
                        .Field("location.pointWithDisturbanceBuffer")
                    );
                var indexNames = GetCurrentIndex(filter);
                searchResponse = await Client
                    .SearchAsync<dynamic>(s => s
                        .Index(indexNames)
                        .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                        .Query(q => q
                            .Bool(b => b
                                .Filter(query)
                            )
                        )
                        .Sort(s => s.Ascending(new Field("_doc")))
                        .Scroll(_elasticConfiguration.ScrollTimeout)
                        .Size(_elasticConfiguration.ScrollBatchSize)
                    );

            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<dynamic>(_elasticConfiguration.ScrollTimeout, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return new ScrollResult<dynamic>
            {
                Records = searchResponse.Documents,
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata?.Total?.Value ?? 0
            };
        }

        /// <inheritdoc />
        public async Task<bool> SignalSearchInternalAsync(
            SearchFilter filter,
            bool onlyAboveMyClearance)
        {
            // Save user extended authorization to use later
            var extendedAuthorizations = filter.ExtendedAuthorization?.ExtendedAreas;
            // Authorization is handled different in signal search, reset some values before we get core queries
            filter.ExtendedAuthorization.ExtendedAreas = null;
            filter.ExtendedAuthorization.UserId = 0;
            filter.ExtendedAuthorization.ProtectedObservations = false;

            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddSignalSearchCriteria(extendedAuthorizations, onlyAboveMyClearance);

            var searchResponse = await Client.CountAsync<dynamic>(s => s
                .Index(ProtectedIndexName)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse.Count > 0;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> TryToGetOccurenceIdDuplicatesAsync(bool activeInstance, bool protectedIndex, int maxReturnedItems)
        {
            var searchResponse = await (activeInstance ? Client : InActiveClient).SearchAsync<dynamic>(s => s
                .Size(0)
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Source(s => s.ExcludeAll())
                .Aggregations(a => a
                    .Terms("OccurrenceIdDuplicatesExists", f => f
                        .Field("occurrence.occurrenceId")
                        .MinimumDocumentCount(2)
                        .Size(maxReturnedItems)
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse.Aggregations.Terms("OccurrenceIdDuplicatesExists").Buckets?.Select(b => b.Key);
        }

        /// <inheritdoc />
        public string UniquePublicIndexName => IndexHelper.GetIndexName<Observation>(_elasticConfiguration.IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

        /// <inheritdoc />
        public string UniqueProtectedIndexName => IndexHelper.GetIndexName<Observation>(_elasticConfiguration.IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, true);

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

                if (!countResponse.IsValid)
                {
                    throw new InvalidOperationException(countResponse.DebugInformation);
                }
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
    }
}
