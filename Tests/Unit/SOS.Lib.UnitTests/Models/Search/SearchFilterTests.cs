using System;
using System.Collections.Generic;
using FluentAssertions;
using Nest;
using SOS.Lib.Models.Search.Filters;
using Xunit;

namespace SOS.Lib.UnitTests.Models.Search
{
    public class SearchFilterTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void A_cloned_filter_is_equivalent_to_the_original_filter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var currentDate = DateTime.Now;
            var filter = new SearchFilter(0)
            {
                Location = new LocationFilter
                {
                    AreaGeographic = new GeographicAreasFilter
                    {
                        CountyIds = new List<string> { "5", "24", "14" },
                        GeometryFilter = new GeographicsFilter
                        {
                            MaxDistanceFromPoint = 50,
                            UsePointAccuracy = true,
                            Geometries = new List<IGeoShape>
                            {
                                new PolygonGeoShape(new[]
                                {
                                    new[]
                                    {
                                        new GeoCoordinate(1, 2),
                                        new GeoCoordinate(3, 4)
                                    }
                                })
                            }
                        }
                    }
                },
                Date = new DateFilter { EndDate = currentDate }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clonedFilter = filter.Clone();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            clonedFilter.Should().BeEquivalentTo(filter);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void A_cloned_filter_that_is_modified_is_not_equivalent_to_the_original_filter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var currentDate = DateTime.Now;
            var filter = new SearchFilter(0)
            {
                Location = new LocationFilter
                {
                    AreaGeographic = new GeographicAreasFilter
                    {
                        CountyIds = new List<string> { "14", "5", "24" },
                        GeometryFilter = new GeographicsFilter
                        {
                            MaxDistanceFromPoint = 50,
                            UsePointAccuracy = true,
                            Geometries = new List<IGeoShape>
                            {
                                new PolygonGeoShape(new[]
                                {
                                    new[]
                                    {
                                        new GeoCoordinate(1, 2),
                                        new GeoCoordinate(3, 4)
                                    }
                                })
                            }
                        }
                    }
                },
                Date = new DateFilter { EndDate = currentDate }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clonedFilter = filter.Clone();
            clonedFilter.Taxa = new TaxonFilter
            {
                Ids = new[] { 4000107 }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            clonedFilter.Should().NotBeEquivalentTo(filter);
        }
    }
}