using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.DependencyInjection;
using SOS.Api.Client.Requests;
using SOS.Api.Client.UnitTests.Support;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Exports.Requests;
using SOS.Api.Contracts.Observations.Requests;
using System.Net;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.Clients;

public class SosRequestLoggingTests
{
    [Fact]
    public async Task RequestLoggingLevelNone_DoesNotConsultLoggerOrCreateEntry()
    {
        var logger = new ListLogger<SosObservationsClient>();
        var client = new SosObservationsClient(
            CreateHttpClient(_ => SearchResponse()),
            logger,
            CreateOptions(LogLevel.None));

        await client.ObservationsBySearchInternalAsync(
            "{}",
            cancellationToken: TestContext.Current.CancellationToken);

        logger.IsEnabledCallCount.Should().Be(0);
        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task ObservationsJsonRequest_LogsStructuredRequestOnce()
    {
        const string requestBody = """{"taxon":{"ids":[1,2]}}""";
        var logger = new ListLogger<SosObservationsClient>(LogLevel.Debug);
        var client = new SosObservationsClient(
            CreateHttpClient(_ => SearchResponse()),
            logger,
            CreateOptions(LogLevel.Debug));

        await client.ObservationsBySearchInternalAsync(
            requestBody,
            new ObservationSearchOptions { Skip = 3, Take = 2 },
            TestContext.Current.CancellationToken);

        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.Level.Should().Be(LogLevel.Debug);
        entry.EventId.Name.Should().Be("SosApiRequest");
        entry.Properties["HttpMethod"].Should().Be("POST");
        entry.Properties["RequestUri"].Should().BeOfType<string>()
            .Which.Should().StartWith("https://sos.example.test/observations/internal/search?")
            .And.Contain("skip=3")
            .And.Contain("take=2");
        entry.Properties["RequestBody"].Should().Be(requestBody);
        entry.Properties["OriginalRequestBodyLength"].Should().Be(requestBody.Length);
        entry.Properties["RequestBodyTruncated"].Should().Be(false);
    }

    [Fact]
    public async Task PublicCursorRequest_IsNotLogged()
    {
        var logger = new ListLogger<SosObservationsClient>();
        var client = new SosObservationsClient(
            CreateHttpClient(_ => JsonResponse(
                """{"nextCursor":null,"take":0,"totalCount":0,"records":[]}""")),
            logger,
            CreateOptions());

        await client.ObservationsByCursorJsonAsync(
            "{}",
            cancellationToken: TestContext.Current.CancellationToken);

        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task AggregationTypedRequest_IsLogged()
    {
        var logger = new ListLogger<SosObservationAggregationsClient>();
        var client = new SosObservationAggregationsClient(
            CreateHttpClient(_ => JsonResponse("[]")),
            logger,
            CreateOptions());

        await client.TimeSeriesHistogramInternalAsync(
            new SearchFilterAggregationInternalDto(),
            cancellationToken: TestContext.Current.CancellationToken);

        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.Properties["RequestUri"].Should().BeOfType<string>()
            .Which.Should().Contain("/observations/internal/timeserieshistogram?");
        entry.Properties["RequestBody"].Should().Be("""{"typeFilter":"DoNotShowMerged"}""");
    }

    [Fact]
    public async Task AnalysisTypedRequest_IsLogged()
    {
        var logger = new ListLogger<SosAnalysisClient>();
        var client = new SosAnalysisClient(
            CreateHttpClient(_ => JsonResponse("""{"records":[]}"""), "https://analysis.example.test/"),
            logger,
            CreateOptions());

        await client.AggregateAsync(
            new SearchFilterAggregationInternalDto(),
            new FieldAggregationOptions { AggregationField = "taxon.id" },
            TestContext.Current.CancellationToken);

        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.Properties["RequestUri"].Should().BeOfType<string>()
            .Which.Should().Contain("/internal/aggregation?")
            .And.Contain("aggregationField=taxon.id");
        entry.Properties["RequestBody"].Should().Be("""{"typeFilter":"DoNotShowMerged"}""");
    }

    [Fact]
    public async Task ExportRequest_RedactsPasswordsOnlyInLoggedUri()
    {
        const string password = "actual-password";
        const string confirmation = "confirmation-password";
        Uri? sentUri = null;
        var logger = new ListLogger<SosExportsClient>();
        var client = new SosExportsClient(
            CreateHttpClient(request =>
            {
                sentUri = request.RequestUri;
                return JsonResponse("\"job-1\"");
            }),
            logger,
            CreateOptions());

        await client.OrderInternalAsync(
            new SearchFilterInternalDto(),
            SosExportType.Csv,
            new ExportOrderOptions
            {
                Description = "Logged export",
                EncryptPassword = password,
                ConfirmEncryptPassword = confirmation
            },
            TestContext.Current.CancellationToken);

        sentUri!.Query.Should().Contain(password).And.Contain(confirmation);
        var loggedUri = logger.Entries.Should().ContainSingle().Subject
            .Properties["RequestUri"].Should().BeOfType<string>().Subject;
        loggedUri.Should().NotContain(password).And.NotContain(confirmation);
        loggedUri.Should().Contain("encryptPassword=[REDACTED]")
            .And.Contain("confirmEncryptPassword=[REDACTED]");
    }

    [Fact]
    public async Task LargeTaxonIds_AreReducedWithoutChangingSentBody()
    {
        var ids = string.Join(',', Enumerable.Range(1, 200));
        var requestBody = $$"""{"taxon":{"ids":[{{ids}}]},"occurrenceStatus":"Present"}""";
        string? sentBody = null;
        var logger = new ListLogger<SosObservationsClient>();
        var client = new SosObservationsClient(
            CreateHttpClient(request =>
            {
                sentBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                return SearchResponse();
            }),
            logger,
            CreateOptions(maxBodyLength: 100));

        await client.ObservationsBySearchInternalAsync(
            requestBody,
            cancellationToken: TestContext.Current.CancellationToken);

        sentBody.Should().Be(requestBody);
        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.Properties["RequestBody"].Should().Be("""{"taxon":{"ids":[-1]},"occurrenceStatus":"Present"}""");
        entry.Properties["OriginalRequestBodyLength"].Should().Be(requestBody.Length);
        entry.Properties["RequestBodyTruncated"].Should().Be(true);
    }

    [Fact]
    public async Task LargeCoordinates_AreReducedAfterTaxonIds()
    {
        var coordinates = string.Join(',', Enumerable.Range(1, 100).Select(value => $"[{value},{value}]"));
        var requestBody =
            "{\"geographics\":{\"geometries\":[{\"type\":\"Polygon\",\"coordinates\":[" +
            coordinates +
            "]}]}}";
        var logger = new ListLogger<SosObservationsClient>();
        var client = new SosObservationsClient(
            CreateHttpClient(_ => SearchResponse()),
            logger,
            CreateOptions(maxBodyLength: 100));

        await client.ObservationsBySearchInternalAsync(
            requestBody,
            cancellationToken: TestContext.Current.CancellationToken);

        var loggedBody = logger.Entries.Should().ContainSingle().Subject
            .Properties["RequestBody"].Should().BeOfType<string>().Subject;
        loggedBody.Should().Contain("\"coordinates\":[-1]");
        loggedBody.Length.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public async Task LargeUnreducibleBody_IsStrictlyTruncated()
    {
        const int maxBodyLength = 80;
        var requestBody =
            "{\"includeFilter\":{\"occurrenceIds\":[\"" + new string('x', 200) + "\"]}}";
        var logger = new ListLogger<SosObservationsClient>();
        var client = new SosObservationsClient(
            CreateHttpClient(_ => SearchResponse()),
            logger,
            CreateOptions(maxBodyLength: maxBodyLength));

        await client.ObservationsBySearchInternalAsync(
            requestBody,
            cancellationToken: TestContext.Current.CancellationToken);

        var loggedBody = logger.Entries.Should().ContainSingle().Subject
            .Properties["RequestBody"].Should().BeOfType<string>().Subject;
        loggedBody.Should().HaveLength(maxBodyLength).And.EndWith("...[truncated]");
    }

    [Fact]
    public async Task InvalidLargeJson_FallsBackToStrictTruncationWithoutStoppingRequest()
    {
        const int maxBodyLength = 50;
        var requestBody = "not-json-" + new string('x', 100);
        var logger = new ListLogger<SosObservationsClient>();
        var client = new SosObservationsClient(
            CreateHttpClient(_ => SearchResponse()),
            logger,
            CreateOptions(maxBodyLength: maxBodyLength));

        await client.ObservationsBySearchInternalAsync(
            requestBody,
            cancellationToken: TestContext.Current.CancellationToken);

        var loggedBody = logger.Entries.Should().ContainSingle().Subject
            .Properties["RequestBody"].Should().BeOfType<string>().Subject;
        loggedBody.Should().HaveLength(maxBodyLength).And.EndWith("...[truncated]");
    }

    [Fact]
    public async Task ResilienceRetry_LogsLogicalRequestOnce()
    {
        var calls = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            calls++;
            return calls == 1
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                : SearchResponse();
        });
        var logger = new ListLogger<SosObservationsClient>();
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<SosObservationsClient>>(logger);
        services.AddSosClients(
            options =>
            {
                options.ObservationsApiBaseAddress = new Uri("https://sos.example.test/");
                options.RequestLoggingLevel = LogLevel.Trace;
                options.Resilience.RetryDelay = TimeSpan.Zero;
                options.Resilience.RetryMaxDelay = TimeSpan.Zero;
                options.Resilience.MaxRetryAttempts = 1;
            },
            configureBuilder: builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        await using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<ISosObservationsClient>()
            .ObservationsBySearchInternalAsync(
                new SearchFilterInternalDto(),
                cancellationToken: TestContext.Current.CancellationToken);

        calls.Should().Be(2);
        logger.Entries.Should().ContainSingle();
    }

    [Fact]
    public void EnabledLogging_RejectsNonPositiveBodyLimit()
    {
        var services = new ServiceCollection();
        services.AddSosClients(
            options =>
            {
                options.ObservationsApiBaseAddress = new Uri("https://sos.example.test/");
                options.RequestLoggingLevel = LogLevel.Debug;
                options.MaxLoggedRequestBodyLength = 0;
            },
            useResilience: false);
        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<ISosObservationsClient>();

        action.Should().Throw<OptionsValidationException>()
            .WithMessage("*MaxLoggedRequestBodyLength*");
    }

    private static IOptions<SosClientOptions> CreateOptions(
        LogLevel logLevel = LogLevel.Trace,
        int maxBodyLength = 8192)
    {
        return Options.Create(new SosClientOptions
        {
            ObservationsApiBaseAddress = new Uri("https://sos.example.test/"),
            AnalysisApiBaseAddress = new Uri("https://analysis.example.test/"),
            RequestLoggingLevel = logLevel,
            MaxLoggedRequestBodyLength = maxBodyLength
        });
    }

    private static HttpClient CreateHttpClient(
        Func<HttpRequestMessage, HttpResponseMessage> send,
        string baseAddress = "https://sos.example.test/")
    {
        return new HttpClient(new StubHttpMessageHandler(send))
        {
            BaseAddress = new Uri(baseAddress)
        };
    }

    private static HttpResponseMessage SearchResponse() => JsonResponse(
        """{"skip":0,"take":0,"totalCount":0,"records":[]}""");

    private static HttpResponseMessage JsonResponse(string body)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
}
