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
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.DataQuality;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.AggregatedResult;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Repositories.Processed.Interfaces;
using DateTime = System.DateTime;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Species data service
    /// </summary>b
    public class ProcessedObservationRepository : ProcessedObservationBaseRepository,
        IProcessedObservationRepository
    {
        private const int ElasticSearchMaxRecords = 10000;
        private readonly TelemetryClient _telemetry;

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
                            .IgnoreAbove(int.MaxValue)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.InstitutionId)
                            .Index(true)
                            .IgnoreAbove(int.MaxValue)
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
                            .IgnoreAbove(int.MaxValue)
                        )
                        .Keyword(kw => kw
                            .Name(nm => nm.PublicCollection)
                            .Index(true)
                            .IgnoreAbove(int.MaxValue)
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
                            .IgnoreAbove(int.MaxValue)
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
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project1Category)
                                    .Index(true) // WFS
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project1Url)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project1Values)
                                    .Index(true)
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project2Name)
                                    .Index(true) // WFS
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project2Category)
                                    .Index(true) // WFS
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project2Url)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.Project2Values)
                                    .Index(true)
                                    .IgnoreAbove(int.MaxValue)
                                )
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
                                .Keyword(kw => kw
                                    .Name(nm => nm.LocationExternalId)
                                    .Index(true)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.LocationPresentationNameParishRegion)
                                    .Index(false)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ParentLocality)
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ReportedByUserAlias)
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.SightingBarcodeURL)
                                    .IgnoreAbove(int.MaxValue)
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
                                            .IgnoreAbove(int.MaxValue)
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
                            .Properties(ps => ps.GetMapping())
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
                            .Properties(ps => ps.GetMapping())
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
                                .Keyword(kw => kw
                                    .Name(nm => nm.AssociatedMedia)
                                    .Index(true)
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Text(t => t
                                    .Name(nm => nm.OccurrenceRemarks)
                                    .IndexOptions(IndexOptions.Docs)
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
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.CatalogNumber)
                                    .IgnoreAbove(int.MaxValue)
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
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OccurrenceStatus)
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.OrganismQuantity)
                                    .IgnoreAbove(int.MaxValue)
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
                                    .IgnoreAbove(int.MaxValue)
                                )
                                .Keyword(kw => kw
                                    .Name(nm => nm.ReportedBy)
                                    .IgnoreAbove(int.MaxValue)
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
                                            .Properties(ps => ps.GetMapping())
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

                    if (!searchResponse.IsValid)
                    {
                        throw new InvalidOperationException(searchResponse.DebugInformation);
                    }
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

                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                return epoch.AddMilliseconds(res.Aggregations?.Max("latestModified")?.Value ?? 0).ToUniversalTime();
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

                if (!queryResponse.IsValid)
                {
                    throw new InvalidOperationException(queryResponse.DebugInformation);
                }

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

        /// <summary>
        /// Constructor used in public mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="telemetry"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            TelemetryClient telemetry,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProcessedObservationRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, httpContextAccessor, elasticConfiguration, logger)
        {
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Constructor used in admin mode
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<ProcessedObservationRepository> logger) : base(false, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
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
        /// <param name="activeInstance"></param>
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
        public async Task<PagedResult<dynamic>> GetAggregatedChunkAsync(SearchFilter filter, AggregationType aggregationType, int skip, int take)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddAggregationFilter(aggregationType);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_Aggregated");
            operation.Telemetry.Properties["Filter"] = filter.ToString();

            // Get number of distinct values
            var searchResponseCount = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Cardinality("species_count", c => c
                        .Field("taxon.scientificName")
                    )
                )
                .Size(0)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                .TrackTotalHits(false)
            );

            var maxResult = (int?)searchResponseCount.Aggregations.Cardinality("species_count").Value ?? 0;
            
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

            // Calculate size to fetch. If zero, get all
            var size = skip + take < maxResult ? skip + take : maxResult == 0 ? 1 : maxResult;
            if (skip == 0 && take == -1)
            {
                size = maxResult == 0 ? 1 : maxResult;
                take = maxResult;
            }

            // Get the real result
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Composite("species", c => c
                        .Size(size)
                        .Sources(s => s
                            .Terms("sortOrder", t => t
                                .Field("taxon.attributes.sortOrder")
                             )
                            .Terms("scientificName", t => t
                                .Field("taxon.scientificName")
                             )
                            .Terms("id", t => t
                                .Field("taxon.id")
                            )
                            .Terms("vernacularName", t => t
                                .Field("taxon.vernacularName")
                                .MissingBucket(true)
                            )
                            .Terms("scientificNameAuthorship", t => t
                                .Field("taxon.scientificNameAuthorship")
                                .MissingBucket(true)
                            )
                            .Terms("redlistCategory", t => t
                                .Field("taxon.attributes.redlistCategory")
                                .MissingBucket(true)
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            _telemetry.StopOperation(operation);

            var result = searchResponse
                .Aggregations
                .Composite("species")
                .Buckets?
                .Select(b =>
                        new AggregatedSpecies
                        {
                            DocCount = b.DocCount,
                            RedlistCategory = b.Key["redlistCategory"]?.ToString() ?? "",
                            ScientificNameAuthorship = b.Key["scientificNameAuthorship"]?.ToString() ?? "",
                            ScientificName = b.Key["scientificName"]?.ToString() ?? "",
                            TaxonId = int.Parse(b.Key["id"].ToString()),
                            VernacularName = b.Key["vernacularName"]?.ToString() ?? ""
                        }
                    )?
                .Skip(skip)
                .Take(take);

            return new PagedResult<dynamic>
            {
                Records = result,
                Skip = skip,
                Take = take,
                TotalCount = maxResult
            };
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetAggregatedHistogramChunkAsync(SearchFilter filter, AggregationType aggregationType)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            query.AddAggregationFilter(aggregationType);

            using var operation = _telemetry.StartOperation<DependencyTelemetry>("Observation_Search_Aggregated_Histogram");

            operation.Telemetry.Properties["Filter"] = filter.ToString();
            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .DateHistogram("aggregation", dh => dh
                        .Field("event.startDate")
                        .CalendarInterval(aggregationType switch {
                            AggregationType.QuantityPerYear  => DateInterval.Year,
                            AggregationType.SightingsPerYear => DateInterval.Year,
                            AggregationType.QuantityPerWeek => DateInterval.Week,
                            AggregationType.SightingsPerWeek => DateInterval.Week,
                            _ => DateInterval.Day
                        }  
                        )
                        .TimeZone($"{(tz.TotalMinutes > 0 ? "+" : "")}{tz.Hours:00}:{tz.Minutes:00}")
                        .Format("yyyy-MM-dd")
                        .Aggregations(a => a
                            .Sum("quantity", sum => sum
                                .Field("occurrence.organismQuantityInt")
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
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
                .Aggregations(a => a
                    .Filter("geotile_filter", g => g
                        .Filter(f => f
                            .GeoBoundingBox(bb => bb
                                .Field("location.pointLocation")
                                .BoundingBox(b => b
                                    .TopLeft(filter.Location.Geometries.BoundingBox.TopLeft.ToGeoLocation())
                                    .BottomRight(filter.Location.Geometries.BoundingBox.BottomRight.ToGeoLocation())
                               )
                            )
                        )
                        .Aggregations(ab => ab
                            .GeoTile("geotile_grid", gg => gg
                                .Field("location.pointLocation")
                                .Size(MaxNrElasticSearchAggregationBuckets + 1)
                                .Precision((GeoTilePrecision)(zoom))
                                .Aggregations(b => b
                                    .Cardinality("taxa_count", t => t
                                    .Field("taxon.id"))
                                )
                            )
                        )
                    )
                )
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
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
                .Aggregations(a => a.Cardinality("provinceCount", c => c
                    .Field("location.province.featureId")))
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

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
                
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Size(1)
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                .TrackTotalHits(false)
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

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

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
                Records = CastDynamicsToObservations(searchResponse.Documents)?.ToExtendedMeasurementOrFactRows(),
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
                Records = CastDynamicsToObservations(searchResponse.Documents)?.ToSimpleMultimediaRows(),
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
                .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                    .Query(q => q
                        .Bool(b => b
                            .Filter(filter.ToQuery())
                        )
                    ),
                pointInTimeId,
                searchAfter);

            return new SearchAfterResult<T>
            {
                Records = (IEnumerable<T>)(typeof(T).Equals(typeof(Observation)) ? CastDynamicsToObservations(searchResponse.Documents) : searchResponse.Documents),
                PointInTimeId = searchResponse.PointInTimeId,
                SearchAfter = searchResponse.Hits?.LastOrDefault()?.Sorts
            };
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
                        .Source(filter.OutputFields.ToProjection(filter is SearchFilterInternal))
                        .Scroll(ScrollTimeout)
                    ) : await Client
                        .ScrollAsync<dynamic>(ScrollTimeout, scrollId);

                if (!queryResponse.IsValid) {
                    throw new InvalidOperationException(queryResponse.DebugInformation);
                }
                
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
        public async Task<IEnumerable<YearCountResult>> GetUserYearCountAsync(SearchFilter filter)
        {
            try
            {
                var (query, excludeQuery) = GetCoreQueries(filter);

                var searchResponse = await Client.SearchAsync<dynamic>(s => s
                   .Index(new[] { PublicIndexName, ProtectedIndexName })
                   .Query(q => q
                        .Bool(b => b
                            .MustNot(excludeQuery)
                            .Filter(query)
                        )
                    )
                    .Aggregations(a => a
                        .Composite("observationByYear", c => c
                            .Size(100) // 100 years
                            .Sources(s => s
                                .Terms("startYear", t => t
                                    .Field("event.startYear")
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
                    .Size(0)
                    .Source(s => s.ExcludeAll())
                    .TrackTotalHits(false)
                );

                if (!searchResponse.IsValid)
                {
                    throw new InvalidOperationException(searchResponse.DebugInformation);
                }

                var result = new HashSet<YearCountResult>();
                foreach (var bucket in searchResponse.Aggregations.Composite("observationByYear").Buckets)
                {
                    var key = bucket.Key;

                    key.TryGetValue("startYear", out int startYear);
                    var count = bucket.DocCount;
                    var taxonCount = (long)bucket.Cardinality("unique_taxonids").Value;

                    result.Add(new YearCountResult
                    {
                        Count = count ?? 0,
                        TaxonCount = taxonCount,
                        Year = startYear
                    });
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get user year count");
                return null!;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<YearMonthCountResult>> GetUserYearMonthCountAsync(SearchFilter filter)
        {
            try
            {
                var (query, excludeQuery) = GetCoreQueries(filter);

                var searchResponse = await Client.SearchAsync<dynamic>(s => s
                   .Index(new[] { PublicIndexName, ProtectedIndexName })
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
                    .Size(0)
                    .Source(s => s.ExcludeAll())
                    .TrackTotalHits(false)
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
                    .Size(0)
                    .Source(s => s.ExcludeAll())
                    .TrackTotalHits(false)
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
                        .Size(0)
                        .Source(s => s.ExcludeAll())
                        .TrackTotalHits(false)
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
                .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                .Aggregations(a => a
                    .Terms("uniqueOccurrenceIdCount", t => t
                        .Field(f => f.Occurrence.OccurrenceId)
                        .MinimumDocumentCount(2)
                        .Size(1)
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
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
                    .Scroll(ScrollTimeout)
                    .Size(ScrollBatchSize)
                );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<Observation>(ScrollTimeout, scrollId);
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
                    .Scroll(ScrollTimeout)
                    .Size(ScrollBatchSize)
                );
            }
            else
            {
                searchResponse = await Client
                    .ScrollAsync<dynamic>(ScrollTimeout, scrollId);
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
                    .ScrollAsync<Observation>(ScrollTimeout, scrollId);

                if (!queryResponse.IsValid)
                {
                    throw new InvalidOperationException(queryResponse.DebugInformation);
                }

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

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

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

        public async Task<PagedResult<UserStatisticsItem>> PagedSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter, int? skip, int? take)
        {
            //var testres1 = await TopListGetProvinceCrossLeagueAsync(filter, skip.GetValueOrDefault(), take.GetValueOrDefault());
            var query = filter.ToProcessedObservationQuery<Observation>();
            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Nested("league", n => n
                        .Path("artportalenInternal.occurrenceRecordedByInternal")
                        .Aggregations(ag => ag
                            .Terms("taxaCountByUserId", t => t
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                                .Size(65536)
                                .Aggregations(a => a
                                    .ReverseNested("recorder", rn => rn
                                        .Aggregations(aa => aa
                                            .Cardinality("taxaCount", c => c
                                                .Field("taxon.id")
                                            )
                                        )
                                    )
                                    .BucketSort("sortByTaxaCount", b => b
                                        .Sort(s => s
                                            .Descending("recorder.taxaCount"))
                                        .From(skip)
                                        .Size(take)
                                    )
                                )
                            )
                            .Cardinality("userCount", t => t
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                            )
                        )
                    )
                )
                .Size(0)
                .Source(so => so.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            var buckets = searchResponse
                .Aggregations
                .Nested("league")
                .Terms("taxaCountByUserId")
                .Buckets
                .Select(b => new UserStatisticsItem { UserId = Convert.ToInt32(b.Key), ObservationCount = Convert.ToInt32(b.DocCount ?? 0), SpeciesCount = Convert.ToInt32(b.ReverseNested("recorder").Cardinality("taxaCount").Value) }).ToList();

            int totalCount = Convert.ToInt32(searchResponse.Aggregations.Nested("league").Cardinality("userCount").Value ?? 0);

            // Update skip and take
            if (skip == null)
            {
                skip = 0;
            }
            if (skip > totalCount)
            {
                skip = totalCount;
            }
            if (take == null)
            {
                take = totalCount - skip;
            }
            else
            {
                take = Math.Min(totalCount - skip.Value, take.Value);
            }

            var pagedResult = new PagedResult<UserStatisticsItem>
            {
                Records = buckets,
                Skip = skip.Value,
                Take = take.Value,
                TotalCount = totalCount
            };

            return pagedResult;
        }

        public async Task<List<UserStatisticsItem>> AreaSpeciesCountSearchAsync(SpeciesCountUserStatisticsQuery filter, List<int> userIds)
        {
            var query = filter.ToProcessedObservationQuery<Observation>();
            query.TryAddNestedTermsCriteria("artportalenInternal.occurrenceRecordedByInternal", "id", userIds);
            var searchResponse = await Client.SearchAsync<Observation>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Nested("league", n => n
                        .Path("artportalenInternal.occurrenceRecordedByInternal")
                        .Aggregations(ag => ag
                            .Terms("taxaCountByUserId", t => t
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                                .Size(65536) // userIds.Count?
                                .Aggregations(a => a
                                    .ReverseNested("recorder", rn => rn
                                        .Aggregations(a => a
                                            .Terms("province", t => t
                                                .Field("location.province.featureId")
                                                .Size(33) // max number of provinces
                                                .Aggregations(a => a
                                                    .Cardinality("taxonCount", c => c
                                                        .Field("taxon.id")
                                                    )
                                                )
                                            )
                                            .Cardinality("sumTaxaCount", c => c
                                                .Field("taxon.id")
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
                .Size(0)
                .Source(so => so.ExcludeAll())
                .TrackTotalHits(false)
            );
            

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            var items = searchResponse
                .Aggregations
                .Nested("league")
                .Terms("taxaCountByUserId")
                .Buckets
                .Select(b => new UserStatisticsItem
                {
                    UserId = Convert.ToInt32(b.Key), 
                    ObservationCount = Convert.ToInt32(b.DocCount ?? 0), 
                    SpeciesCount = Convert.ToInt32(b.ReverseNested("recorder").Cardinality("sumTaxaCount").Value),
                    SpeciesCountByFeatureId = b
                        .ReverseNested("recorder")
                        .Terms("province")
                        .Buckets
                        .ToDictionary(bu => bu.Key, bu => Convert.ToInt32(bu.Cardinality("taxonCount").Value.GetValueOrDefault()))
                }).ToList();

            return items;
        }

        #region Top Lists (experimental code)

        //public async Task<PagedResult<dynamic>> TopListGetProvinceCrossLeagueAsync(SearchFilter filter, int skip, int take)
        public async Task<PagedResult<dynamic>> TopListGetProvinceCrossLeagueAsync(SpeciesCountUserStatisticsQuery filter, int skip, int take)
        {
            var query = filter.ToQuery<Observation>();
            //var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<Observation>(s => s
               .Index(new[] { PublicIndexName, ProtectedIndexName })
               .Query(q => q
                    .Bool(b => b
                        //.MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Nested("league", n => n
                        .Path("artportalenInternal.occurrenceRecordedByInternal")
                        .Aggregations(a => a
                            .Terms("recordedBy", t => t
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                                .Size(skip + take)
                                .Aggregations(a => a
                                    .ReverseNested("recorder", rn => rn
                                        .Aggregations(a => a
                                            .Terms("province", t => t
                                                .Field("location.province.name")
                                                .Size(33)
                                                .Aggregations(a => a
                                                    .Cardinality("taxonCount", c => c
                                                        .Field("taxon.id")
                                                    )
                                                )
                                            )
                                            .SumBucket("taxonCount", sb => sb
                                                .BucketsPath("province.taxonCount")
                                            )
                                        )
                                    )
                                    .BucketSort("leagueSort", bs => bs
                                        .Sort(s => s
                                            .Descending("recorder.taxonCount")
                                        )
                                    )
                                )
                            )
                            .Cardinality("recordedByCount", c => c
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
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
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return new PagedResult<dynamic>
            {
                Skip = skip,
                Take = take,
                TotalCount = (long)searchResponse.Aggregations.Nested("league").Cardinality("recordedByCount").Value,
                Records = searchResponse.Aggregations.Nested("league").Terms("recordedBy").Buckets.Skip(skip).Take(take).Select(b =>
                    new
                    {
                        Id = b.Key,
                        Provinces = b.ReverseNested("recorder").Terms("province").Buckets.Select(b => new
                        {
                            Id = b.Key,
                            TaxonCount = (int)b.Cardinality("taxonCount").Value
                        })
                    }
                )
            };
        }

        public async Task<PagedResult<dynamic>> TopListGetLocationLeagueAsync(SearchFilter filter, int skip, int take)
        {
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
               .Index(new[] { PublicIndexName, ProtectedIndexName })
               .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("localities", c => c
                        .Field("location.locationId")
                        .Size(skip + take)
                        .Aggregations(a => a
                            .Cardinality("taxonCount", c => c
                                .Field("taxon.id")
                            )
                            .BucketSort("sort", bs => bs
                                .Sort(s => s
                                    .Descending("taxonCount.value")
                                )
                            )
                        )
                    )
                    .Cardinality("localityCount", c => c
                        .Field("location.locationId")
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return new PagedResult<dynamic>
            {
                Skip = skip,
                Take = take,
                TotalCount = (long)searchResponse.Aggregations.Cardinality("localityCount").Value,
                Records = searchResponse.Aggregations.Terms("localities").Buckets.Skip(skip).Take(take).Select(b =>
                    new
                    {
                        Id = b.Key,
                        TaxonCount = (int)b.Cardinality("taxonCount").Value
                    }
                )
            };
        }

        /// <inheritdoc/>
        public async Task<PagedResult<dynamic>> TopListGetMonthCrossLeagueAsync(SearchFilter filter, int skip, int take)
        {
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
               .Index(new[] { PublicIndexName, ProtectedIndexName })
               .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Nested("league", n => n
                        .Path("artportalenInternal.occurrenceRecordedByInternal")
                        .Aggregations(a => a
                            .Terms("recordedBy", t => t
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
                                .Size(skip + take)
                                .Aggregations(a => a
                                    .ReverseNested("recorder", rn => rn
                                        .Aggregations(a => a
                                            .Terms("months", t => t
                                                .Field("event.startMonth")
                                                .Size(12)
                                                .Aggregations(a => a
                                                    .Cardinality("taxonCount", c => c
                                                        .Field("taxon.id")
                                                    )
                                                )
                                            )
                                            .SumBucket("taxonCount", sb => sb
                                                .BucketsPath("months.taxonCount")
                                            )
                                        )
                                    )
                                    .BucketSort("leagueSort", bs => bs
                                        .Sort(s => s
                                            .Descending("recorder.taxonCount")
                                        )
                                    )
                                )
                            )
                            .Cardinality("recordedByCount", c => c
                                .Field("artportalenInternal.occurrenceRecordedByInternal.id")
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
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return new PagedResult<dynamic>
            {
                Skip = skip,
                Take = take,
                Records = searchResponse.Aggregations.Nested("league").Terms("recordedBy").Buckets.Skip(skip).Take(take).Select(b =>
                    new
                    {
                        Id = b.Key,
                        Provinces = b.ReverseNested("recorder").Terms("months").Buckets.Select(b => new
                        {
                            Id = b.Key,
                            TaxonCount = (int)b.Cardinality("taxonCount").Value
                        })
                    }
                ),
                TotalCount = (long)searchResponse.Aggregations.Nested("league").Cardinality("recordedByCount").Value
            };
        }

        public async Task<PagedResult<dynamic>> TopListGetMunicipalityLeagueAsync(SearchFilter filter, int skip, int take)
        {
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
               .Index(new[] { PublicIndexName, ProtectedIndexName })
               .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("municipalities", c => c
                        .Field("location.municipality.featureId")
                        .Size(skip + take)
                        .Aggregations(a => a
                            .Cardinality("taxonCount", c => c
                                .Field("taxon.id")
                            )
                            .BucketSort("sort", bs => bs
                                .Sort(s => s
                                    .Descending("taxonCount.value")
                                )
                            )
                        )
                    )
                    .Cardinality("municipalityCount", c => c
                        .Field("location.municipality.featureId")
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return new PagedResult<dynamic>
            {
                Skip = skip,
                Take = take,
                TotalCount = (long)searchResponse.Aggregations.Cardinality("municipalityCount").Value,
                Records = searchResponse.Aggregations.Terms("municipalities").Buckets.Skip(skip).Take(take).Select(b =>
                    new
                    {
                        Id = b.Key,
                        TaxonCount = (int)b.Cardinality("taxonCount").Value
                    }
                )
            };
        }

        /// <inheritdoc/>
        public async Task<PagedResult<dynamic>> TopListGetProvinceLeagueAsync(SearchFilter filter, int skip, int take)
        {
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
               .Index(new[] { PublicIndexName, ProtectedIndexName })
               .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("provinces", c => c
                        .Field("location.province.featureId")
                        .Size(skip + take)
                        .Aggregations(a => a
                            .Cardinality("taxonCount", c => c
                                .Field("taxon.id")
                            )
                            .BucketSort("sort", bs => bs
                                .Sort(s => s
                                    .Descending("taxonCount.value")
                                )
                            )
                        )
                    )
                    .Cardinality("provinceCount", c => c
                        .Field("location.province.featureId")
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return new PagedResult<dynamic>
            {
                Skip = skip,
                Take = take,
                TotalCount = (long)searchResponse.Aggregations.Cardinality("provinceCount").Value,
                Records = searchResponse.Aggregations.Terms("provinces").Buckets.Skip(skip).Take(take).Select(b =>
                    new
                    {
                        Id = b.Key,
                        TaxonCount = (int)b.Cardinality("taxonCount").Value
                    }
                )
            };
        }

        /// <inheritdoc/>
        public async Task<PagedResult<dynamic>> TopListGetReportingVolumeLeagueAsync(SearchFilter filter, int skip, int take)
        {
            var (query, excludeQuery) = GetCoreQueries(filter);

            var searchResponse = await Client.SearchAsync<dynamic>(s => s
               .Index(new[] { PublicIndexName, ProtectedIndexName })
               .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .Aggregations(a => a
                    .Terms("reportedBy", c => c
                        .Field("artportalenInternal.reportedByUserId")
                        .Size(skip + take)
                    )
                    .Cardinality("reportedByCount", c => c
                        .Field("artportalenInternal.reportedByUserId")
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return new PagedResult<dynamic>
            {
                Skip = skip,
                Take = take,
                TotalCount = (long)searchResponse.Aggregations.Cardinality("reportedByCount").Value,
                Records = searchResponse.Aggregations.Terms("reportedBy").Buckets.Skip(skip).Take(take).Select(b =>
                    new
                    {
                        Id = b.Key,
                        TaxonCount = b.DocCount
                    }
                )
            };
        }

        #endregion
    }
}
