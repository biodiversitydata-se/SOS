using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search.Result;
using SOS.Shared.Api.Dtos;
using System.Text.Json.Nodes;


namespace SOS.Shared.Api.Extensions.Dto;

public static class ResultExtensions
{        
    public static GeoGridResultDto ToGeoGridResultDto(this GeoGridTileResult geoGridTileResult, long totalGridCellCount)
    {
        return new GeoGridResultDto
        {
            Zoom = geoGridTileResult.Zoom,
            GridCellCount = geoGridTileResult.GridCellTileCount,
            BoundingBox = geoGridTileResult.BoundingBox.ToLatLonBoundingBoxDto(),
            GridCells = geoGridTileResult.GridCellTiles.Select(cell => cell.ToGeoGridCellDto()),
            TotalGridCellCount = geoGridTileResult.GridCellTileCount > totalGridCellCount ? geoGridTileResult.GridCellTileCount : totalGridCellCount
        };
    }

    public static GeoGridMetricResultDto ToDto(this GeoGridMetricResult geoGridMetricResult)
    {
        return new GeoGridMetricResultDto
        {
            BoundingBox = geoGridMetricResult.BoundingBox.ToLatLonBoundingBoxDto(),
            GridCellCount = geoGridMetricResult.GridCellCount,
            GridCellSizeInMeters = geoGridMetricResult.GridCellSizeInMeters,
            GridCells = geoGridMetricResult.GridCells.Select(cell => cell.ToGridCellDto(geoGridMetricResult.GridCellSizeInMeters)),
            Sweref99TmBoundingBox = geoGridMetricResult.BoundingBox.ToXYBoundingBoxDto()
        };
    }

    public static IEnumerable<YearCountResultDto> ToDtos(this IEnumerable<YearCountResult> yearMonthResults)
    {
        return yearMonthResults.Select(item => item.ToDto());
    }

    public static YearCountResultDto ToDto(this YearCountResult yearCountResult)
    {
        return new YearCountResultDto
        {
            Count = yearCountResult.Count,
            TaxonCount = yearCountResult.TaxonCount,
            Year = yearCountResult.Year
        };
    }

    public static IEnumerable<YearMonthCountResultDto> ToDtos(this IEnumerable<YearMonthCountResult> yearMonthCountResults)
    {
        return yearMonthCountResults.Select(item => item.ToDto());
    }

    public static YearMonthCountResultDto ToDto(this YearMonthCountResult yearMonthCountResult)
    {
        return new YearMonthCountResultDto
        {
            Count = yearMonthCountResult.Count,
            Month = yearMonthCountResult.Month,
            TaxonCount = yearMonthCountResult.TaxonCount,
            Year = yearMonthCountResult.Year
        };
    }

    public static IEnumerable<YearMonthDayCountResultDto> ToDtos(this IEnumerable<YearMonthDayCountResult> yearMonthDayCountResults)
    {
        return yearMonthDayCountResults.Select(item => item.ToDto());
    }

    public static YearMonthDayCountResultDto ToDto(this YearMonthDayCountResult yearMonthDayCountResult)
    {
        return new YearMonthDayCountResultDto
        {
            Count = yearMonthDayCountResult.Count,
            Day = yearMonthDayCountResult.Day,
            Month = yearMonthDayCountResult.Month,
            Localities = yearMonthDayCountResult.Localities?.ToDtos()!,
            TaxonCount = yearMonthDayCountResult.TaxonCount,
            Year = yearMonthDayCountResult.Year
        };
    }

    public static PagedResultDto<TRecordDto> ToPagedResultDto<TRecord, TRecordDto>(
        this PagedResult<TRecord> pagedResult,
        IEnumerable<TRecordDto> records)
    {
        return new PagedResultDto<TRecordDto>
        {
            Records = records,
            Skip = pagedResult.Skip,
            Take = pagedResult.Take,
            TotalCount = pagedResult.TotalCount,
        };
    }

    public static GeoPagedResultDto<TRecordDto> ToGeoPagedResultDto<TRecord, TRecordDto>(
        this PagedResult<TRecord> pagedResult,
        IEnumerable<TRecordDto> records,
        OutputFormatDto outputFormat = OutputFormatDto.Json)
    {
        if (outputFormat == OutputFormatDto.Json)
        {
            return new GeoPagedResultDto<TRecordDto>
            {
                Records = records,
                Skip = pagedResult.Skip,
                Take = pagedResult.Take,
                TotalCount = pagedResult.TotalCount,
            };
        }
        
        var jsonRecords = records.Cast<JsonObject>();
        bool flattenProperties = outputFormat == OutputFormatDto.GeoJsonFlat;
        string geoJson = GeoJsonHelper.GetFeatureCollectionString(jsonRecords, flattenProperties);
        return new GeoPagedResultDto<TRecordDto>
        {
            Skip = pagedResult.Skip,
            Take = pagedResult.Take,
            TotalCount = pagedResult.TotalCount,
            GeoJson = geoJson
        };
    }

    public static ScrollResultDto<TRecordDto> ToScrollResultDto<TRecord, TRecordDto>(this ScrollResult<TRecord> scrollResult, IEnumerable<TRecordDto> records)
    {
        return new ScrollResultDto<TRecordDto>
        {
            Records = records,
            ScrollId = scrollResult.ScrollId,
            HasMorePages = scrollResult.ScrollId != null,
            Take = scrollResult.Take,
            TotalCount = scrollResult.TotalCount
        };
    }

    public static IEnumerable<TimeSeriesHistogramResultDto> ToTimeSeriesHistogramResultDtos(
        this IEnumerable<TimeSeriesHistogramResult> result)
    {
        return result.Select(item => item.ToTimeSeriesHistogramResultDto());
    }

    public static TimeSeriesHistogramResultDto ToTimeSeriesHistogramResultDto(
        this TimeSeriesHistogramResult result)
    {
        return new TimeSeriesHistogramResultDto
        {
            Type = (TimeSeriesTypeDto)(int)result.Type,
            Period = result.Period,
            Observations = result.Observations,
            Quantity = result.Quantity,
            Taxa = result.Taxa
        };
    }
}