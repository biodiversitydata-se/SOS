using FluentAssertions;
using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Generic;
using Xunit;

namespace SOS.Lib.UnitTests.Models.Search;

public class SearchFilterTests
{
    [Fact]
    public void IsSearchFilterLarge_AccumulatesTaxonIdsAndGeometryPoints()
    {
        var coordinates = Enumerable.Range(0, 170)
            .Select(i => new Coordinate(i, i))
            .Append(new Coordinate(0, 0))
            .ToArray();
        var filter = new SearchFilter
        {
            Taxa = new TaxonFilter { Ids = Enumerable.Range(1, 1_000) },
            Location = new LocationFilter
            {
                Geometries = new GeographicsFilter
                {
                    Geometries = [new Polygon(new LinearRing(coordinates))]
                }
            }
        };

        filter.IsSearchFilterLarge().Should().BeTrue();
    }

    [Fact]
    public void IsSearchFilterLarge_ReturnsFalseForSmallFilter()
    {
        var filter = new SearchFilter
        {
            Taxa = new TaxonFilter { Ids = Enumerable.Range(1, 100) },
            Location = new LocationFilter
            {
                Geometries = new GeographicsFilter
                {
                    MaxDistanceFromPoint = 100,
                    Geometries = [new Point(18, 59)]
                }
            }
        };

        filter.IsSearchFilterLarge().Should().BeFalse();
    }

    [Fact]
    public void IsSearchFilterLarge_UsesTaxonIdCount()
    {
        var filter = new SearchFilter
        {
            Taxa = new TaxonFilter { Ids = Enumerable.Range(1, 1_667).ToList() }
        };

        filter.IsSearchFilterLarge().Should().BeTrue();
    }

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
                        Geometries = new List<Geometry>
                        {
                             new Polygon(new LinearRing([
                                 new Coordinate(1, 10),
                                 new Coordinate(10, 10),
                                 new Coordinate(10, 1),
                                 new Coordinate(1, 1),
                                 new Coordinate(1, 10)
                            ]))
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
                        Geometries = new List<Geometry>
                        {
                             new Polygon(new LinearRing([
                                 new Coordinate(1, 10),
                                 new Coordinate(10, 10),
                                 new Coordinate(10, 1),
                                 new Coordinate(1, 1),
                                 new Coordinate(1, 10)
                            ]))
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
