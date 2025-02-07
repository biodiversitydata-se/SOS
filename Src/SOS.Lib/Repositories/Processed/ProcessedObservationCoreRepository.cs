using CSharpFunctionalExtensions;
using Elasticsearch.Net;
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
            var createIndexResponse = await Client.Indices.CreateAsync(protectedIndex ? ProtectedIndexName : PublicIndexName, s => s
                .Settings(s => s
                    .NumberOfShards(protectedIndex ? NumberOfShardsProtected : NumberOfShards)
                    .NumberOfReplicas(NumberOfReplicas)
                    .Setting("max_terms_count", 110000)
                    .Setting(UpdatableIndexSettings.MaxResultWindow, 100000)
                )
                .Map<Observation>(m => m
                    .AutoMap<Observation>()
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.Id, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.DynamicProperties, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.InformationWithheld, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.BibliographicCitation, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.CollectionId, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.CollectionCode, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.DataGeneralizations, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.DatasetId, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.DatasetName, IndexSetting.SearchSortAggregate) // WFS
                        .KeywordLowerCase(kwlc => kwlc.InstitutionId, IndexSetting.SearchOnly)
                        .KeywordLowerCase(kwlc => kwlc.Language, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.License, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.OwnerInstitutionCode, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.PrivateCollection, IndexSetting.SearchOnly)
                        .KeywordLowerCase(kwlc => kwlc.PublicCollection, IndexSetting.SearchOnly)
                        .KeywordLowerCase(kwlc => kwlc.References, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.RightsHolder, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.SpeciesCollectionLabel, IndexSetting.SearchOnly)
                        .NumberVal(n => n.DiffusionStatus, IndexSetting.SearchOnly, NumberType.Byte)
                        .BooleanVal(b => b.HasGeneralizedObservationInOtherIndex, IndexSetting.SearchOnly)
                        .BooleanVal(b => b.IsGeneralized, IndexSetting.SearchOnly)
                        .BooleanVal(b => b.Protected, IndexSetting.None)
                        .BooleanVal(b => b.Sensitive, IndexSetting.SearchOnly)
                        .Object<ExtendedMeasurementOrFact>(n => n
                            .Name(nm => nm.MeasurementOrFacts)
                            .Properties(ps => ps
                                .KeywordLowerCase(kwlc => kwlc.MeasurementAccuracy, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementDeterminedBy, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementDeterminedDate, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementID, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementMethod, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementRemarks, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementType, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementTypeID, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementUnit, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementUnitID, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementValue, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.MeasurementValueID, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.OccurrenceID, IndexSetting.None)
                            )
                        )
                        .Object<ProjectsSummary>(t => t
                            .AutoMap()
                            .Name(nm => nm.ProjectsSummary)
                            .Properties(ps => ps
                                .NumberVal(n => n.Project1Id, IndexSetting.SearchOnly, NumberType.Integer) // WFS
                                .NumberVal(n => n.Project2Id, IndexSetting.SearchOnly, NumberType.Integer) // WFS
                                .KeywordLowerCase(kwlc => kwlc.Project1Name, IndexSetting.SearchSortAggregate) // WFS
                                .KeywordLowerCase(kwlc => kwlc.Project1Category, IndexSetting.SearchOnly) // WFS
                                .KeywordLowerCase(kwlc => kwlc.Project1Url, IndexSetting.SearchOnly) // WFS
                                .KeywordLowerCase(kwlc => kwlc.Project1Values, IndexSetting.SearchOnly) // WFS
                                .KeywordLowerCase(kwlc => kwlc.Project2Name, IndexSetting.SearchOnly) // WFS
                                .KeywordLowerCase(kwlc => kwlc.Project2Category, IndexSetting.SearchOnly) // WFS
                                .KeywordLowerCase(kwlc => kwlc.Project2Url, IndexSetting.SearchOnly) // WFS
                                .KeywordLowerCase(kwlc => kwlc.Project2Values, IndexSetting.SearchOnly) // WFS
                            )
                        )
                        .Object<Project>(n => n
                            .AutoMap()
                            .Name(nm => nm.Projects)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.AccessRights)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<SOS.Lib.Models.Processed.DataStewardship.Common.DataStewardshipInfo>(d => d
                            .Name(nm => nm.DataStewardship)
                            .Properties(ps => ps
                                .KeywordLowerCase(kwlc => kwlc.DatasetIdentifier, IndexSetting.SearchSortAggregate)
                                .KeywordLowerCase(kwlc => kwlc.DatasetTitle, IndexSetting.None)
                            )
                        )
                        .Object<ArtportalenInternal>(t => t
                            .AutoMap()
                            .Name(nm => nm.ArtportalenInternal)
                            .Properties(ps => ps                                                                
                                .KeywordLowerCase(kwlc => kwlc.SightingBarcodeURL, IndexSetting.SearchOnly)
                                .KeywordLowerCase(kwlc => kwlc.BirdValidationAreaIds, IndexSetting.SearchOnly)
                                .KeywordLowerCase(kwlc => kwlc.LocationPresentationNameParishRegion, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.ParentLocality, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.ReportedByUserAlias, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.Summary, IndexSetting.None)
                                .BooleanVal(b => b.SecondHandInformation, IndexSetting.SearchOnly)
                                .BooleanVal(b => b.HasAnyTriggeredVerificationRuleWithWarning, IndexSetting.SearchOnly)
                                .BooleanVal(b => b.HasTriggeredVerificationRules, IndexSetting.SearchOnly)
                                .BooleanVal(b => b.HasUserComments, IndexSetting.SearchOnly)
                                .BooleanVal(b => b.IncrementalHarvested, IndexSetting.SearchOnly)
                                .BooleanVal(b => b.NoteOfInterest, IndexSetting.SearchOnly)
                                .NumberVal(n => n.ChecklistId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.FieldDiaryGroupId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.ParentLocationId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.ReportedByUserId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.ReportedByUserServiceUserId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.SightingId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.SightingPublishTypeIds, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.SightingTypeId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.SightingTypeSearchGroupId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.SpeciesFactsIds, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.TriggeredObservationRuleFrequencyId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.TriggeredObservationRuleReproductionId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.TriggeredObservationRuleActivityRuleId, IndexSetting.None, NumberType.Integer)
                                .NumberVal(n => n.TriggeredObservationRulePeriodRuleId, IndexSetting.None, NumberType.Integer)
                                .NumberVal(n => n.TriggeredObservationRulePromptRuleId, IndexSetting.None, NumberType.Integer)
                                .NumberVal(n => n.TriggeredObservationRuleRegionalSightingState, IndexSetting.None, NumberType.Integer)
                                .NumberVal(n => n.TriggeredObservationRuleStatusRuleId, IndexSetting.None, NumberType.Integer)
                                .NumberVal(n => n.ActivityCategoryId, IndexSetting.None, NumberType.Integer)
                                .BooleanVal(b => b.TriggeredObservationRulePrompts, IndexSetting.None)
                                .BooleanVal(b => b.TriggeredObservationRuleUnspontaneous, IndexSetting.None)
                                .NumberVal(n => n.SightingSpeciesCollectionItemId, IndexSetting.None, NumberType.Integer)
                                .NumberVal(n => n.DiffusionId, IndexSetting.None, NumberType.Integer)
                                .NumberVal(n => n.IncludedByLocationId, IndexSetting.None, NumberType.Integer)
                                .Object<UserInternal>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.OccurrenceRecordedByInternal)
                                    .Properties(ps => ps
                                        .KeywordLowerCase(kwlc => kwlc.UserAlias, IndexSetting.None)
                                        .NumberVal(n => n.Id, IndexSetting.SearchOnly, NumberType.Integer)
                                        .NumberVal(n => n.PersonId, IndexSetting.None, NumberType.Integer)
                                        .NumberVal(n => n.UserServiceUserId, IndexSetting.SearchOnly, NumberType.Integer)
                                        .BooleanVal(b => b.Discover, IndexSetting.None)
                                        .BooleanVal(b => b.ViewAccess, IndexSetting.SearchOnly)
                                    )
                                )
                                .Object<UserInternal>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.OccurrenceVerifiedByInternal)
                                    .Properties(ps => ps
                                        .KeywordLowerCase(kwlc => kwlc.UserAlias, IndexSetting.None)
                                        .NumberVal(n => n.Id, IndexSetting.SearchOnly, NumberType.Integer)
                                        .NumberVal(n => n.PersonId, IndexSetting.None, NumberType.Integer)
                                        .NumberVal(n => n.UserServiceUserId, IndexSetting.SearchOnly, NumberType.Integer)
                                        .BooleanVal(b => b.Discover, IndexSetting.None)
                                        .BooleanVal(b => b.ViewAccess, IndexSetting.SearchOnly)
                                    )
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
                                .KeywordLowerCase(kwlc => kwlc.UniqueKey, IndexSetting.None)
                            )
                        )
                        .Object<IDictionary<string, string>>(c => c                            
                            .Name(nm => nm.Defects)
                            .Properties(ps => ps
                                .KeywordLowerCase(kwlc => kwlc.Keys, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.Values, IndexSetting.None)
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
                                .KeywordLowerCase(kwlc => kwlc.Bed, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.EarliestAgeOrLowestStage, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.EarliestEonOrLowestEonothem, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.EarliestEpochOrLowestSeries, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.EarliestEraOrLowestErathem, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.EarliestGeochronologicalEra, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.EarliestPeriodOrLowestSystem, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.Formation, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.GeologicalContextId, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.Group, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.HighestBiostratigraphicZone, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.LatestAgeOrHighestStage, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.LatestEonOrHighestEonothem, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.LatestEpochOrHighestSeries, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.LatestEraOrHighestErathem, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.LatestGeochronologicalEra, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.LatestPeriodOrHighestSystem, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.LithostratigraphicTerms, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.LowestBiostratigraphicZone, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.Member, IndexSetting.None)
                            )
                        )
                        .Object<Identification>(c => c
                            .AutoMap()
                            .Name(nm => nm.Identification)
                            .Properties(ps => ps
                                .KeywordLowerCase(kwlc => kwlc.IdentificationRemarks, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.ConfirmedBy, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.ConfirmedDate, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.DateIdentified, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.IdentificationId, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.IdentificationQualifier, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.IdentificationReferences, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.IdentifiedBy, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.TypeStatus, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.VerifiedBy, IndexSetting.None)
                                .BooleanVal(b => b.UncertainIdentification, IndexSetting.SearchOnly)
                                .BooleanVal(b => b.Validated, IndexSetting.None)
                                .BooleanVal(b => b.Verified, IndexSetting.SearchSortAggregate)
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.DeterminationMethod)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.VerificationStatus)
                                    .Properties(ps => ps.GetMapping())
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.VerificationStatus)
                                    .Properties(ps => ps.GetMapping(valueIndexSetting: IndexSetting.SearchSortAggregate))
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
                                .KeywordLowerCase(kwlc => kwlc.MaterialSampleId, IndexSetting.None)
                            )
                        )
                        .Object<Occurrence>(t => t
                            .AutoMap()
                            .Name(nm => nm.Occurrence)
                            .Properties(ps => ps
                                .Date(d => d
                                    .Name(nm => nm.ReportedDate)
                                )
                                .KeywordLowerCase(kwlc => kwlc.AssociatedMedia, IndexSetting.SearchOnly)
                                .KeywordLowerCase(kwlc => kwlc.AssociatedOccurrences, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.AssociatedReferences, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.AssociatedSequences, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.AssociatedTaxa, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.BiotopeDescription, IndexSetting.None)                                
                                .KeywordLowerCase(kwlc => kwlc.RecordedBy, IndexSetting.SearchSortAggregate)
                                .KeywordLowerCase(kwlc => kwlc.CatalogNumber, IndexSetting.SearchSortAggregate)
                                .KeywordLowerCase(kwlc => kwlc.Disposition, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.IndividualCount, IndexSetting.SearchSortAggregate) // Can we set this to: IndexSetting.None? Printobs2 references this property but should rather use OrganismQuantityInt or OrganismQuantity.
                                .KeywordLowerCase(kwlc => kwlc.OccurrenceId, IndexSetting.SearchSortAggregate)
                                .KeywordLowerCase(kwlc => kwlc.OrganismQuantity, IndexSetting.SearchSortAggregate)
                                .KeywordLowerCase(kwlc => kwlc.OtherCatalogNumbers, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.Preparations, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.RecordNumber, IndexSetting.SearchOnly)
                                .KeywordLowerCase(kwlc => kwlc.ReportedBy, IndexSetting.SearchSortAggregate)
                                .KeywordLowerCase(kwlc => kwlc.Url, IndexSetting.None)
                                .NumberVal(n => n.SensitivityCategory, IndexSetting.SearchSortAggregate, NumberType.Integer)
                                .NumberVal(n => n.BirdNestActivityId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.Length, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.Weight, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.CatalogId, IndexSetting.SearchOnly, NumberType.Integer)
                                .NumberVal(n => n.OrganismQuantityInt, IndexSetting.SearchSortAggregate, NumberType.Integer)
                                .BooleanVal(b => b.IsNaturalOccurrence, IndexSetting.SearchOnly)
                                .BooleanVal(b => b.IsNeverFoundObservation, IndexSetting.SearchOnly)
                                .BooleanVal(b => b.IsNotRediscoveredObservation, IndexSetting.SearchOnly)
                                .BooleanVal(b => b.IsPositiveObservation, IndexSetting.SearchOnly)
                                .Object<Multimedia>(n => n
                                    .AutoMap()
                                    .Name(nm => nm.Media)
                                    .Properties(ps => ps
                                        .KeywordLowerCase(kwlc => kwlc.Description, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Audience, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Contributor, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Created, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Creator, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.DatasetID, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Format, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Identifier, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.License, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Publisher, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.References, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.RightsHolder, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Source, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Title, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Type, IndexSetting.None)
                                        .Object<MultimediaComment>(mc => mc
                                            .AutoMap()
                                            .Name(nm => nm.Comments)
                                            .Properties(ps => ps
                                                .KeywordLowerCase(kwlc => kwlc.Comment, IndexSetting.None)
                                                .KeywordLowerCase(kwlc => kwlc.CommentBy, IndexSetting.None)
                                                .KeywordLowerCase(kwlc => kwlc.Created, IndexSetting.None)
                                            )
                                        )
                                    )
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.OccurrenceStatus)
                                    .Properties(ps => ps.GetMapping(valueIndexSetting: IndexSetting.SearchSortAggregate))
                                )
                                .Object<VocabularyValue>(c => c
                                    .Name(nm => nm.Activity)
                                    .Properties(ps => ps.GetMapping(valueIndexSetting: IndexSetting.SearchSortAggregate))
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
                                    .Properties(ps => ps.GetMapping(valueIndexSetting: IndexSetting.SearchSortAggregate))
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
                                    .Properties(ps => ps.GetMapping(valueIndexSetting: IndexSetting.SearchSortAggregate))
                                )
                                .Object<Substrate>(c => c
                                    .AutoMap()
                                    .Name(nm => nm.Substrate)
                                    .Properties(ps => ps
                                        .KeywordLowerCase(kwlc => kwlc.SpeciesScientificName, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.Description, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.SpeciesDescription, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.SubstrateDescription, IndexSetting.None)
                                        .KeywordLowerCase(kwlc => kwlc.SpeciesVernacularName, IndexSetting.None)
                                        .NumberVal(n => n.Quantity, IndexSetting.None, NumberType.Integer)
                                        .NumberVal(n => n.SpeciesId, IndexSetting.SearchOnly, NumberType.Integer)
                                        .NumberVal(n => n.Id, IndexSetting.SearchOnly, NumberType.Integer)
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
                                .KeywordLowerCase(kwlc => kwlc.AssociatedOrganisms, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.OrganismId, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.OrganismName, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.OrganismRemarks, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.OrganismScope, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.PreviousIdentifications, IndexSetting.None)
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
        /// Make sure Elasticserach nodes are up
        /// </summary>
        /// <param name="clusterCount"></param>
        private void CheckNodes(int clusterCount)
        {
            CheckNode(PublicIndexName, Math.Max(1, clusterCount - 1)); // Subtract 1 since we are using replicas in prod
            CheckNode(ProtectedIndexName, Math.Max(1, clusterCount - 1)); // Subtract 1 since we are using replicas in prod
        }

        /// <summary>
        /// Make sure all clusters are available
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="minClusterCount"></param>
        /// <exception cref="Exception"></exception>
        private void CheckNode(string indexName, int minClusterCount)
        {
            var clusterHealthDictionary = _clusterHealthCache.Get();
            if (clusterHealthDictionary == null)
            {
                clusterHealthDictionary = new ConcurrentDictionary<string, ClusterHealthResponse>();
                _clusterHealthCache.Set(clusterHealthDictionary);
            }

            ClusterHealthResponse health;
            if (clusterHealthDictionary.TryGetValue(indexName, out var clusterHealth))
            {
                health = clusterHealth;
            }
            else
            {
                health = Client.Cluster.Health(indexName);
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

                var latestModified = epoch.AddMilliseconds(res.Aggregations?.Max("latestModified")?.Value ?? 0);
                // If there is incorrect data from the futeure, get data from 5 min back in time
                return latestModified > DateTime.UtcNow ? DateTime.UtcNow.AddMinutes(-5) : latestModified;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to get last modified date for provider: {providerId}, index: {(protectedIndex ? ProtectedIndexName : PublicIndexName)}");
                return DateTime.MinValue;
            }
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

        private async Task<ISearchResponse<dynamic>> PageAggregationItemListAsync(
            string indexName,
            string aggregationFieldKey,
            string aggregationFieldList,
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
                            .Terms(aggregationFieldKey, tt => tt
                                .Field(aggregationFieldKey)
                                .Order(SortOrder.Descending)
                            )
                            .Terms(aggregationFieldList, tt => tt
                                .Field(aggregationFieldList)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="query"></param>
        /// <param name="excludeQuery"></param>
        /// <param name="nextPage"></param>
        /// <param name="take"></param>
        /// <returns></returns>
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
                                .Order(SortOrder.Ascending)
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

        /// <summary>
        /// Write data to elastic search
        /// </summary>
        /// <param name="items"></param>
        /// <param name="protectedIndex"></param>
        /// <param name="refreshIndex"></param>
        /// <returns></returns>
        private BulkAllObserver WriteToElastic(IEnumerable<Observation> items, bool protectedIndex, bool refreshIndex = false)
        {
            if (!items.Any())
            {
                return null;
            }
            var percentageUsed = GetDiskUsage();

            if (percentageUsed > 90)
            {
                Logger.LogError($"Disk usage too high in cluster ({percentageUsed}%), aborting indexing");
                return null;
            }
            Logger.LogDebug($"Current diskusage in cluster: {percentageUsed}%");

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
                    .RefreshOnCompleted(refreshIndex)
                    .DroppedDocumentCallback((r, o) =>
                    {
                        if (r.Error != null)
                        {
                            Logger.LogError("OccurrenceId: {@occurrenceId}, " + $"{r.Error.Reason}", o?.Occurrence?.OccurrenceId);
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
            IClassCache<ConcurrentDictionary<string, ClusterHealthResponse>> clusterHealthCache,
            ILogger<ProcessedObservationCoreRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, logger)
        {
            if (elasticConfiguration.Clusters != null)
            {
                CheckNodes(elasticConfiguration.Clusters.First().Hosts?.Count() ?? 0);
            }
           
            _taxonManager = taxonManager;
        }

        /// <inheritdoc />
        public int AddMany(IEnumerable<Observation> items, bool protectedIndex, bool refreshIndex = false)
        {
            // Save valid processed data
            Logger.LogDebug($"Start indexing batch for searching with {items.Count()} items");
            var indexResult = WriteToElastic(items, protectedIndex, refreshIndex);
            Logger.LogDebug("Finished indexing batch for searching");
            if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
            return items.Count();
        }

        /// <inheritdoc />
        public async Task<int> AddManyAsync(IEnumerable<Observation> items, bool protectedIndex, bool refreshIndex = false)
        {
            return await Task.Run(() =>
            {
                return AddMany(items, protectedIndex, refreshIndex);
            });
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
        public async Task<bool> DeleteAllDocumentsAsync(bool protectedIndex, bool waitForCompletion = false)
        {
            try
            {
                var res = await Client.DeleteByQueryAsync<Observation>(q => q
                    .Index(protectedIndex ? ProtectedIndexName : PublicIndexName)
                    .Query(q => q.MatchAll())
                    .Refresh(waitForCompletion)
                    .WaitForCompletion(waitForCompletion)
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
                p => p.IndexSettings(g => g.RefreshInterval(new Time(5000))));
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
        public async Task<IEnumerable<AggregationItem>> GetAggregationItemsAsync(SearchFilter filter,
            string aggregationField,
            int? precisionThreshold,
            int size = 65536,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            var termsOrder = sortOrder.GetTermsOrder<dynamic>();
            size = Math.Max(1, size);

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
                        .Size(size)
                        .Field(aggregationField)
                        .Order(termsOrder)
                    )
                    .Cardinality("cardinalityAggregation", t => t
                        .Field(aggregationField)
                        .PrecisionThreshold(precisionThreshold ?? 40000)
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

        /// <inheritdoc />
        public async Task<PagedResult<AggregationItem>> GetAggregationItemsAsync(SearchFilter filter,
            string aggregationField,
            int skip,
            int take,
            int? precisionThreshold,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            int size = Math.Max(1, Math.Min(65536, skip + take));
            var termsOrder = sortOrder.GetTermsOrder<dynamic>();

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
                        .Size(size)
                        .Field(aggregationField)
                        .Order(termsOrder)
                    )
                    .Cardinality("cardinalityAggregation", t => t
                        .Field(aggregationField)
                        .PrecisionThreshold(precisionThreshold ?? 40000)
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();
            IEnumerable<AggregationItem> records = searchResponse.Aggregations
                .Terms("termAggregation")
                .Buckets
                .Select(b => new AggregationItem { AggregationKey = b.Key, DocCount = (int)(b.DocCount ?? 0) })
                .Skip(skip)
                .Take(take);
            var totalCount = Convert.ToInt32(searchResponse.Aggregations.Cardinality("cardinalityAggregation").Value);
            var result = new PagedResult<AggregationItem>()
            {
                Records = records,
                Skip = skip,
                Take = take,
                TotalCount = totalCount
            };

            return result;
        }

        public async Task<IEnumerable<AggregationItemOrganismQuantity>> GetAggregationItemsAggregateOrganismQuantityAsync(SearchFilter filter,
            string aggregationField,
            int? precisionThreshold,
            int size = 65536,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            var termsOrder = sortOrder.GetTermsOrder<dynamic>();
            size = Math.Max(1, size);

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
                        .Size(size)
                        .Field(aggregationField)
                        .Order(termsOrder)
                        .Aggregations(ta => ta
                            .Sum("totalOrganismQuantity", sa => sa
                                .Field("occurrence.organismQuantityAggregation")
                            )
                        )
                    )
                    .Cardinality("cardinalityAggregation", t => t
                        .Field(aggregationField)
                        .PrecisionThreshold(precisionThreshold ?? 40000)
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();
            IEnumerable<AggregationItemOrganismQuantity> result = searchResponse.Aggregations
                .Terms("termAggregation")
                .Buckets
                .Select(b => new AggregationItemOrganismQuantity
                {
                    AggregationKey = b.Key,
                    DocCount = (int)(b.DocCount ?? 0),
                    OrganismQuantity = (int)(b.Sum("totalOrganismQuantity")?.Value ?? 0)
                });

            return result;
        }

        /// <inheritdoc />
        public async Task<List<AggregationItem>> GetAllAggregationItemsAsync(SearchFilter filter, string aggregationField)
        {
            var indexName = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            var items = new List<AggregationItem>();
            CompositeKey nextPageKey = null;
            var take = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageAggregationItemAsync(indexName, aggregationField, query, excludeQuery, nextPageKey, take);
                var compositeAgg = searchResponse.Aggregations.Composite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    items.Add(new AggregationItem
                    {
                        AggregationKey = bucket.Key["termAggregation"].ToString(),
                        DocCount = Convert.ToInt32(bucket.DocCount.GetValueOrDefault(0))
                    });
                }

                nextPageKey = compositeAgg.Buckets.Count >= take ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);

            return items;
        }

        /// <inheritdoc />
        public async Task<List<AggregationItemList<TKey, TValue>>> GetAllAggregationItemsListAsync<TKey, TValue>(SearchFilter filter, string aggregationFieldKey, string aggregationFieldList)
        {
            var indexName = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);
            var aggregationDictionary = new Dictionary<TKey, List<TValue>>();
            CompositeKey nextPageKey = null;
            var pageTaxaAsyncTake = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageAggregationItemListAsync(indexName, aggregationFieldKey, aggregationFieldList, query, excludeQuery, nextPageKey, pageTaxaAsyncTake);
                var compositeAgg = searchResponse.Aggregations.Composite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    TKey keyValue = (TKey)bucket.Key[aggregationFieldKey];
                    TValue listValue = (TValue)bucket.Key[aggregationFieldList];
                    if (!aggregationDictionary.ContainsKey(keyValue))
                        aggregationDictionary[keyValue] = new List<TValue>();
                    aggregationDictionary[keyValue].Add(listValue);
                }

                nextPageKey = compositeAgg.Buckets.Count >= pageTaxaAsyncTake ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);

            var items = aggregationDictionary.Select(m => new AggregationItemList<TKey, TValue> { AggregationKey = m.Key, Items = m.Value }).ToList();
            return items;
        }

        /// <inheritdoc />
        public async Task<PagedResult<dynamic>> GetChunkAsync(SearchFilter filter, int skip, int take, bool getAllFields = false)
        {            
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var sortDescriptor = await Client.GetSortDescriptorAsync<Observation>(indexNames, filter?.Output?.SortOrders);
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
                includeRealCount = internalFilter.IncludeRealCount ?? false;
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

            return new PagedResult<dynamic>
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
            var (query, excludeQuery) = GetCoreQueries(filter);
            var occurrencesByEventId = new Dictionary<string, List<string>>();
            CompositeKey nextPageKey = null;
            var take = MaxNrElasticSearchAggregationBuckets;
            do
            {
                var searchResponse = await PageEventOccurrenceItemAsync(indexName, query, excludeQuery, nextPageKey, take);
                var compositeAgg = searchResponse.Aggregations.Composite("compositeAggregation");
                foreach (var bucket in compositeAgg.Buckets)
                {
                    string eventId = bucket.Key["eventId"].ToString();
                    string occurrenceId = bucket.Key["occurrenceId"].ToString();
                    if (!occurrencesByEventId.ContainsKey(eventId))
                        occurrencesByEventId[eventId] = new List<string>();
                    occurrencesByEventId[eventId].Add(occurrenceId);
                }

                nextPageKey = compositeAgg.Buckets.Count >= take ? compositeAgg.AfterKey : null;
            } while (nextPageKey != null);


            var eventIdItems = occurrencesByEventId.Select(m => new EventOccurrenceAggregationItem { EventId = m.Key, OccurrenceIds = m.Value }).ToList();
            return eventIdItems;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<int>> GetProjectIdsNestedAsync(SearchFilter filter)
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
                    .Nested("projects", n => n
                        .Path("projects")
                        .Aggregations(a => a
                            .Terms("projectId", t => t
                                .Field("projects.id")
                            )
                        )
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();
            var result = searchResponse.Aggregations
                .Nested("projects")
                    .Terms("projectId")
                        .Buckets
                            .Select(b => int.Parse(b.Key));

            return result;
        }

        public async Task<IEnumerable<int>> GetProjectIdsAsync(SearchFilter filter)
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
                    .Terms("projectId", t => t
                        .Field("projects.id")
                    )
                )
                .Size(0)
                .Source(s => s.ExcludeAll())
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();
            var result = searchResponse.Aggregations
                .Terms("projectId")
                    .Buckets
                        .Select(b => int.Parse(b.Key));

            return result;
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

        public int GetDiskUsage()
        {
            var currentAllocation = Client.Cat.Allocation();
            if (currentAllocation != null && currentAllocation.IsValid)
            {
                foreach (var record in currentAllocation.Records)
                {
                    if (int.TryParse(record.DiskPercent, out int percentageUsed))
                    {
                        return percentageUsed;
                    }
                }
            }
            return 0;
        }

        /// <inheritdoc />
        public async Task<WaitForStatus> GetHealthStatusAsync(WaitForStatus waitForStatus, int waitForSeconds)
        {
            try
            {
                var response = await Client.Cluster
                        .HealthAsync(new[] { Indices.Index(PublicIndexName), Indices.Index(ProtectedIndexName) }, chr => chr
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
                Logger.LogError(e, "Failed to get ElasticSearch health");
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
        public async Task<long> GetMatchCountAsync(SearchFilterBase filter, bool skipAuthorizationFilters = false)
        {
            var indexNames = GetCurrentIndex(filter, skipAuthorizationFilters);
            var (query, excludeQuery) = GetCoreQueries(filter, skipAuthorizationFilters);

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
        public async Task<GeoGridMetricResult> GetMetricGridAggregationAsync(
            SearchFilter filter,
            int gridCellSizeInMeters,
            MetricCoordinateSys metricCoordinateSys,
            bool skipAuthorizationFilters = false,
            int? maxBuckets = null,
            CompositeKey afterKey = null,
            TimeSpan? timeout = null)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter, skipAuthorizationFilters: skipAuthorizationFilters);

            // Max buckets can't exceed MaxNrElasticSearchAggregationBuckets
            if (maxBuckets.HasValue && maxBuckets.Value > MaxNrElasticSearchAggregationBuckets)
            {
                maxBuckets = MaxNrElasticSearchAggregationBuckets;
            }

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
                        .Size(maxBuckets ?? MaxNrElasticSearchAggregationBuckets + 1)
                        .After(afterKey ?? null)
                        .Sources(s => s
                            .Terms("metric_x", t => t
                                .Script(sct => sct
                                    .Source(
                                        $"(Math.floor(doc['location.{(metricCoordinateSys.Equals(MetricCoordinateSys.ETRS89) || metricCoordinateSys.Equals(MetricCoordinateSys.ETRS89_LAEA_Europe) ? "etrs89X" : "sweref99TmX")}'].value / {gridCellSizeInMeters}) * {gridCellSizeInMeters}).intValue()")
                                )
                            )
                            .Terms("metric_y", t => t
                                .Script(sct => sct
                                    .Source(
                                        $"(Math.floor(doc['location.{(metricCoordinateSys.Equals(MetricCoordinateSys.ETRS89) || metricCoordinateSys.Equals(MetricCoordinateSys.ETRS89_LAEA_Europe) ? "etrs89Y" : "sweref99TmY")}'].value / {gridCellSizeInMeters}) * {gridCellSizeInMeters}).intValue()")
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
                .RequestConfiguration(r => r
                    .RequestTimeout(timeout.GetValueOrDefault(TimeSpan.FromSeconds(_elasticConfiguration.RequestTimeout.GetValueOrDefault(60))))
                )
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

            var gridResult = new GeoGridMetricResult()
            {
                BoundingBox = filter.Location?.Geometries?.BoundingBox,
                GridCellSizeInMeters = gridCellSizeInMeters,
                GridCellCount = nrOfGridCells,
                GridCells = searchResponse.Aggregations.Composite("gridCells").Buckets.Select(b =>
                    new GridCell
                    {
                        MetricBoundingBox = new XYBoundingBox
                        {
                            BottomRight = new XYCoordinate(double.Parse(b.Key["metric_x"].ToString()) + gridCellSizeInMeters, double.Parse(b.Key["metric_y"].ToString())),
                            TopLeft = new XYCoordinate(double.Parse(b.Key["metric_x"].ToString()), double.Parse(b.Key["metric_y"].ToString()) + gridCellSizeInMeters)
                        },
                        ObservationsCount = b.DocCount,
                        TaxaCount = (long?)b.Cardinality("taxa_count").Value
                    }
                ),
                AfterKey = searchResponse.Aggregations.Composite("gridCells").AfterKey
            };

            // When operation is disposed, telemetry item is sent.
            return gridResult;
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
                        .Field("event.eventId")
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

        /// <inheritdoc/>
        public async Task<dynamic> GetObservationAsync(string occurrenceId, SearchFilter filter, bool getAllFields = false)
        {
            var indexNames = GetCurrentIndex(filter);
            var query = filter.ToQuery(skipSightingTypeFilters: true);
            query.TryAddTermCriteria("occurrence.occurrenceId", occurrenceId);

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

            return searchResponse.Documents;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetObservationsAsync(IEnumerable<string> occurrenceIds, bool protectedIndex)
        {
            return await GetObservationsAsync(occurrenceIds, new[] { "occurrence", "location" }, protectedIndex);
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
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<SearchAfterResult<T>> GetObservationsBySearchAfterAsync<T>(
            SearchFilter filter,
            string pointInTimeId = null,
            IEnumerable<object> searchAfter = null)
        {
            var searchIndex = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            // Retry policy by Polly
            var searchResponse = await PollyHelper.GetRetryPolicy(3, 100).ExecuteAsync(async () =>
            {
                var queryResponse = await SearchAfterAsync<dynamic>(searchIndex, new SearchDescriptor<dynamic>()
                .Index(searchIndex)
                .Source(filter.Output?.Fields.ToProjection(filter is SearchFilterInternal))
                    .Query(q => q
                        .Bool(b => b
                            .Filter(query)
                            .MustNot(excludeQuery)
                        )
                    ),
                pointInTimeId,
                searchAfter);

                queryResponse.ThrowIfInvalid();

                return queryResponse;
            });

            return new SearchAfterResult<T>
            {
                Records = (IEnumerable<T>)(typeof(T).Equals(typeof(Observation)) ? searchResponse.Documents?.ToObservationsArray() : searchResponse.Documents),
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

            return new ScrollResult<dynamic>
            {
                Records = searchResponse.Documents,
                ScrollId = searchResponse.Documents.Count < take ? null : searchResponse.ScrollId,
                Take = take,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
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

        public async Task<SearchAfterResult<dynamic>> AggregateByUserFieldAsync(SearchFilter filter,
            string aggregationField,
            bool aggregateOrganismQuantity,
            int? precisionThreshold,
            string? afterKey = null,
            int? take = 10)
        {
            var indexNames = GetCurrentIndex(filter);
            var (query, excludeQuery) = GetCoreQueries(filter);

            var tz = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);            
            var searchResponse = await Client.SearchAsync<dynamic>(s => s
                .Index(indexNames)
                .Query(q => q
                    .Bool(b => b
                        .MustNot(excludeQuery)
                        .Filter(query)
                    )
                )
                .RuntimeFields(rf => rf // Since missing field seems replaced by MissingBucket in Nest implementation, we need to make a script field to handle empty string and null the same way
                    .RuntimeField("scriptField", FieldType.Keyword, s => s
                            .Script(@$"
                            if (!doc['{aggregationField}'].empty){{  
                                String value = '' + doc['{aggregationField}'].value; 
                                if (value != '') {{ 
                                    emit(value); 
                                }} 
                            }}"
                         )
                    )
                )
                .Aggregations(a =>
                {
                    // Lägg till Composite-aggregeringen
                    var compositeAgg = a.Composite("aggregation", c => c
                        .After(string.IsNullOrEmpty(afterKey) ? null : new CompositeKey(new Dictionary<string, object>() { { "termAggregation", afterKey } }))
                        .Size(take)
                        .Sources(s => s
                            .Terms("termAggregation", t => t
                                .Field("scriptField")
                                .MissingBucket(true)
                            )
                        )                        
                        .Aggregations(a => a
                            .Cardinality("unique_taxonids", c => c
                                .Field("taxon.id")
                                .PrecisionThreshold(precisionThreshold ?? 3000)
                            )
                        )
                    );
                    
                    if (aggregateOrganismQuantity)
                    {
                        return a.Composite("aggregation", c => c
                            .After(string.IsNullOrEmpty(afterKey) ? null : new CompositeKey(new Dictionary<string, object>() { { "termAggregation", afterKey } }))
                            .Size(take)
                            .Sources(s => s
                                .Terms("termAggregation", t => t
                                    .Field("scriptField")
                                    .MissingBucket(true)
                                )
                            )
                            .Aggregations(a => a
                                .Cardinality("unique_taxonids", c => c
                                    .Field("taxon.id")
                                    .PrecisionThreshold(precisionThreshold ?? 3000)
                                )
                                .Sum("totalOrganismQuantity", s => s
                                    .Field("occurrence.organismQuantityAggregation")
                                )
                            )
                        );
                    }
                    else
                    {
                        return a.Composite("aggregation", c => c
                            .After(string.IsNullOrEmpty(afterKey) ? null : new CompositeKey(new Dictionary<string, object>() { { "termAggregation", afterKey } }))
                            .Size(take)
                            .Sources(s => s
                                .Terms("termAggregation", t => t
                                    .Field("scriptField")
                                    .MissingBucket(true)
                                )
                            )
                            .Aggregations(a => a
                                .Cardinality("unique_taxonids", c => c
                                    .Field("taxon.id")
                                    .PrecisionThreshold(precisionThreshold ?? 3000)
                                )
                            )
                        );
                    }                    
                })
                .Size(0)
                .Source(s => s.ExcludeAll())
            );

            searchResponse.ThrowIfInvalid();
            afterKey = searchResponse
               .Aggregations
               .Composite("aggregation")
               .AfterKey?.Values.FirstOrDefault()?.ToString()!;

            return new SearchAfterResult<dynamic>
            {
                SearchAfter = new[] { afterKey },
                Records = searchResponse
                    .Aggregations
                    .Composite("aggregation")
                    .Buckets?
                    .Select(b =>
                        new
                        {
                            AggregationField = b.Key.Values.First(),
                            b.DocCount,
                            UniqueTaxon = b.Cardinality("unique_taxonids").Value,
                            OrganismQuantity = aggregateOrganismQuantity ? b.Sum("totalOrganismQuantity")?.Value : 0
                        })?.ToArray()
            };
        }
    }
}
