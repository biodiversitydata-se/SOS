using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    public static class SearchExtensions
    {
        private static List<Func<QueryContainerDescriptor<Observation>, QueryContainer>> CreateTypedQuery(FilterBase filter)
        {
            var queryContainers = new List<Func<QueryContainerDescriptor<Observation>, QueryContainer>>();

            if (filter.CountyIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Location.County.Id)
                        .Terms(filter.CountyIds)
                    )
                );
            }

            if (filter.GeometryFilter?.IsValid ?? false)
            {
                foreach (var geom in filter.GeometryFilter.Geometries)
                {
                    switch (geom.Type.ToLower())
                    {
                        case "point":
                            queryContainers.Add(q => q
                                .GeoDistance(gd => gd
                                    .Field(f => f.Location.PointLocation)
                                    .DistanceType(GeoDistanceType.Arc)
                                    .Location(geom.ToGeoLocation())
                                    .Distance(filter.GeometryFilter.MaxDistanceFromPoint ?? 0, DistanceUnit.Meters)
                                    .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                                )
                            );

                            break;
                        case "polygon":
                        case "multipolygon":
                            if (filter.GeometryFilter.UsePointAccuracy)
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field(f => f.Location.PointWithBuffer)
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Intersects)
                                    )
                                );
                            }
                            else
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field(f => f.Location.Point)
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Within)
                                    )
                                );
                            }

                            break;
                    }
                }
            }

            if (filter.EndDate.HasValue)
            {
                queryContainers.Add(q => q
                    .DateRange(r => r
                            .Field(f => f.Event.EndDate)
                            .LessThanOrEquals(
                                DateMath.Anchored(filter.EndDate.Value
                                    .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                    )
                );
            }

            if (filter.GenderIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Occurrence.Gender.Id)
                        .Terms(filter.GenderIds)
                    )
                );
            }

            if (filter.MunicipalityIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Location.Municipality.Id)
                        .Terms(filter.MunicipalityIds)
                    )
                );
            }

            if (filter.OnlyValidated.HasValue && filter.OnlyValidated.Value.Equals(true))
            {
                queryContainers.Add(q => q
                    .Term(m => m
                        .Field(f => f.Identification.Validated)
                        .Value(true)));
            }

            if (filter.PositiveSightings.HasValue)
            {
                queryContainers.Add(q => q
                    .Term(m => m
                        .Field(f => f.Occurrence.IsPositiveObservation)
                        .Value(filter.PositiveSightings.Value)));
            }

            if (filter.DataProviderIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.DataProviderId)
                        .Terms(filter.DataProviderIds)
                    )
                );
            }

            if (filter.ProvinceIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Location.Province.Id)
                        .Terms(filter.ProvinceIds)
                    )
                );
            }

            if (filter.RedListCategories?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Taxon.RedlistCategory)
                        .Terms(filter.RedListCategories)
                    )
                );
            }

            if (filter.StartDate.HasValue)
            {
                queryContainers.Add(q => q
                    .DateRange(r => r
                            .Field(f => f.Event.StartDate)
                            .GreaterThanOrEquals(
                                DateMath.Anchored(filter.StartDate.Value
                                    .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                    )
                );
            }

            if (filter.TaxonIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Taxon.Id)
                        .Terms(filter.TaxonIds)
                    )
                );
            }

            return queryContainers;
        }


        private static List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> CreateQuery(FilterBase filter)
        {
            var queryContainers = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            if (filter.CountyIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("location.county.id")
                        .Terms(filter.CountyIds)
                    )
                );
            }

            if (filter.GeometryFilter?.IsValid ?? false)
            {
                foreach (var geom in filter.GeometryFilter.Geometries)
                {
                    switch (geom.Type.ToLower())
                    {
                        case "point":
                            queryContainers.Add(q => q
                                .GeoDistance(gd => gd
                                    .Field("location.pointLocation")
                                    .DistanceType(GeoDistanceType.Arc)
                                    .Location(geom.ToGeoLocation())
                                    .Distance(filter.GeometryFilter.MaxDistanceFromPoint ?? 0, DistanceUnit.Meters)
                                    .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                                )
                            );

                            break;
                        case "polygon":
                        case "multipolygon":
                            if (filter.GeometryFilter.UsePointAccuracy)
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field("location.pointWithBuffer")
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Intersects)
                                    )
                                );
                            }
                            else
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field("location.point")
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Within)
                                    )
                                );
                            }

                            break;
                    }
                }
            }

            if (filter.GenderIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("occurrence.gender.id")
                        .Terms(filter.GenderIds)
                    )
                );
            }

            if (filter.MunicipalityIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("location.municipality.id")
                        .Terms(filter.MunicipalityIds)
                    )
                );
            }

            if (filter.OnlyValidated.HasValue && filter.OnlyValidated.Value.Equals(true))
            {
                queryContainers.Add(q => q
                    .Term(m => m.Field("identification.validated").Value(true)));
            }

            if (filter.PositiveSightings.HasValue)
            {
                queryContainers.Add(q => q
                    .Term(m => m.Field("occurrence.isPositiveObservation").Value(filter.PositiveSightings.Value)));
            }

            if (filter.DataProviderIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("dataProviderId")
                        .Terms(filter.DataProviderIds)
                    )
                );
            }

            if (filter.ProvinceIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("location.province.id")
                        .Terms(filter.ProvinceIds)
                    )
                );
            }

            if (filter.RedListCategories?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("taxon.redlistCategory")
                        .Terms(filter.RedListCategories.Select(m => m.ToLower()))
                    )
                );
            }

            // If internal filter is "Use Period For All Year" we cannot apply date-range filter.
            if (!(filter is SearchFilterInternal filterInternal && filterInternal.UsePeriodForAllYears))
            {
                if (filter.SearchOnlyBetweenDates)
                {
                    if (filter.StartDate.HasValue)
                    {
                        queryContainers.Add(q => q
                            .DateRange(r => r
                                    .Field("event.startDate")
                                    .GreaterThanOrEquals(
                                        DateMath.Anchored(filter.StartDate.Value
                                            .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                            )
                        );
                    }

                    if (filter.EndDate.HasValue)
                    {
                        queryContainers.Add(q => q
                            .DateRange(r => r
                                    .Field("event.endDate")
                                    .LessThanOrEquals(
                                        DateMath.Anchored(filter.EndDate.Value
                                            .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                            )
                        );
                    }
                }
                else
                {
                    if (filter.EndDate.HasValue)
                    {
                        queryContainers.Add(q => q
                            .DateRange(r => r
                                    .Field("event.startDate")
                                    .LessThanOrEquals(
                                        DateMath.Anchored(filter.EndDate.Value
                                            .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                            )
                        );
                    }

                    if (filter.StartDate.HasValue)
                    {
                        queryContainers.Add(q => q
                            .DateRange(r => r
                                    .Field("event.endDate")
                                    .GreaterThanOrEquals(
                                        DateMath.Anchored(filter.StartDate.Value
                                            .ToUniversalTime())) //.RoundTo(DateMathTimeUnit.Day))
                            )
                        );
                    }
                }
            }

            if (filter.TaxonIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("taxon.id")
                        .Terms(filter.TaxonIds)
                    )
                );
            }

            return queryContainers;
        }


        /// <summary>
        ///     Create project parameter filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToProjectParameterQuery(
            this FilterBase filter)
        {
            var query = CreateQuery(filter);
            query.Add(q => q
                .Nested(n => n
                    .Path("projects")
                    .Query(q => q
                        .Nested(n => n
                            .Path("projects.projectParameters")
                            .Query(q => q
                                .Exists(e => e
                                    .Field("projects.projectParameters")
                                )
                            )
                        )
                    )
                )
            );

            return query;
        }

        public static IEnumerable<Func<QueryContainerDescriptor<Observation>, QueryContainer>> ToMultimediaQuery(
            this FilterBase filter)
        {
            var query = CreateTypedQuery(filter);
            query.Add(q => q
                .Nested(n => n
                    .Path(observation => observation.Media)
                    .Query(nq => nq
                        .Exists(e => e
                            .Field(observation => observation.Media))
                    ))
            );

            return query;
        }

        public static IEnumerable<Func<QueryContainerDescriptor<Observation>, QueryContainer>> ToMeasurementOrFactsQuery(
            this FilterBase filter)
        {
            var query = CreateTypedQuery(filter);
            query.Add(q => q
                .Nested(n => n
                    .Path(observation => observation.MeasurementOrFacts)
                    .Query(nq => nq
                        .Exists(e => e
                            .Field(observation => observation.MeasurementOrFacts))
                    ))
            );

            return query;
        }

        /// <summary>
        ///     Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToQuery(
            this FilterBase filter)
        {
            if (!filter.IsFilterActive)
            {
                return new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();
            }

            return CreateQuery(filter);
        }

        public static IEnumerable<Func<QueryContainerDescriptor<Observation>, QueryContainer>> ToTypedObservationQuery(
            this FilterBase filter)
        {
            if (!filter.IsFilterActive)
            {
                return new List<Func<QueryContainerDescriptor<Observation>, QueryContainer>>();
            }

            return CreateTypedQuery(filter);
        }


        /// <summary>
        ///     Build a projection string
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static Func<SourceFilterDescriptor<dynamic>, ISourceFilter> ToProjection(this IEnumerable<string> fields,
            bool isInternal)
        {
            var projection = new SourceFilterDescriptor<dynamic>();
            if (isInternal)
            {
                projection.Excludes(e => e
                    .Field("defects")
                    .Field("occurrence.sightingTypeSearchGroupId")
                    .Field("location.point")
                    .Field("location.pointLocation")
                    .Field("location.pointWithBuffer")
                    .Field("isInEconomicZoneOfSweden"));
            }
            else
            {
                projection.Excludes(e => e
                    .Field("defects")
                    /*.Field("artportalenInternal.reportedByUserAlias")
                    .Field("artportalenInternal.identifiedByInternal")*/
                    .Field("artportalenInternal")
                    .Field("location.point")
                    .Field("location.pointLocation")
                    .Field("location.pointWithBuffer")
                    .Field("location.parentLocationId")
                    .Field("isInEconomicZoneOfSweden")
                );
            }

            if (fields?.Any() ?? false)
            {
                projection.Includes(i => i
                    .Fields(fields.Select(f => new Field(f)))
                );
            }

            return p => projection;
        }

        /// <summary>
        /// Create a sort descriptor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public static SortDescriptor<dynamic> ToSortDescriptor<T>(this string sortBy, SearchSortOrder sortOrder)
        {
            var sortDescriptor = new SortDescriptor<dynamic>();

            if (!string.IsNullOrEmpty(sortBy))
            {
                // Split sort string 
                var propertyNames = sortBy.Split('.');
                // Create a object of current class
                var parent = Activator.CreateInstance(typeof(T));
                var targetProperty = (PropertyInfo)null;

                // Loop throw all levels in passed sort string
                for (var i = 0; i < propertyNames.Length; i++)
                {
                    // Get property info for current property
                    targetProperty = parent?.GetProperty(propertyNames[i]);

                    // As long it's not the last property, it must be a sub object. Create a instance of it since it's the new parent
                    if (i != propertyNames.Length - 1)
                    {
                        parent = Activator.CreateInstance(targetProperty.GetPropertyType());
                    }
                }

                // Target property found, get it's type
                var propertyType = targetProperty?.GetPropertyType();

                // If it's a string, add keyword in order to make the sorting work
                if (propertyType == typeof(string))
                {
                    //check if the string property already has the keyword attribute, if it does, we do not need the .keyword
                    KeywordAttribute isKeywordAttribute =
                        (KeywordAttribute)Attribute.GetCustomAttribute(targetProperty, typeof(KeywordAttribute));
                    if (isKeywordAttribute == null)
                    {
                        sortBy = $"{sortBy}.keyword";
                    }
                }

                sortDescriptor.Field(sortBy,
                    sortOrder == SearchSortOrder.Desc ? SortOrder.Descending : SortOrder.Ascending);
            }

            return sortDescriptor;
        }
    }
}