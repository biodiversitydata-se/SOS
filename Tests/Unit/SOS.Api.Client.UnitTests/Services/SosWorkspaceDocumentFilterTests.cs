using FluentAssertions;
using GeoJSON.Text.Geometry;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Exceptions;
using SOS.Api.Client.Services;
using SOS.Api.Client.Workspaces;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Workspaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SOS.Api.Client.UnitTests.Services;

public class SosWorkspaceDocumentFilterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Document_ConvertsLikeItsSearchAndDoesNotMutateSavedState(bool aggregation)
    {
        ISosWorkspaceFilterService service = new SosWorkspaceFilterService(TimeProvider.System, []);
        var workspace = new SosWorkspaceDocument
        {
            Name = "Saved search",
            Search = new()
            {
                DataProviderIds = [1],
                ProjectIds = [123],
                Geometries = [new Point(new Position(59.3, 18.1))],
                TaxonSelection = new() { Taxa = [new() { TaxonId = 10 }] }
            },
            Settings = new() { AnalysisSrid = 3006, ExportSrid = 4326 },
            ApplicationState = new()
            {
                ["another-app"] = JsonSerializer.SerializeToElement(new { selectedTab = "map" })
            }
        };
        var savedJson = JsonSerializer.Serialize(workspace);
        workspace = JsonSerializer.Deserialize<SosWorkspaceDocument>(savedJson)!;
        var overrides = new WorkspaceFilterOverrides
        {
            TaxonIds = [20],
            Point = new LatLonCoordinateDto { Latitude = 59.4, Longitude = 18.2 },
            RadiusInMeters = 500,
            DateFrom = new DateTime(2024, 1, 1),
            DateTo = new DateTime(2024, 12, 31),
            Output = new OutputFilterExtendedDto()
        };
        var cancellationToken = TestContext.Current.CancellationToken;

        SearchFilterInternalBaseDto fromDocument = aggregation
            ? await service.CreateAggregationFilterAsync(workspace, overrides, cancellationToken)
            : await service.CreateSearchFilterAsync(workspace, overrides, cancellationToken);
        SearchFilterInternalBaseDto fromSearch = aggregation
            ? await service.CreateAggregationFilterAsync(workspace.Search, overrides, cancellationToken)
            : await service.CreateSearchFilterAsync(workspace.Search, overrides, cancellationToken);

        fromDocument.Taxon!.Ids.Should().Equal(20);
        fromDocument.Geographics!.Geometries.Should().HaveCount(2);
        fromDocument.Geographics.MaxDistanceFromPoint.Should().Be(500);
        fromDocument.Date!.StartDate!.Value.Year.Should().Be(2024);
        JsonNode.DeepEquals(
            JsonSerializer.SerializeToNode(fromDocument, fromDocument.GetType()),
            JsonSerializer.SerializeToNode(fromSearch, fromSearch.GetType())).Should().BeTrue();
        JsonSerializer.Serialize(workspace).Should().Be(savedJson);
        if (!aggregation)
        {
            ((SearchFilterInternalDto)fromDocument).Output.Should().BeSameAs(overrides.Output);
        }
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 2)]
    [InlineData(true, 0)]
    [InlineData(true, 2)]
    public async Task Document_RejectsUnsupportedSchemaBeforeResolvingTaxa(bool aggregation, int version)
    {
        ISosWorkspaceFilterService service = new SosWorkspaceFilterService(TimeProvider.System, []);
        var workspace = new SosWorkspaceDocument
        {
            SchemaVersion = version,
            Search = new() { TaxonSelection = new() { TaxonListIds = [228] } }
        };

        var convert = async () =>
        {
            if (aggregation)
                await service.CreateAggregationFilterAsync(workspace, cancellationToken: TestContext.Current.CancellationToken);
            else
                await service.CreateSearchFilterAsync(workspace, cancellationToken: TestContext.Current.CancellationToken);
        };

        var exception = await convert.Should().ThrowAsync<SosWorkspaceFilterValidationException>();
        exception.Which.ParameterName.Should().Be(nameof(SosWorkspaceDocument.SchemaVersion));
        exception.Which.AllowedValues.Should().Equal("1");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Document_RejectsNullSearch(bool aggregation)
    {
        ISosWorkspaceFilterService service = new SosWorkspaceFilterService(TimeProvider.System, []);
        var workspace = JsonSerializer.Deserialize<SosWorkspaceDocument>(
            "{\"schemaVersion\":1,\"search\":null}")!;

        var convert = async () =>
        {
            if (aggregation)
                await service.CreateAggregationFilterAsync(workspace, cancellationToken: TestContext.Current.CancellationToken);
            else
                await service.CreateSearchFilterAsync(workspace, cancellationToken: TestContext.Current.CancellationToken);
        };

        var exception = await convert.Should().ThrowAsync<SosWorkspaceFilterValidationException>();
        exception.Which.ParameterName.Should().Be(nameof(SosWorkspaceDocument.Search));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Document_PassesCancellationToTaxonSource(bool aggregation)
    {
        var source = new CancelledTaxonSource();
        ISosWorkspaceFilterService service = new SosWorkspaceFilterService(TimeProvider.System, [source]);
        var workspace = new SosWorkspaceDocument
        {
            Search = new() { TaxonSelection = new() { TaxonListIds = [228] } }
        };
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var convert = async () =>
        {
            if (aggregation)
                await service.CreateAggregationFilterAsync(workspace, cancellationToken: cancellation.Token);
            else
                await service.CreateSearchFilterAsync(workspace, cancellationToken: cancellation.Token);
        };

        await convert.Should().ThrowAsync<OperationCanceledException>();
        source.ReceivedToken.Should().Be(cancellation.Token);
    }

    private sealed class CancelledTaxonSource : ISosWorkspaceTaxonSource
    {
        public CancellationToken ReceivedToken { get; private set; }

        public Task<IReadOnlyCollection<int>> GetTaxonListIdsAsync(
            int taxonListId, bool includeChildTaxaForRedlists, CancellationToken cancellationToken = default)
        {
            ReceivedToken = cancellationToken;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyCollection<int>>([]);
        }

        public Task<SosResolvedTaxon?> GetTaxonAsync(int taxonId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyCollection<SosResolvedTaxon>> GetDescendantsAsync(
            int taxonId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
