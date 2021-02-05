using System;
using System.Collections.Generic;
using FluentAssertions;
using MongoDB.Bson;
using Nest;
using SOS.Lib.Models.Search;
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
            var filter = new SearchFilter
            {
                AreaGeographic = new GeographicFilter
                {
                    CountyIds = new[] { "5", "24", "14" },
                    GeometryFilter = new GeometryFilter
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
                },
                EndDate = currentDate
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
            var filter = new SearchFilter
            {
                AreaGeographic = new GeographicFilter
                {
                    CountyIds = new[] { "14", "5", "24" },
                    GeometryFilter = new GeometryFilter
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
                },
                EndDate = currentDate
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var clonedFilter = filter.Clone();
            clonedFilter.TaxonIds = new[] {4000107};

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            clonedFilter.Should().NotBeEquivalentTo(filter);
        }
    }
}