// Legacy JSON fixtures intentionally exercise the server's pre-contract wire shapes.
// Endpoint signatures themselves use SOS.Api.Contracts DTOs.
global using SOS.Observations.Api.IntegrationTests;
global using DataStewardshipFilterDto = SOS.Api.Contracts.Observations.Requests.DataStewardshipFilterDto;
global using ExcludeFilterDto = SOS.Api.Contracts.Observations.Requests.ExcludeFilterDto;
global using GeoGridResultDto = SOS.Api.Contracts.Aggregations.Responses.GeoGridResultDto;
global using GeoGridMetricResultDto = SOS.Api.Contracts.Aggregations.Responses.GeoGridMetricResultDto;
global using TaxonAggregationItemDto = SOS.Api.Contracts.Aggregations.Responses.TaxonAggregationItemDto;
global using TimeSeriesHistogramResultDto = SOS.Api.Contracts.Aggregations.Responses.TimeSeriesHistogramResultDto;
global using DsDatasetDto = SOS.Api.Contracts.DataStewardship.Responses.DsDatasetDto;
