using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search.Result;
using SOS.Shared.Api.Dtos;
using System.Text.Json.Nodes;


namespace SOS.Shared.Api.Extensions.Dto;

public static class ResultExtensions
{
    extension(GeoGridTileResult geoGridTileResult)
    {
        public GeoGridResultDto ToGeoGridResultDto(long totalGridCellCount)
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
    }

    extension(GeoGridMetricResult geoGridMetricResult)
    {
        public GeoGridMetricResultDto ToDto()
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
    }

    extension(IEnumerable<YearCountResult> yearMonthResults)
    {
        public IEnumerable<YearCountResultDto> ToDtos()
        {
            return yearMonthResults.Select(item => item.ToDto());
        }
    }

    extension(YearCountResult yearCountResult)
    {
        public YearCountResultDto ToDto()
        {
            return new YearCountResultDto
            {
                Count = yearCountResult.Count,
                TaxonCount = yearCountResult.TaxonCount,
                Year = yearCountResult.Year
            };
        }
    }

    extension(IEnumerable<YearMonthCountResult> yearMonthCountResults)
    {
        public IEnumerable<YearMonthCountResultDto> ToDtos()
        {
            return yearMonthCountResults.Select(item => item.ToDto());
        }
    }

    extension(YearMonthCountResult yearMonthCountResult)
    {
        public YearMonthCountResultDto ToDto()
        {
            return new YearMonthCountResultDto
            {
                Count = yearMonthCountResult.Count,
                Month = yearMonthCountResult.Month,
                TaxonCount = yearMonthCountResult.TaxonCount,
                Year = yearMonthCountResult.Year
            };
        }
    }

    extension(IEnumerable<YearMonthDayCountResult> yearMonthDayCountResults)
    {
        public IEnumerable<YearMonthDayCountResultDto> ToDtos()
        {
            return yearMonthDayCountResults.Select(item => item.ToDto());
        }
    }

    extension(YearMonthDayCountResult yearMonthDayCountResult)
    {
        public YearMonthDayCountResultDto ToDto()
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
    }

    extension<TRecord>(PagedResult<TRecord> pagedResult)
    {
        public PagedResultDto<TRecordDto> ToPagedResultDto<TRecordDto>(
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

        public GeoPagedResultDto<TRecordDto> ToGeoPagedResultDto<TRecordDto>(
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
    }

    extension<TRecord>(ScrollResult<TRecord> scrollResult)
    {
        public ScrollResultDto<TRecordDto> ToScrollResultDto<TRecordDto>(IEnumerable<TRecordDto> records)
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
    }

    extension(IEnumerable<TimeSeriesHistogramResult> result)
    {
        public IEnumerable<TimeSeriesHistogramResultDto> ToTimeSeriesHistogramResultDtos(
)
        {
            return result.Select(item => item.ToTimeSeriesHistogramResultDto());
        }
    }

    extension(TimeSeriesHistogramResult result)
    {
        public TimeSeriesHistogramResultDto ToTimeSeriesHistogramResultDto(
)
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
}