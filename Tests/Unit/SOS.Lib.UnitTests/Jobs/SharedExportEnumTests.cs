using SOS.Api.Contracts.Common.Geo;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Exports.Requests;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Models.Search.Filters;
using Xunit;

namespace SOS.Lib.UnitTests.Jobs;

public class SharedExportEnumTests
{
    [Theory]
    [InlineData(MetricCoordinateSys.SWEREF99_TM, CoordinateSys.WGS84)]
    [InlineData(MetricCoordinateSys.ETRS89_LAEA_Europe, CoordinateSys.ETRS89_LAEA_Europe)]
    public void NewAnalysisExportJobs_RoundTripWithSharedCoordinateSystems(MetricCoordinateSys metric, CoordinateSys response)
    {
        var jobs = new[]
        {
            (Job.FromExpression<IExportAndSendJob>(job => job.RunAooEooAsync(
                1, null, "app", new SearchFilterAggregationInternalDto(), ProtectionFilterDto.Public,
                1000, false, new[] { 1.0 }, false, false, false, false,
                metric, response, "user@example.org", "export", ExportFormat.AooEoo,
                false, null, null, JobCancellationToken.Null)), MetricIndex: 12, ResponseIndex: 13),
            (Job.FromExpression<IExportAndSendJob>(job => job.RunAooEooArticle17Async(
                1, null, "app", new SearchFilterBaseDto(), ProtectionFilterDto.Public,
                1000, 100, metric, response, "user@example.org", "export",
                ExportFormat.AooEooArticle17, false, null, null, JobCancellationToken.Null)), MetricIndex: 7, ResponseIndex: 8)
        };

        foreach (var (job, metricIndex, responseIndex) in jobs)
        {
            var restored = InvocationData.SerializeJob(job).DeserializeJob();
            Assert.Equal(job.Method, restored.Method);
            Assert.Equal(typeof(MetricCoordinateSys), restored.Method.GetParameters()[metricIndex].ParameterType);
            Assert.Equal(typeof(CoordinateSys), restored.Method.GetParameters()[responseIndex].ParameterType);
            Assert.Equal(metric, Assert.IsType<MetricCoordinateSys>(restored.Args[metricIndex]));
            Assert.Equal(response, Assert.IsType<CoordinateSys>(restored.Args[responseIndex]));
        }
    }

    [Theory]
    [InlineData("SWEREF99_TM", 3006)]
    [InlineData("ETRS89", 3035)]
    [InlineData("ETRS89_LAEA_Europe", 3035)]
    public void StoredCoordinateSystems_PreserveNewtonsoftNamesAndNumericValues(string name, int epsg)
    {
        var settings = new JsonSerializerSettings { Converters = { new StringEnumConverter() } };
        var value = JsonConvert.DeserializeObject<MetricCoordinateSys>($"\"{name}\"", settings);

        Assert.Equal(epsg, (int)value);
        Assert.Equal(epsg.ToString(), JsonConvert.SerializeObject(value));
        Assert.Equal(value, JsonConvert.DeserializeObject<MetricCoordinateSys>(epsg.ToString(), settings));
        Assert.Equal(value, JsonConvert.DeserializeObject<MetricCoordinateSys>(JsonConvert.SerializeObject(value, settings), settings));
    }

    [Theory]
    [InlineData(TaxonomyType.Dyntaxa, "Dyntaxa")]
    [InlineData(TaxonomyType.Artportalen, "Artportalen")]
    public void StoredExportFiltersAndNewJobs_PreserveSharedTaxonomy(TaxonomyType taxonomy, string name)
    {
        var filter = new SearchFilter { Taxa = new TaxonFilter { TaxonomyType = taxonomy } };
        var settings = new JsonSerializerSettings { Converters = { new StringEnumConverter() } };
        var json = JsonConvert.SerializeObject(filter, settings);
        Assert.Contains($"\"TaxonomyType\":\"{name}\"", json);
        Assert.Equal(taxonomy, JsonConvert.DeserializeObject<SearchFilter>(json, settings)!.Taxa.TaxonomyType);

        var job = Job.FromExpression<IExportAndStoreJob>(job => job.RunAsync(
            filter, "exports", "observations", JobCancellationToken.Null));
        var restored = InvocationData.SerializeJob(job).DeserializeJob();
        Assert.Equal(job.Method, restored.Method);
        Assert.Equal(taxonomy, Assert.IsType<SearchFilter>(restored.Args[0]).Taxa.TaxonomyType);
    }

    [Theory]
    [InlineData(AggregationType.SightingsPerWeek, "SightingsPerWeek")]
    [InlineData(AggregationType.SightingsPerYear, "SightingsPerYear")]
    [InlineData(AggregationType.QuantityPerWeek, "QuantityPerWeek")]
    [InlineData(AggregationType.QuantityPerYear, "QuantityPerYear")]
    [InlineData(AggregationType.SpeciesSightingsList, "SpeciesSightingsList")]
    [InlineData(AggregationType.SpeciesSightingsListTaxonCount, "SpeciesSightingsListTaxonCount")]
    [InlineData(AggregationType.SightingsPerWeek48, "SightingsPerWeek48")]
    public void AggregationType_RoundTripsWithBackgroundJobSerializerSettings(AggregationType value, string name)
    {
        var settings = new JsonSerializerSettings { Converters = { new StringEnumConverter() } };
        var json = JsonConvert.SerializeObject(value, settings);
        Assert.Equal($"\"{name}\"", json);
        Assert.Equal(value, JsonConvert.DeserializeObject<AggregationType>(json, settings));
    }

    [Theory]
    [InlineData(SearchSortOrder.Asc, "Asc")]
    [InlineData(SearchSortOrder.Desc, "Desc")]
    public void StoredExportFilters_PreserveStringSortOrdersWithoutEnumAttributes(SearchSortOrder order, string name)
    {
        var filter = new SearchFilter
        {
            Output = new OutputFilter
            {
                SortOrders = [new SortOrderFilter { SortBy = "modified", SortOrder = order }]
            }
        };
        // Job producers and workers register this converter in their Hangfire configuration.
        var settings = new JsonSerializerSettings { Converters = { new StringEnumConverter() } };

        var json = JsonConvert.SerializeObject(filter, settings);
        Assert.Contains($"\"SortOrder\":\"{name}\"", json);
        var restoredFilter = JsonConvert.DeserializeObject<SearchFilter>(json)!;
        Assert.Equal(order, Assert.Single(restoredFilter.Output.SortOrders).SortOrder);

        var job = Job.FromExpression<IExportAndStoreJob>(job => job.RunAsync(
            filter, "exports", "observations", JobCancellationToken.Null));
        var restoredJob = InvocationData.SerializeJob(job).DeserializeJob();
        Assert.Equal(job.Method, restoredJob.Method);
        var restoredJobFilter = Assert.IsType<SearchFilter>(restoredJob.Args[0]);
        Assert.Equal(order, Assert.Single(restoredJobFilter.Output.SortOrders).SortOrder);
    }

    [Theory]
    [InlineData(ExportFormat.Csv)]
    [InlineData(ExportFormat.DwC)]
    [InlineData(ExportFormat.DwCEvent)]
    [InlineData(ExportFormat.GeoJson)]
    [InlineData(ExportFormat.Excel)]
    [InlineData(ExportFormat.AooEoo)]
    [InlineData(ExportFormat.AooEooArticle17)]
    public void NewExportJobs_RoundTripWithSharedFormat(ExportFormat format)
    {
        var jobs = new[]
        {
            (Job.FromExpression<IExportAndSendJob>(job => job.RunAsync(
                new SearchFilterBaseDto(), ProtectionFilterDto.Public, 1, null, "app", "user@example.org", "export",
                format, "sv-SE", false, PropertyLabelType.PropertyName, false, false, null, false, null,
                JobCancellationToken.Null)), FormatIndex: 7),
            (Job.FromExpression<IExportAndSendJob>(job => job.RunAooEooAsync(
                1, null, "app", new SearchFilterAggregationInternalDto(), ProtectionFilterDto.Public,
                1000, false, new[] { 1.0 }, false, false, false, false,
                MetricCoordinateSys.SWEREF99_TM, CoordinateSys.WGS84, "user@example.org", "export", format,
                false, null, null, JobCancellationToken.Null)), FormatIndex: 16),
            (Job.FromExpression<IExportAndSendJob>(job => job.RunAooEooArticle17Async(
                1, null, "app", new SearchFilterBaseDto(), ProtectionFilterDto.Public,
                1000, 100, MetricCoordinateSys.SWEREF99_TM, CoordinateSys.WGS84, "user@example.org", "export",
                format, false, null, null, JobCancellationToken.Null)), FormatIndex: 11)
        };

        foreach (var (job, formatIndex) in jobs)
        {
            var restored = InvocationData.SerializeJob(job).DeserializeJob();

            Assert.Equal(job.Method, restored.Method);
            Assert.Equal(typeof(ExportFormat), restored.Method.GetParameters()[formatIndex].ParameterType);
            Assert.Equal(format, Assert.IsType<ExportFormat>(restored.Args[formatIndex]));
        }
    }

    [Theory]
    [InlineData(PropertyLabelType.PropertyName)]
    [InlineData(PropertyLabelType.PropertyPath)]
    [InlineData(PropertyLabelType.Swedish)]
    [InlineData(PropertyLabelType.English)]
    public void NewExportAndValidationJobs_RoundTripWithSharedLabels(PropertyLabelType labels)
    {
        var export = Job.FromExpression<IExportAndSendJob>(job => job.RunAsync(
            new SearchFilterBaseDto(), ProtectionFilterDto.Public, 1, null, "app", "user@example.org", "export",
            ExportFormat.Csv, "sv-SE", false, labels, false, false, null, false, null, JobCancellationToken.Null));
        var validation = Job.FromExpression<IDataValidationReportJob>(job => job.RunAsync(
            "report", "user", "provider", 10, 5, 5, null, true, labels, JobCancellationToken.Null));

        foreach (var (job, labelsIndex) in new[] { (export, 10), (validation, 8) })
        {
            var restored = InvocationData.SerializeJob(job).DeserializeJob();

            Assert.Equal(job.Method, restored.Method);
            Assert.Equal(typeof(PropertyLabelType), restored.Method.GetParameters()[labelsIndex].ParameterType);
            Assert.Equal(labels, Assert.IsType<PropertyLabelType>(restored.Args[labelsIndex]));
        }
    }
}
