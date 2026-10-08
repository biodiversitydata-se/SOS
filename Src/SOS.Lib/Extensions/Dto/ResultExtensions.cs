﻿using SOS.Lib.Helpers;
using SOS.Lib.Models.Search.Result;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using System.Text.Json.Nodes;
using SOS.Api.Contracts.Aggregations.Responses;

namespace SOS.Lib.Extensions.Dto;

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
                BoundingBox = geoGridTileResult.BoundingBox.GetCompleteBoundingBox(),
                GridCells = geoGridTileResult.GridCellTiles.Select(cell => cell.ToGeoGridCellDto()).ToList(),
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
                BoundingBox = geoGridMetricResult.BoundingBox.GetCompleteBoundingBox(),
                GridCellCount = geoGridMetricResult.GridCellCount,
                GridCellSizeInMeters = geoGridMetricResult.GridCellSizeInMeters,
                GridCells = geoGridMetricResult.GridCells.Select(cell => cell.ToGridCellDto(geoGridMetricResult.GridCellSizeInMeters)).ToList(),
                Sweref99TmBoundingBox = geoGridMetricResult.BoundingBox.ToXYBoundingBoxDto()
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
                Records = records?.ToList() ?? [],
                Skip = pagedResult.Skip,
                Take = pagedResult.Take,
                TotalCount = pagedResult.TotalCount,
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

    extension<TRecord, TSearchAfter>(SearchAfterResult<TRecord, TSearchAfter> searchAfterResult)
    {
        public SearchByCursorResultDto<TRecordDto> ToSearchByCursorResultDto<TRecordDto>(
            IEnumerable<TRecordDto> records,
            int take)
        {            
            return new SearchByCursorResultDto<TRecordDto>
            {
                Records = records,
                NextCursor = searchAfterResult.SearchAfter as string,
                Take = take,
                TotalCount = searchAfterResult.TotalCount
            };
        }
    }

    extension<TRecord>(PagedResult<TRecord> pagedResult)
    {
        public GeoPagedResultDto<TRecordDto> ToGeoPagedResultDto<TRecordDto>(
            IEnumerable<TRecordDto> records,
            OutputFormatDto outputFormat = OutputFormatDto.Json,
            SearchResultMetadataDto? metadata = null)
        {
            GeoPagedResultDto<TRecordDto> dto;
            if (outputFormat == OutputFormatDto.Json)
            {
                dto = new GeoPagedResultDto<TRecordDto>
                {
                    Records = records?.ToList() ?? [],
                    Skip = pagedResult.Skip,
                    Take = pagedResult.Take,
                    TotalCount = pagedResult.TotalCount,
                };
            }
            else
            {
                var jsonRecords = records.Cast<JsonObject>();
                bool flattenProperties = outputFormat == OutputFormatDto.GeoJsonFlat;
                string geoJson = GeoJsonHelper.GetFeatureCollectionString(jsonRecords, flattenProperties);
                dto = new GeoPagedResultDto<TRecordDto>
                {
                    Skip = pagedResult.Skip,
                    Take = pagedResult.Take,
                    TotalCount = pagedResult.TotalCount,
                    GeoJson = geoJson
                };
            }

            dto.Metadata = metadata;
            return dto;
        }
    }
}
