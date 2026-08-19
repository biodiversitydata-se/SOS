using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.DependencyInjection;
using SOS.Api.Client.DependencyInjection.Authentication;
using SOS.Api.Client.Exceptions;
using SOS.Api.Client.Requests;
using SOS.Api.Client.UnitTests.Support;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Exports.Requests;
using SOS.Api.Contracts.Observations.Requests;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.DependencyInjection;

public class RequestContextAndOrderPolicyTests
{
    [Fact]
    public async Task SosClientResolvesAsAggregateEntryPoint()
    {
        var services = new ServiceCollection();
        services.AddSosClients(
            options =>
            {
                options.ObservationsApiBaseAddress = new Uri("https://observations.example.test/");
                options.AnalysisApiBaseAddress = new Uri("https://analysis.example.test/");
            },
            useResilience: false);
        await using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<SosClient>();

        client.Observations.Should().NotBeNull();
        client.Analysis.Should().NotBeNull();
        client.Exports.Should().NotBeNull();
        client.ObservationSearch.Should().NotBeNull();
        client.AnalysisService.Should().NotBeNull();
        client.ExportService.Should().NotBeNull();
        client.WorkspaceFilters.Should().NotBeNull();
    }

    [Fact]
    public async Task SafeGet_IsRetriedForTransientFailure()
    {
        var calls = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            calls++;
            return calls == 1
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                : JsonResponse("[]");
        });
        var services = new ServiceCollection();
        services.AddSosClients(
            options =>
            {
                options.ObservationsApiBaseAddress = new Uri("https://observations.example.test/");
                options.Resilience.RetryDelay = TimeSpan.Zero;
                options.Resilience.RetryMaxDelay = TimeSpan.Zero;
                options.Resilience.MaxRetryAttempts = 1;
            },
            useResilience: true,
            configureBuilder: builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        await using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<ISosDataProvidersClient>()
            .GetDataProvidersAsync(cancellationToken: TestContext.Current.CancellationToken);

        calls.Should().Be(2);
    }

    [Fact]
    public async Task ExportOrder_ExplicitRoleOverridesProviderAndFailedPostIsNotRetried()
    {
        var calls = 0;
        var handler = new StubHttpMessageHandler(request =>
        {
            calls++;
            request.Headers.GetValues("X-Authorization-Role-Id").Should().ContainSingle("7");
            request.Headers.GetValues("X-Authorization-Application-Identifier").Should().ContainSingle("explicit-app");
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("temporarily unavailable")
            };
        });
        var services = new ServiceCollection();
        services.AddSingleton<ISosRoleIdProvider>(new FixedRoleIdProvider(99));
        services.AddSosClients(
            options =>
            {
                options.ObservationsApiBaseAddress = new Uri("https://observations.example.test/");
                options.AnalysisApiBaseAddress = new Uri("https://analysis.example.test/");
                options.ApplicationIdentifier = "default-app";
            },
            useResilience: true,
            configureBuilder: builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<ISosExportsClient>();

        var action = () => client.OrderInternalAsync(
            new SearchFilterInternalDto(),
            SosExportType.Csv,
            new ExportOrderOptions
            {
                Description = "test",
                RoleId = 7,
                AuthorizationApplicationIdentifier = "explicit-app"
            },
            TestContext.Current.CancellationToken);

        await action.Should().ThrowAsync<SosApiException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task AnalysisOrder_FailedPostIsNotRetried()
    {
        var calls = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            calls++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("temporarily unavailable")
            };
        });
        var services = new ServiceCollection();
        services.AddSosClients(
            options =>
            {
                options.ObservationsApiBaseAddress = new Uri("https://observations.example.test/");
                options.AnalysisApiBaseAddress = new Uri("https://analysis.example.test/");
                options.Resilience.RetryDelay = TimeSpan.Zero;
                options.Resilience.RetryMaxDelay = TimeSpan.Zero;
                options.Resilience.MaxRetryAttempts = 2;
            },
            useResilience: true,
            configureBuilder: builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        await using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<ISosAnalysisClient>()
            .OrderAooEooAsync(
                new SearchFilterAggregationInternalDto(),
                new AooEooOrderOptions { Description = "test" },
                TestContext.Current.CancellationToken);

        await action.Should().ThrowAsync<SosApiException>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task RoleProviderAndConfiguredDefaultsDoNotDuplicateHeaders()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.GetValues("X-Authorization-Role-Id").Should().ContainSingle("99");
            request.Headers.GetValues("X-Authorization-Application-Identifier").Should().ContainSingle("default-app");
            request.Headers.GetValues("X-Requesting-System").Should().ContainSingle("test-system");
            request.Headers.Accept.Select(header => header.MediaType)
                .Should().Equal("application/json");
            return JsonResponse("[]");
        });
        var services = new ServiceCollection();
        services.AddSingleton<ISosRoleIdProvider>(new FixedRoleIdProvider(99));
        services.AddSosClients(
            options =>
            {
                options.ObservationsApiBaseAddress = new Uri("https://observations.example.test/");
                options.ApplicationIdentifier = "default-app";
                options.SystemName = "test-system";
            },
            useResilience: false,
            configureBuilder: builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        await using var provider = services.BuildServiceProvider();

        var result = await provider
            .GetRequiredService<ISosDataProvidersClient>()
            .GetDataProvidersAsync(cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConfiguredHeadersFillMissingValuesWithoutAddingRole()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.Contains("X-Authorization-Role-Id").Should().BeFalse();
            request.Headers.GetValues("X-Authorization-Application-Identifier").Should().ContainSingle("default-app");
            request.Headers.GetValues("X-Requesting-System").Should().ContainSingle("test-system");
            return JsonResponse("[]");
        });
        var services = new ServiceCollection();
        services.AddSosClients(
            options =>
            {
                options.ObservationsApiBaseAddress = new Uri("https://observations.example.test/");
                options.ApplicationIdentifier = "default-app";
                options.SystemName = "test-system";
            },
            useResilience: false,
            configureBuilder: builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        await using var provider = services.BuildServiceProvider();

        await provider
            .GetRequiredService<ISosDataProvidersClient>()
            .GetDataProvidersAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MissingHeaderValuesAreOmitted()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.Contains("X-Authorization-Role-Id").Should().BeFalse();
            request.Headers.Contains("X-Authorization-Application-Identifier").Should().BeFalse();
            request.Headers.Contains("X-Requesting-System").Should().BeFalse();
            return JsonResponse("[]");
        });
        var services = new ServiceCollection();
        services.AddSosClients(
            options => options.ObservationsApiBaseAddress =
                new Uri("https://observations.example.test/"),
            useResilience: false,
            configureBuilder: builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        await using var provider = services.BuildServiceProvider();

        await provider
            .GetRequiredService<ISosDataProvidersClient>()
            .GetDataProvidersAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public void MultipleRoleIdProvidersFailWithClearConfigurationError()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISosRoleIdProvider>(new FixedRoleIdProvider(1));
        services.AddSingleton<ISosRoleIdProvider>(new FixedRoleIdProvider(2));
        services.AddSosClients(
            options => options.ObservationsApiBaseAddress =
                new Uri("https://observations.example.test/"),
            useResilience: false);
        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<ISosDataProvidersClient>();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only one ISosRoleIdProvider*");
    }

    [Fact]
    public async Task BinaryDownloadSuppressesDefaultJsonAccept()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.Accept.Should().BeEmpty();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([1, 2, 3])
            };
        });
        var services = new ServiceCollection();
        services.AddSosClients(
            options => options.ObservationsApiBaseAddress =
                new Uri("https://observations.example.test/"),
            useResilience: false,
            configureBuilder: builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        await using var provider = services.BuildServiceProvider();

        await using var response = await provider
            .GetRequiredService<ISosExportsClient>()
            .DownloadInternalAsync(
                new SearchFilterInternalDto(),
                SosExportType.Csv,
                cancellationToken: TestContext.Current.CancellationToken);

        response.ContentLength.Should().Be(3);
    }

    [Fact]
    public async Task WebClientAddsBearerTokenWhenAuthorizationIsMissing()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.Authorization.Should().BeEquivalentTo(
                new AuthenticationHeaderValue("Bearer", "access-token"));
            return JsonResponse("[]");
        });
        var services = new ServiceCollection();
        services.AddSingleton<ISosAccessTokenProvider>(new FixedAccessTokenProvider("access-token"));
        services.AddSosWebClients(
            options => options.ObservationsApiBaseAddress =
                new Uri("https://observations.example.test/"),
            useResilience: false,
            configureBuilder: builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        await using var provider = services.BuildServiceProvider();

        await provider
            .GetRequiredService<ISosDataProvidersClient>()
            .GetDataProvidersAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task WebClientPreservesExistingAuthorizationHeader()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            request.Headers.Authorization.Should().BeEquivalentTo(
                new AuthenticationHeaderValue("Basic", "existing-credentials"));
            return JsonResponse("[]");
        });
        var services = new ServiceCollection();
        services.AddSingleton<ISosAccessTokenProvider>(new FixedAccessTokenProvider("access-token"));
        services.AddSosWebClients(
            options => options.ObservationsApiBaseAddress =
                new Uri("https://observations.example.test/"),
            useResilience: false,
            configureBuilder: builder =>
            {
                builder.ConfigureHttpClient(client =>
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", "existing-credentials"));
                builder.ConfigurePrimaryHttpMessageHandler(() => handler);
            });
        await using var provider = services.BuildServiceProvider();

        await provider
            .GetRequiredService<ISosDataProvidersClient>()
            .GetDataProvidersAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class FixedRoleIdProvider(int roleId) : ISosRoleIdProvider
    {
        public int? GetCurrentRoleId() => roleId;
    }

    private sealed class FixedAccessTokenProvider(string token) : ISosAccessTokenProvider
    {
        public ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<string?>(token);
    }
}
