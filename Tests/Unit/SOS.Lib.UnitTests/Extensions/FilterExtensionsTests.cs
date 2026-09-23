using FluentAssertions;
using SOS.Lib.Dtos.Enum;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Search.Filters;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class FilterExtensionsTests
{
    [Theory]
    [InlineData(ExtendedFilterDto.SightingTypeFilterDto.DoNotShowMerged, SearchFilterBase.SightingTypeFilter.DoNotShowMerged)]
    [InlineData(ExtendedFilterDto.SightingTypeFilterDto.ShowOnlyMerged, SearchFilterBase.SightingTypeFilter.ShowOnlyMerged)]
    [InlineData(ExtendedFilterDto.SightingTypeFilterDto.ShowBoth, SearchFilterBase.SightingTypeFilter.ShowBoth)]
    [InlineData(ExtendedFilterDto.SightingTypeFilterDto.DoNotShowSightingsInMerged, SearchFilterBase.SightingTypeFilter.DoNotShowSightingsInMerged)]
    [InlineData(ExtendedFilterDto.SightingTypeFilterDto.DoNotShowMergedIncludeReplacementChilds, SearchFilterBase.SightingTypeFilter.DoNotShowMergedIncludeReplacementChilds)]
    [InlineData(ExtendedFilterDto.SightingTypeFilterDto.ShowChildrenAndReplacements, SearchFilterBase.SightingTypeFilter.ShowChildrenAndReplacements)]
    public void ToSearchFilter_MapsArtportalenTypeFilter(
        ExtendedFilterDto.SightingTypeFilterDto dtoValue,
        SearchFilterBase.SightingTypeFilter expected)
    {
        var dto = new SearchFilterDto
        {
            Artportalen = new ArtportalenFilterDto { TypeFilter = dtoValue }
        };

        var filter = dto.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE");

        filter.TypeFilter.Should().Be(expected);
    }

    [Fact]
    public void ToSearchFilter_UsesExistingDefault_WhenTypeFilterIsOmitted()
    {
        var dto = new SearchFilterDto();

        var filter = dto.ToSearchFilter(0, ProtectionFilterDto.Public, "sv-SE");

        filter.TypeFilter.Should().Be(SearchFilterBase.SightingTypeFilter.DoNotShowMerged);
    }

    [Fact]
    public void ToSearchFilterInternal_ExtendedTypeFilterOverridesArtportalenTypeFilter()
    {
        var dto = new SearchFilterInternalDto
        {
            Artportalen = new ArtportalenFilterDto
            {
                TypeFilter = ExtendedFilterDto.SightingTypeFilterDto.ShowOnlyMerged
            },
            ExtendedFilter = new ExtendedFilterDto
            {
                TypeFilter = ExtendedFilterDto.SightingTypeFilterDto.ShowBoth
            }
        };

        var filter = dto.ToSearchFilterInternal(0, "sv-SE");

        filter.TypeFilter.Should().Be(SearchFilterBase.SightingTypeFilter.ShowBoth);
    }
}
