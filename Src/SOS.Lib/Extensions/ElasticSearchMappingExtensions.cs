using Elastic.Clients.Elasticsearch.Mapping;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SOS.Lib.Extensions
{
    internal static class ElasticSearchMappingExtensions
    {
        /// <summary>
        /// Get index settings
        /// </summary>
        /// <param name="indexSetting"></param>
        /// <returns></returns>
        private static (bool IndexForSearch, bool IndexForSortAndAggregate) GetIndexSettings(IndexSetting indexSetting)
        {
            return indexSetting switch
            {
                IndexSetting.None => (false, false),
                IndexSetting.SearchOnly => (true, false),
                _ => (true, true)
            };
        }

        /// <summary>
        /// Get Event mapping
        /// </summary>
        /// <param name="propertiesDescriptor"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<Event> GetMapping(this PropertiesDescriptor<Event> propertiesDescriptor)
        {
            return propertiesDescriptor
                .Date(d => d.EndDate, c => c.Index(true).DocValues(true))
                .Date(d => d.StartDate, c => c.Index(true).DocValues(true))
                .ShortNumber(s => s.EndDayOfYear, c => c.Index(true).DocValues(true))
                .ShortNumber(s => s.StartDayOfYear, c => c.Index(true).DocValues(true))
                .ShortNumber(s => s.StartDay, c => c.Index(true))
                .ShortNumber(s => s.EndDay, c => c.Index(true))
                .ByteNumber(s => s.StartMonth, c => c.Index(true).DocValues(true))
                .ByteNumber(s => s.EndMonth, c => c.Index(true).DocValues(true))
                .ByteNumber(s => s.StartHistogramWeek, c => c.Index(true).DocValues(true))
                .ByteNumber(s => s.EndHistogramWeek, c => c.Index(true).DocValues(true))
                .ShortNumber(s => s.StartYear, c => c.Index(true).DocValues(true))
                .ShortNumber(s => s.EndYear, c => c.Index(true).DocValues(true))
                .KeywordLowerCase(kwlc => kwlc.EventId, IndexSetting.SearchSortAggregate)
                .KeywordLowerCase(kwlc => kwlc.EventRemarks, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.FieldNumber, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.FieldNotes, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Habitat, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.ParentEventId, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.PlainEndDate, IndexSetting.SearchOnly) // WFS
                .KeywordLowerCase(kwlc => kwlc.PlainEndTime, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.PlainStartDate, IndexSetting.SearchOnly) // WFS
                .KeywordLowerCase(kwlc => kwlc.PlainStartTime, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.SampleSizeUnit, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.SampleSizeValue, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.SamplingEffort, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.SamplingProtocol, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VerbatimEventDate, IndexSetting.None)
                .Object(o => o.Media.FirstOrDefault(), c => c
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
                .Object<ExtendedMeasurementOrFact>(n => n
                    .AutoMap()
                    .Name(nm => nm.MeasurementOrFacts)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.OccurrenceID, IndexSetting.SearchSortAggregate)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementRemarks, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementAccuracy, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementDeterminedBy, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementDeterminedDate, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementID, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementMethod, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementType, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementTypeID, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementUnit, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementUnitID, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementValue, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.MeasurementValueID, IndexSetting.None)
                    )
                )
                .Object<Weather>(n => n
                    .AutoMap()
                    .Name(nm => nm.Weather)
                    .Properties(ps => ps
                        .NumberVal(kwlc => kwlc.SnowCover, IndexSetting.None, NumberType.Byte)
                        .NumberVal(kwlc => kwlc.WindDirection, IndexSetting.None, NumberType.Byte)
                        .NumberVal(kwlc => kwlc.WindStrength, IndexSetting.None, NumberType.Byte)
                        .NumberVal(kwlc => kwlc.Precipitation, IndexSetting.None, NumberType.Byte)
                        .NumberVal(kwlc => kwlc.Visibility, IndexSetting.None, NumberType.Byte)
                        .NumberVal(kwlc => kwlc.Cloudiness, IndexSetting.None, NumberType.Byte)
                        .Object<Measuring>(n => n
                            .AutoMap()
                            .Name(nm => nm.Sunshine)
                            .Properties(ps => ps
                                .NumberVal(n => n.Value, IndexSetting.None, NumberType.Double)
                                .NumberVal(n => n.Unit, IndexSetting.None, NumberType.Byte)
                            )
                        )
                        .Object<Measuring>(n => n
                            .AutoMap()
                            .Name(nm => nm.AirTemperature)
                            .Properties(ps => ps
                                .NumberVal(n => n.Value, IndexSetting.None, NumberType.Double)
                                .NumberVal(n => n.Unit, IndexSetting.None, NumberType.Byte)
                            )
                        )
                        .Object<Measuring>(n => n
                            .AutoMap()
                            .Name(nm => nm.WindDirectionDegrees)
                            .Properties(ps => ps
                                .NumberVal(n => n.Value, IndexSetting.None, NumberType.Double)
                                .NumberVal(n => n.Unit, IndexSetting.None, NumberType.Byte)
                            )
                        )
                        .Object<Measuring>(n => n
                            .AutoMap()
                            .Name(nm => nm.WindSpeed)
                            .Properties(ps => ps
                                .NumberVal(n => n.Value, IndexSetting.None, NumberType.Double)
                                .NumberVal(n => n.Unit, IndexSetting.None, NumberType.Byte)
                            )
                        )
                    )
                )                
                .Object<VocabularyValue>(t => t
                    .Name(nm => nm.DiscoveryMethod)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.Value, IndexSetting.SearchOnly)
                        .NumberVal(n => n.Id, IndexSetting.SearchSortAggregate, NumberType.Short)                            
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
                .NumberVal(n => n.DecimalLongitude, IndexSetting.SearchSortAggregate, NumberType.Double)
                .NumberVal(n => n.DecimalLatitude, IndexSetting.SearchSortAggregate, NumberType.Double)
                .NumberVal(n => n.CoordinateUncertaintyInMeters, IndexSetting.SearchSortAggregate, NumberType.Integer)
                .NumberVal(n => n.Type, IndexSetting.None, NumberType.Byte)
                .NumberVal(n => n.CoordinatePrecision, IndexSetting.None, NumberType.Double)
                .NumberVal(n => n.MaximumDepthInMeters, IndexSetting.None, NumberType.Double)
                .NumberVal(n => n.MaximumDistanceAboveSurfaceInMeters, IndexSetting.None, NumberType.Double)
                .NumberVal(n => n.MaximumElevationInMeters, IndexSetting.None, NumberType.Double)
                .NumberVal(n => n.MinimumDepthInMeters, IndexSetting.None, NumberType.Double)
                .NumberVal(n => n.MinimumDistanceAboveSurfaceInMeters, IndexSetting.None, NumberType.Double)
                .NumberVal(n => n.MinimumElevationInMeters, IndexSetting.None, NumberType.Double)                
                .BooleanVal(b => b.IsInEconomicZoneOfSweden, IndexSetting.SearchOnly)                
                .KeywordLowerCase(kwlc => kwlc.LocationId, IndexSetting.SearchSortAggregate)
                .KeywordLowerCase(kwlc => kwlc.CountryCode, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.FootprintSRS, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.GeodeticDatum, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.GeoreferencedBy, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.GeoreferencedDate, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.GeoreferenceProtocol, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.GeoreferenceSources, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.GeoreferenceVerificationStatus, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.HigherGeography, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.HigherGeographyId, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Island, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.IslandGroup, IndexSetting.None)
                .Keyword(kw => kw
                    .Name(nm => nm.Locality)
                    .Normalizer("lowercase")
                    .DocValues(true)
                    .Fields(f => f
                        .Keyword(kw => kw
                            .Name("raw")
                            .DocValues(false)
                        )
                    )
                )
                .KeywordLowerCase(kwlc => kwlc.LocationRemarks, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.LocationAccordingTo, IndexSetting.None)                
                .KeywordLowerCase(kwlc => kwlc.FootprintSpatialFit, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.FootprintWKT, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.GeoreferenceRemarks, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.PointRadiusSpatialFit, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VerbatimCoordinates, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VerbatimCoordinateSystem, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VerbatimDepth, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VerbatimElevation, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VerbatimLatitude, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VerbatimLocality, IndexSetting.SearchOnly) // WFS
                .KeywordLowerCase(kwlc => kwlc.VerbatimLongitude, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VerbatimSRS, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.WaterBody, IndexSetting.None)
                .Object<LocationAttributes>(c => c
                    .Name(nm => nm.Attributes)
                    .Properties(ps => ps
                        .BooleanVal(b => b.IsPrivate, IndexSetting.None)
                        .NumberVal(n => n.ProjectId, IndexSetting.SearchOnly, NumberType.Integer)
                        .KeywordLowerCase(kwlc => kwlc.ExternalId, IndexSetting.SearchOnly)
                        .KeywordLowerCase(kwlc => kwlc.CountyPartIdByCoordinate, IndexSetting.SearchOnly)
                        .KeywordLowerCase(kwlc => kwlc.ProvincePartIdByCoordinate, IndexSetting.SearchOnly)
                        .KeywordLowerCase(kwlc => kwlc.VerbatimMunicipality, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.VerbatimProvince, IndexSetting.None)
                    )
                )
                .Object<VocabularyValue>(c => c
                    .Name(nm => nm.Continent)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.Value, IndexSetting.None)
                        .NumberVal(nr => nr.Id, IndexSetting.None, NumberType.Byte)
                    )
                )
                .Object<VocabularyValue>(c => c
                    .Name(nm => nm.Country)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.Value, IndexSetting.SearchOnly)
                        .NumberVal(nr => nr.Id, IndexSetting.SearchSortAggregate, NumberType.Byte)
                    )
                )                
                .Object<Area>(c => c
                    .Name(nm => nm.Atlas10x10)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.FeatureId, IndexSetting.SearchSortAggregate)
                        .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.None)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.Atlas5x5)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.FeatureId, IndexSetting.SearchSortAggregate)
                        .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.None)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.CountryRegion)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.FeatureId, IndexSetting.SearchSortAggregate)
                        .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.SearchOnly)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.County)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.FeatureId, IndexSetting.SearchSortAggregate)
                        .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.SearchSortAggregate)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.Municipality)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.FeatureId, IndexSetting.SearchSortAggregate)
                        .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.SearchSortAggregate)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.Parish)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.FeatureId, IndexSetting.SearchSortAggregate)
                        .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.SearchSortAggregate)
                    )
                )
                .Object<Area>(c => c
                    .Name(nm => nm.Province)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.FeatureId, IndexSetting.SearchSortAggregate)
                        .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.SearchSortAggregate)
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
                .KeywordLowerCase(kwlc => kwlc.Category, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.CategorySwedish, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Owner, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.ProjectURL, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Description, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.SurveyMethod, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.SurveyMethodUrl, IndexSetting.None)
                .DateVal(d => d.StartDate, IndexSetting.None)
                .DateVal(d => d.EndDate, IndexSetting.None)
                .BooleanVal(b => b.IsPublic, IndexSetting.None)
                .Object(o => o.ProjectParameters, p => p.Properties(p => p
                    .KeywordLowerCase(kwlc => kwlc.ProjectParameters.First().DataType, IndexSetting.None)
                    .KeywordLowerCase(kwlc => kwlc.ProjectParameters.First().Name, IndexSetting.None)
                    .KeywordLowerCase(kwlc => kwlc.ProjectParameters.First().Unit, IndexSetting.None)
                    .KeywordLowerCase(kwlc => kwlc.ProjectParameters.First().Description, IndexSetting.None)
                    .KeywordLowerCase(kwlc => kwlc.ProjectParameters.First().Value, IndexSetting.None)
                )
                       
                 ))
                .Object<ProjectParameter>(n => n
                    .AutoMap()
                    .Name(nm => nm.ProjectParameters)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.DataType, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.Unit, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.Description, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.Value, IndexSetting.None)
                    )
                );
        }

        /// <summary>
        /// Get mapping for project parameters
        /// </summary>
        /// <param name="propertiesDescriptor"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<ProjectParameter> GetMapping(this PropertiesDescriptor<ProjectParameter> propertiesDescriptor)
        {
            return propertiesDescriptor
                .KeywordLowerCase(kwlc => kwlc.DataType, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Unit, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Description, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Value, IndexSetting.None);
        }

        /// <summary>
        /// Get Taxon mapping
        /// </summary>
        /// <param name="propertiesDescriptor"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<Taxon> GetMapping(this PropertiesDescriptor<Taxon> propertiesDescriptor)
        {
            return propertiesDescriptor
                .KeywordLowerCase(kwlc => kwlc.AcceptedNameUsage, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.AcceptedNameUsageId, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Class, IndexSetting.None)
                .Keyword(kw => kw.Name(n => n.DisplayName).Index(false).DocValues(false))
                .KeywordLowerCase(kwlc => kwlc.NomenclaturalCode, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.NomenclaturalStatus, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Order, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.TaxonId, IndexSetting.SearchSortAggregate)
                .KeywordLowerCase(kwlc => kwlc.TaxonRemarks, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VerbatimTaxonRank, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Family, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Genus, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.HigherClassification, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.InfraspecificEpithet, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Kingdom, IndexSetting.SearchOnly)
                .KeywordLowerCase(kwlc => kwlc.NameAccordingTo, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.NameAccordingToId, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.NamePublishedIn, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.NamePublishedInId, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.NamePublishedInYear, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.OriginalNameUsage, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.OriginalNameUsageId, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.ParentNameUsage, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.ParentNameUsageId, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Phylum, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.ScientificName, IndexSetting.SearchSortAggregate)
                .KeywordLowerCase(kwlc => kwlc.ScientificNameAuthorship, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.ScientificNameId, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.SpecificEpithet, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.Subgenus, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.TaxonConceptId, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.TaxonomicStatus, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.TaxonRank, IndexSetting.SearchOnly)
                .KeywordLowerCase(kwlc => kwlc.VerbatimId, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VerbatimName, IndexSetting.None)
                .KeywordLowerCase(kwlc => kwlc.VernacularName, IndexSetting.SearchSortAggregate)                
                .NumberVal(n => n.Id, IndexSetting.SearchSortAggregate, NumberType.Integer)
                .NumberVal(n => n.SecondaryParentDyntaxaTaxonIds, IndexSetting.None, NumberType.Integer)
                .BooleanVal(b => b.BirdDirective, IndexSetting.SearchOnly)
                .Object<TaxonAttributes>(c => c
                    .AutoMap()
                    .Name(nm => nm.Attributes)
                    .Properties(ps => ps
                        .KeywordLowerCase(kwlc => kwlc.ActionPlan, IndexSetting.SearchOnly) // WFS
                        .KeywordLowerCase(kwlc => kwlc.OrganismGroup, IndexSetting.SearchSortAggregate) // WFS
                        .KeywordLowerCase(kwlc => kwlc.InvasiveRiskAssessmentCategory, IndexSetting.SearchOnly) // WFS
                        .KeywordLowerCase(kwlc => kwlc.RedlistCategory, IndexSetting.SearchSortAggregate) // WFS
                        .KeywordLowerCase(kwlc => kwlc.RedlistCategoryDerived, IndexSetting.SearchSortAggregate)
                        .KeywordLowerCase(kwlc => kwlc.SwedishOccurrence, IndexSetting.None)
                        .KeywordLowerCase(kwlc => kwlc.SwedishHistory, IndexSetting.None)
                        .NumberVal(n => n.DyntaxaTaxonId, IndexSetting.None, NumberType.Integer)
                        .NumberVal(n => n.GbifTaxonId, IndexSetting.None, NumberType.Integer)
                        .NumberVal(n => n.ParentDyntaxaTaxonId, IndexSetting.None, NumberType.Integer)
                        .NumberVal(n => n.DisturbanceRadius, IndexSetting.None, NumberType.Integer)
                        .NumberVal(n => n.SortOrder, IndexSetting.SearchSortAggregate, NumberType.Integer)
                        .NumberVal(n => n.SpeciesGroup, IndexSetting.None, NumberType.Byte)
                        .BooleanVal(n => n.IsRedlisted, IndexSetting.SearchOnly) // WFS
                        .BooleanVal(n => n.IsInvasiveInSweden, IndexSetting.SearchOnly) // WFS
                        .BooleanVal(n => n.IsInvasiveAccordingToEuRegulation, IndexSetting.SearchOnly) // WFS
                        .BooleanVal(n => n.Natura2000HabitatsDirectiveArticle2, IndexSetting.SearchOnly) // WFS
                        .BooleanVal(n => n.Natura2000HabitatsDirectiveArticle4, IndexSetting.SearchOnly) // WFS
                        .BooleanVal(n => n.Natura2000HabitatsDirectiveArticle5, IndexSetting.SearchOnly) // WFS
                        .BooleanVal(n => n.ProtectedByLaw, IndexSetting.SearchOnly) // WFS                        
                        .Object<VocabularyValue>(t => t
                            .Name(nm => nm.ProtectionLevel)
                            .Properties(ps => ps
                                .KeywordLowerCase(kwlc => kwlc.Value, IndexSetting.None)
                                .NumberVal(n => n.Id, IndexSetting.None, NumberType.Byte)
                            )
                        )
                        .Object<VocabularyValue>(t => t
                            .Name(nm => nm.SensitivityCategory)
                            .Properties(ps => ps
                                .KeywordLowerCase(kwlc => kwlc.Value, IndexSetting.SearchOnly)
                                .NumberVal(n => n.Id, IndexSetting.SearchSortAggregate, NumberType.Byte)
                            )
                        )
                        .Object<TaxonSynonymName>(n => n
                            .Name(nm => nm.Synonyms)
                            .Properties(ps => ps
                                .KeywordLowerCase(kwlc => kwlc.Author, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.NomenclaturalStatus, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.TaxonomicStatus, IndexSetting.None)
                            )
                        )
                        .Object<TaxonScientificName>(n => n
                            .Name(nm => nm.ScientificNames)
                            .Properties(ps => ps
                                .KeywordLowerCase(kwlc => kwlc.Author, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.None)
                                .BooleanVal(b => b.IsPreferredName, IndexSetting.None)
                                .BooleanVal(b => b.ValidForSighting, IndexSetting.None)
                            )
                        )
                        .Object<TaxonVernacularName>(n => n
                            .Name(nm => nm.VernacularNames)
                            .Properties(ps => ps
                                .Boolean(b => b
                                    .Name(nm => nm.IsPreferredName)
                                    .Index(index: false)
                                    .DocValues(docValues: false)
                                )
                                .KeywordLowerCase(kwlc => kwlc.CountryCode, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.Name, IndexSetting.None)
                                .KeywordLowerCase(kwlc => kwlc.Language, IndexSetting.None)
                                .BooleanVal(b => b.ValidForSighting, IndexSetting.None)
                            )
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
        /// Map boolean property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="propertiesDescriptor"></param>
        /// <param name="propertyName"></param>
        /// <param name="indexSetting"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<T> BooleanVal<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor, Expression<Func<T, TValue>> propertyName,
            IndexSetting indexSetting = IndexSetting.SearchSortAggregate) where T : class
        {
            var indexSettings = GetIndexSettings(indexSetting);
            return propertiesDescriptor
                .Boolean(propertyName, b => b
                    .Index(indexSettings.IndexForSearch)
                    .DocValues(indexSettings.IndexForSortAndAggregate)
                );
        }

        /// <summary>
        /// Map date property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="propertiesDescriptor"></param>
        /// <param name="propertyName"></param>
        /// <param name="indexSetting"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<T> DateVal<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor, Expression<Func<T, TValue>> propertyName,
            IndexSetting indexSetting = IndexSetting.SearchSortAggregate) where T : class
        {
            var indexSettings = GetIndexSettings(indexSetting);
            return propertiesDescriptor
                .Date(propertyName, n => n
                    .Index(indexSettings.IndexForSearch)
                    .DocValues(indexSettings.IndexForSortAndAggregate)
                );
        }

        /// <summary>
        /// Map keword property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="propertiesDescriptor"></param>
        /// <param name="propertyName"></param>
        /// <param name="indexSetting"></param>
        /// <param name="normalizer"></param>
        /// <param name="ignoreAbove"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<T> KeywordVal<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor, Expression<Func<T, TValue>> propertyName,
            IndexSetting indexSetting = IndexSetting.SearchSortAggregate,
            Normalizer? normalizer = Normalizer.None,
            int? ignoreAbove = null) where T : class
        {
            var indexSettings = GetIndexSettings(indexSetting);
            return propertiesDescriptor
                .Keyword(propertyName, c => c
                    .DocValues(indexSettings.IndexForSortAndAggregate)
                    .IgnoreAbove(ignoreAbove)
                    .Index(indexSettings.IndexForSearch)
                    .IndexOptions(IndexOptions.Docs)
                    .Normalizer(normalizer switch { Normalizer.LowerCase => "lowercase", _ => null })
                );
        }

        /// <summary>
        /// Map number property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="propertiesDescriptor"></param>
        /// <param name="propertyName"></param>
        /// <param name="indexSetting"></param>
        /// <param name="numberType"></param>
        /// <returns></returns>
        public static PropertiesDescriptor<T> NumberVal<T, TValue>(this PropertiesDescriptor<T> propertiesDescriptor, Expression<Func<T, TValue>> propertyName,
            IndexSetting indexSetting,
            NumberType numberType) where T : class
        {
           var indexSettings = GetIndexSettings(indexSetting);
            return numberType switch {
                NumberType.Byte => propertiesDescriptor
                    .ByteNumber(propertyName, n => n
                        .Index(indexSettings.IndexForSearch)
                        .DocValues(indexSettings.IndexForSortAndAggregate)
                    ),
                NumberType.Double => propertiesDescriptor
                    .DoubleNumber(propertyName, n => n
                        .Index(indexSettings.IndexForSearch)
                        .DocValues(indexSettings.IndexForSortAndAggregate)
                    ),
                NumberType.Float => propertiesDescriptor
                    .FloatNumber(propertyName, n => n
                        .Index(indexSettings.IndexForSearch)
                        .DocValues(indexSettings.IndexForSortAndAggregate)
                    ),
                NumberType.Long => propertiesDescriptor
                    .LongNumber(propertyName, n => n
                        .Index(indexSettings.IndexForSearch)
                        .DocValues(indexSettings.IndexForSortAndAggregate)
                    ),
                NumberType.Short => propertiesDescriptor
                    .ShortNumber(propertyName, n => n
                        .Index(indexSettings.IndexForSearch)
                        .DocValues(indexSettings.IndexForSortAndAggregate)
                    ),
                _ => propertiesDescriptor
                    .IntegerNumber(propertyName, n => n
                        .Index(indexSettings.IndexForSearch)
                        .DocValues(indexSettings.IndexForSortAndAggregate)
                    )
            };
        }
    }
}
