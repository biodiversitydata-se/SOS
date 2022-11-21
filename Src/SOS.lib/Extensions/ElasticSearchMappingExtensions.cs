using Nest;
using SOS.Lib.Models.Processed.Observation;
using System;
using System.Linq.Expressions;

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
                .KeyWordLowerCase(kwlc => kwlc.EventId, true)
                .KeyWordLowerCase(kwlc => kwlc.EventRemarks, false)
                .KeyWordLowerCase(kwlc => kwlc.FieldNumber, false)
                .KeyWordLowerCase(kwlc => kwlc.FieldNotes, false)
                .KeyWordLowerCase(kwlc => kwlc.Habitat, false)
                .KeyWordLowerCase(kwlc => kwlc.ParentEventId, false)
                .KeyWordLowerCase(kwlc => kwlc.PlainEndDate, true) // WFS
                .KeyWordLowerCase(kwlc => kwlc.PlainEndTime, false)
                .KeyWordLowerCase(kwlc => kwlc.PlainStartDate, true) // WFS
                .KeyWordLowerCase(kwlc => kwlc.PlainStartTime, false)
                .KeyWordLowerCase(kwlc => kwlc.SampleSizeUnit, false)
                .KeyWordLowerCase(kwlc => kwlc.SampleSizeValue, false)
                .KeyWordLowerCase(kwlc => kwlc.SamplingEffort, false)
                .KeyWordLowerCase(kwlc => kwlc.SamplingProtocol, false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimEventDate, false)
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
                        .KeyWordLowerCase(kwlc => kwlc.Value)
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
                .KeyWordLowerCase(kwlc => kwlc.CountryCode)
                .KeyWordLowerCase(kwlc => kwlc.FootprintSRS, false)
                .KeyWordLowerCase(kwlc => kwlc.GeodeticDatum, false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferencedBy, false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferencedDate, false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferenceProtocol, false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferenceSources, false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferenceVerificationStatus, false)
                .KeyWordLowerCase(kwlc => kwlc.HigherGeography, false)
                .KeyWordLowerCase(kwlc => kwlc.HigherGeographyId, false)
                .KeyWordLowerCase(kwlc => kwlc.Island, false)
                .KeyWordLowerCase(kwlc => kwlc.IslandGroup, false)
                .Keyword(kw => kw
                    .Name(nm => nm.Locality)
                    .Normalizer("lowercase")
                    .Fields(f => f
                        .Keyword(kw => kw
                            .Name("raw")
                        )
                    ) 
                )
                .KeyWordLowerCase(kwlc => kwlc.LocationRemarks, false)
                .KeyWordLowerCase(kwlc => kwlc.LocationAccordingTo, false)
                .KeyWordLowerCase(kwlc => kwlc.LocationId)
                .KeyWordLowerCase(kwlc => kwlc.FootprintSpatialFit, false)
                .KeyWordLowerCase(kwlc => kwlc.FootprintWKT, false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferenceRemarks, false)
                .KeyWordLowerCase(kwlc => kwlc.PointRadiusSpatialFit, false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimCoordinates, false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimCoordinateSystem, false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimDepth, false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimElevation, false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimLatitude, false)  
                .KeyWordLowerCase(kwlc => kwlc.VerbatimLocality) // WFS
                .KeyWordLowerCase(kwlc => kwlc.VerbatimLongitude, false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimSRS, false)
                .KeyWordLowerCase(kwlc => kwlc.WaterBody, false)
                .Object<LocationAttributes>(c => c
                    .Name(nm => nm.Attributes)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.ExternalId)
                        .KeyWordLowerCase(kwlc => kwlc.CountyPartIdByCoordinate)
                        .KeyWordLowerCase(kwlc => kwlc.ProvincePartIdByCoordinate)
                        .KeyWordLowerCase(kwlc => kwlc.VerbatimMunicipality, false)
                        .KeyWordLowerCase(kwlc => kwlc.VerbatimProvince, false)
                    )
                )
                .Object<VocabularyValue>(c => c
                    .Name(nm => nm.Continent)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.Value, false)
                        .Number(nr => nr
                            .Name(nm => nm.Id)
                            .Type(NumberType.Integer)
                        )
                    )
                )
                .Object<VocabularyValue>(c => c
                    .Name(nm => nm.Country)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.Value, false)
                        .Number(nr => nr
                            .Name(nm => nm.Id)
                            .Type(NumberType.Integer)
                        )
                    )
                )
                .Object<Area>(c => c
                    .AutoMap()
                    .Name(nm => nm.CountryRegion)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.FeatureId)
                        .KeyWordLowerCase(kwlc => kwlc.Name)
                    )
                )
                .Object<Area>(c => c
                    .AutoMap()
                    .Name(nm => nm.County)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.FeatureId)
                        .KeyWordLowerCase(kwlc => kwlc.Name)
                    )
                )
                .Object<Area>(c => c
                    .AutoMap()
                    .Name(nm => nm.Municipality)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.FeatureId)
                        .KeyWordLowerCase(kwlc => kwlc.Name)
                    )
                )
                .Object<Area>(c => c
                    .AutoMap()
                    .Name(nm => nm.Parish)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.FeatureId)
                        .KeyWordLowerCase(kwlc => kwlc.Name)
                    )
                )
                .Object<Area>(c => c
                    .AutoMap()
                    .Name(nm => nm.Province)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.FeatureId)
                        .KeyWordLowerCase(kwlc => kwlc.Name)
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
                .KeyWordLowerCase(kwlc => kwlc.Category, false)
                .KeyWordLowerCase(kwlc => kwlc.CategorySwedish, false)
                .KeyWordLowerCase(kwlc => kwlc.Name, false)
                .KeyWordLowerCase(kwlc => kwlc.Owner, false)
                .KeyWordLowerCase(kwlc => kwlc.ProjectURL, false)
                .KeyWordLowerCase(kwlc => kwlc.Description, false)
                .KeyWordLowerCase(kwlc => kwlc.SurveyMethod, false)
                .KeyWordLowerCase(kwlc => kwlc.SurveyMethodUrl, false)
                .Nested<ProjectParameter>(n => n
                    .AutoMap()
                    .Name(nm => nm.ProjectParameters)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.DataType, false)
                        .KeyWordLowerCase(kwlc => kwlc.Name, false)
                        .KeyWordLowerCase(kwlc => kwlc.Unit, false)
                        .KeyWordLowerCase(kwlc => kwlc.Description, false)
                        .KeyWordLowerCase(kwlc => kwlc.Value, false)
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
                .KeyWordLowerCase(kwlc => kwlc.AcceptedNameUsage, false)
                .KeyWordLowerCase(kwlc => kwlc.AcceptedNameUsageId, false)
                .KeyWordLowerCase(kwlc => kwlc.NomenclaturalCode, false)
                .KeyWordLowerCase(kwlc => kwlc.NomenclaturalStatus, false)
                .KeyWordLowerCase(kwlc => kwlc.Class, false)
                .KeyWordLowerCase(kwlc => kwlc.Order, false)
                .KeyWordLowerCase(kwlc => kwlc.TaxonId)
                .KeyWordLowerCase(kwlc => kwlc.TaxonRemarks, false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimTaxonRank, false)
                .KeyWordLowerCase(kwlc => kwlc.Family, false)
                .KeyWordLowerCase(kwlc => kwlc.Genus, false)
                .KeyWordLowerCase(kwlc => kwlc.HigherClassification, false)
                .KeyWordLowerCase(kwlc => kwlc.InfraspecificEpithet, false)
                .KeyWordLowerCase(kwlc => kwlc.Kingdom)
                .KeyWordLowerCase(kwlc => kwlc.NameAccordingTo, false)
                .KeyWordLowerCase(kwlc => kwlc.NameAccordingToId, false)
                .KeyWordLowerCase(kwlc => kwlc.NamePublishedIn, false)
                .KeyWordLowerCase(kwlc => kwlc.NamePublishedInId, false)
                .KeyWordLowerCase(kwlc => kwlc.NamePublishedInYear, false)
                .KeyWordLowerCase(kwlc => kwlc.OriginalNameUsage, false)
                .KeyWordLowerCase(kwlc => kwlc.OriginalNameUsageId, false)
                .KeyWordLowerCase(kwlc => kwlc.ParentNameUsage, false)
                .KeyWordLowerCase(kwlc => kwlc.ParentNameUsageId, false)
                .KeyWordLowerCase(kwlc => kwlc.Phylum, false)
                .KeyWordLowerCase(kwlc => kwlc.ScientificName)
                .KeyWordLowerCase(kwlc => kwlc.ScientificNameAuthorship, false)
                .KeyWordLowerCase(kwlc => kwlc.ScientificNameId, false)
                .KeyWordLowerCase(kwlc => kwlc.SpecificEpithet, false)
                .KeyWordLowerCase(kwlc => kwlc.Subgenus, false)
                .KeyWordLowerCase(kwlc => kwlc.TaxonConceptId, false)
                .KeyWordLowerCase(kwlc => kwlc.TaxonomicStatus, false)
                .KeyWordLowerCase(kwlc => kwlc.TaxonRank)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimId, false)
                .KeyWordLowerCase(kwlc => kwlc.VernacularName)
                .Object<TaxonAttributes>(c => c
                    .AutoMap()
                    .Name(nm => nm.Attributes)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.ActionPlan)
                        .KeyWordLowerCase(kwlc => kwlc.OrganismGroup)
                        .KeyWordLowerCase(kwlc => kwlc.InvasiveRiskAssessmentCategory)
                        .KeyWordLowerCase(kwlc => kwlc.RedlistCategory)
                        .KeyWordLowerCase(kwlc => kwlc.RedlistCategoryDerived)
                        .KeyWordLowerCase(kwlc => kwlc.SwedishOccurrence)
                        .KeyWordLowerCase(kwlc => kwlc.SwedishHistory)
                        .Nested<TaxonSynonymName>(n => n
                            .Name(nm => nm.Synonyms)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.Author, false)
                                .KeyWordLowerCase(kwlc => kwlc.Name, false)
                                .KeyWordLowerCase(kwlc => kwlc.NomenclaturalStatus, false)
                                .KeyWordLowerCase(kwlc => kwlc.TaxonomicStatus, false)
                            )
                        )
                        .Nested<TaxonVernacularName>(n => n
                            .Name(nm => nm.VernacularNames)
                            .Properties(ps => ps
                                .Boolean(b => b
                                    .Name(nm => nm.IsPreferredName)
                                    .Index(false)
                                )
                                .KeyWordLowerCase(kwlc => kwlc.CountryCode, false)
                                .KeyWordLowerCase(kwlc => kwlc.Name, false)
                                .KeyWordLowerCase(kwlc => kwlc.Language, false)
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
                .KeyWordLowerCase(kwlc => kwlc.Value, false)
                .Number(nr => nr
                    .Name(nm => nm.Id)
                    .Type(NumberType.Integer)
                );
        }


        public static PropertiesDescriptor<T> KeyWordLowerCase<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor, Expression<Func<T, TValue>> objectPath, bool? index = true, int? ignoreAbove = null) where T : class
        {
            return propertiesDescriptor
                .Keyword(kw => kw
                    .Name(objectPath)
                    .Normalizer("lowercase")
                    .Index(index)
                    .IgnoreAbove(ignoreAbove)
                );
        }
    }
}
