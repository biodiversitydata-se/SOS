using Nest;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Extensions
{
    internal static class ElasticSearchMappingExtensions
    {
        /// <summary>
        /// Get Event mapping
        /// </summary>
        /// <param name="propertiesDescriptor"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<Event> GetMapping(this PropertiesDescriptor<Event> propertiesDescriptor)
        {
            return propertiesDescriptor
                .Date(d => d
                    .Name(nm => nm.EndDate)
                )
                .Date(d => d
                    .Name(nm => nm.StartDate)
                )
                .Keyword(kw => kw
                    .Name(nm => nm.EventId)
                    .IgnoreAbove(int.MaxValue)
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
                    .Index(false)
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
                );
        }

        /// <summary>
        /// Get location mapping
        /// </summary>
        /// <param name="propertiesDescriptor"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<Location> GetMapping(this PropertiesDescriptor<Location> propertiesDescriptor)
        {
            return propertiesDescriptor
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
                    .Name(nm => nm.Locality)
                    .Normalizer("lowercase")
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
                    .Index(true) // WFS
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
                );
        }

        /// <summary>
        /// Get project mapping
        /// </summary>
        /// <param name="propertiesDescriptor"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<Project> GetMapping(this PropertiesDescriptor<Project> propertiesDescriptor)
        {
            return propertiesDescriptor
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
                );
        }

        /// <summary>
        /// Get Taxon mapping
        /// </summary>
        /// <param name="propertiesDescriptor"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<Taxon> GetMapping(this PropertiesDescriptor<Taxon> propertiesDescriptor)
        {
            return propertiesDescriptor
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
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.SensitivityCategory)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<VocabularyValue>(c => c
                            .Name(nm => nm.TaxonCategory)
                            .Properties(ps => ps.GetMapping())
                        )
                    )
                );
        }

        /// <summary>
        /// Get Vocabular mapping
        /// </summary>
        /// <param name="propertiesDescriptor"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<VocabularyValue> GetMapping(this PropertiesDescriptor<VocabularyValue> propertiesDescriptor)
        {
            return propertiesDescriptor
                .Keyword(kw => kw
                    .Name(nm => nm.Value)
                    .Index(false)
                )
                .Number(nr => nr
                    .Name(nm => nm.Id)
                    .Type(NumberType.Integer)
                );
        }
    }
}
