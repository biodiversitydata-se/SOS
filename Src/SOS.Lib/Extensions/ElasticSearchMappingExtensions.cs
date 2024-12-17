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
                .KeyWordLowerCase(kwlc => kwlc.EventRemarks, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.FieldNumber, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.FieldNotes, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Habitat, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.ParentEventId, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.PlainEndDate, index: true, docValues: false) // WFS
                .KeyWordLowerCase(kwlc => kwlc.PlainEndTime, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.PlainStartDate, index: true, docValues: false) // WFS
                .KeyWordLowerCase(kwlc => kwlc.PlainStartTime, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.SampleSizeUnit, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.SampleSizeValue, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.SamplingEffort, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.SamplingProtocol, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimEventDate, index: false, docValues: false)
                .Nested<Multimedia>(n => n
                    .AutoMap()
                    .Name(nm => nm.Media)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.Description, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Audience, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Contributor, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Created, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Creator, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.DatasetID, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Format, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Identifier, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.License, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Publisher, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.References, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.RightsHolder, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Source, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Title, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Type, index: false, docValues: false)
                    )
                )
                .Nested<ExtendedMeasurementOrFact>(n => n
                    .AutoMap()
                    .Name(nm => nm.MeasurementOrFacts)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.OccurrenceID)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementRemarks, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementAccuracy, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementDeterminedBy, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementDeterminedDate, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementID, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementMethod, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementType, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementTypeID, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementUnit, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementUnitID, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementValue, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.MeasurementValueID, index: false, docValues: false)
                    )
                )
                .Number(x => x
                    .Name(nm => nm.EndDay)
                    .Type(NumberType.Byte)
                )
                .Number(x => x
                    .Name(nm => nm.EndDayOfYear)
                    .Type(NumberType.Short)
                )
                .Number(x => x
                    .Name(nm => nm.EndMonth)
                    .Type(NumberType.Byte)
                )
                .Number(x => x
                    .Name(nm => nm.EndYear)
                    .Type(NumberType.Short)
                )
                .Number(x => x
                    .Name(nm => nm.StartDay)
                    .Type(NumberType.Byte)
                )
                .Number(x => x
                    .Name(nm => nm.StartDayOfYear)
                    .Type(NumberType.Short)
                )
                .Number(x => x
                    .Name(nm => nm.StartMonth)
                    .Type(NumberType.Byte)
                )
                .Number(x => x
                    .Name(nm => nm.StartYear)
                    .Type(NumberType.Short)
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
                .KeyWordLowerCase(kwlc => kwlc.CountryCode, index: true, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.FootprintSRS, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.GeodeticDatum, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferencedBy, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferencedDate, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferenceProtocol, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferenceSources, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferenceVerificationStatus, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.HigherGeography, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.HigherGeographyId, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Island, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.IslandGroup, index: false, docValues: false)
                .Keyword(kw => kw
                    .Name(nm => nm.Locality)
                    .Normalizer("lowercase")
                    .Fields(f => f
                        .Keyword(kw => kw
                            .Name("raw")
                        )
                    )
                )
                .KeyWordLowerCase(kwlc => kwlc.LocationRemarks, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.LocationAccordingTo, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.LocationId)
                .KeyWordLowerCase(kwlc => kwlc.FootprintSpatialFit, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.FootprintWKT, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.GeoreferenceRemarks, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.PointRadiusSpatialFit, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimCoordinates, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimCoordinateSystem, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimDepth, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimElevation, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimLatitude, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimLocality, index: true, docValues: false) // WFS
                .KeyWordLowerCase(kwlc => kwlc.VerbatimLongitude, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimSRS, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.WaterBody, index: false, docValues: false)
                .Object<LocationAttributes>(c => c
                    .Name(nm => nm.Attributes)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.ExternalId, index: true, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.CountyPartIdByCoordinate, index: true, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.ProvincePartIdByCoordinate, index: true, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.VerbatimMunicipality, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.VerbatimProvince, index: false, docValues: false)
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
                    .Name(nm => nm.Atlas10x10)
                    .Properties(ps => ps
                        .Keyword(kw => kw.Name(n => n.FeatureId))
                        .KeyWordLowerCase(kwlc => kwlc.Name)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.Atlas5x5)
                    .Properties(ps => ps
                        .Keyword(kw => kw.Name(n => n.FeatureId))
                        .KeyWordLowerCase(kwlc => kwlc.Name)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.CountryRegion)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.FeatureId)
                        .KeyWordLowerCase(kwlc => kwlc.Name)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.County)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.FeatureId)
                        .KeyWordLowerCase(kwlc => kwlc.Name)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.Municipality)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.FeatureId)
                        .KeyWordLowerCase(kwlc => kwlc.Name)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.Parish)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.FeatureId)
                        .KeyWordLowerCase(kwlc => kwlc.Name)
                    )
                )
                .Object<Area>(c => c
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
                .KeyWordLowerCase(kwlc => kwlc.Category, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.CategorySwedish, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Name, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Owner, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.ProjectURL, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Description, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.SurveyMethod, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.SurveyMethodUrl, index: false, docValues: false)
                .Nested<ProjectParameter>(n => n
                    .AutoMap()
                    .Name(nm => nm.ProjectParameters)
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.DataType, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Name, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Unit, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Description, index: false, docValues: false)
                        .KeyWordLowerCase(kwlc => kwlc.Value, index: false, docValues: false)
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
                .KeyWordLowerCase(kwlc => kwlc.AcceptedNameUsage, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.AcceptedNameUsageId, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Class, index: false, docValues: false)
                .Keyword(kw => kw.Name(n => n.DisplayName).Index(false).DocValues(false))
                .KeyWordLowerCase(kwlc => kwlc.NomenclaturalCode, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.NomenclaturalStatus, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Order, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.TaxonId)
                .KeyWordLowerCase(kwlc => kwlc.TaxonRemarks, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimTaxonRank, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Family, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Genus, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.HigherClassification, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.InfraspecificEpithet, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Kingdom, index: true, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.NameAccordingTo, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.NameAccordingToId, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.NamePublishedIn, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.NamePublishedInId, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.NamePublishedInYear, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.OriginalNameUsage, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.OriginalNameUsageId, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.ParentNameUsage, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.ParentNameUsageId, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Phylum, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.ScientificName, index: true, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.ScientificNameAuthorship, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.ScientificNameId, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.SpecificEpithet, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.Subgenus, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.TaxonConceptId, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.TaxonomicStatus, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.TaxonRank, index: true, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VerbatimId, index: false, docValues: false)
                .KeyWordLowerCase(kwlc => kwlc.VernacularName, index: true, docValues: false)
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
                                .KeyWordLowerCase(kwlc => kwlc.Author, index: false, docValues: false)
                                .KeyWordLowerCase(kwlc => kwlc.Name, index: false, docValues: false)
                                .KeyWordLowerCase(kwlc => kwlc.NomenclaturalStatus, index: false, docValues: false)
                                .KeyWordLowerCase(kwlc => kwlc.TaxonomicStatus, index: false, docValues: false)
                            )
                        )
                        .Nested<TaxonVernacularName>(n => n
                            .Name(nm => nm.VernacularNames)
                            .Properties(ps => ps
                                .Boolean(b => b
                                    .Name(nm => nm.IsPreferredName)
                                    .Index(index: false)
                                    .DocValues(docValues: false)
                                )
                                .KeyWordLowerCase(kwlc => kwlc.CountryCode, index: false, docValues: false)
                                .KeyWordLowerCase(kwlc => kwlc.Name, index: false, docValues: false)
                                .KeyWordLowerCase(kwlc => kwlc.Language, index: false, docValues: false)
                            )
                        )
                        /* .Object<VocabularyValue>(c => c
                             .Name(nm => nm.ProtectionLevel)
                             .Properties(ps => ps.GetMapping())
                         )*/
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


        public static PropertiesDescriptor<T> KeyWordLowerCase<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor, Expression<Func<T, TValue>> objectPath,
            bool? index = true,
            int? ignoreAbove = null,
            bool? docValues = true) where T : class
        {
            return propertiesDescriptor
                .Keyword(kw => kw
                    .DocValues(docValues)
                    .IgnoreAbove(ignoreAbove)
                    .Index(index)
                    .Name(objectPath)
                    .IndexOptions(IndexOptions.Docs)                    
                    .Normalizer("lowercase")
                );
        }
    }
}
