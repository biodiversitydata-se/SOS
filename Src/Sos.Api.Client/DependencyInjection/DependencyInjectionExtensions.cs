using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SOS.Api.Client;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Clients;
using SOS.Api.Client.DependencyInjection.Authentication;
using SOS.Api.Client.DependencyInjection.Handlers;
using SOS.Api.Client.Services;
using System.Net;
using Polly;

namespace SOS.Api.Client.DependencyInjection;

public static class DependencyInjectionExtensions
{
        /// <summary>
        /// Registers SOS clients without automatic authentication.
        /// Suitable for non-web scenarios, custom authentication, tests, background jobs, etc.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// services.AddSosClients(options =>
        /// {
        ///     options.ObservationsApiBaseAddress = new Uri(Settings.HttpClientsConfiguration.SosObservationsApi.BaseAddress);
        /// },
        /// configureBuilder: httpClientBuilder =>
        /// {
        ///     httpClientBuilder.AddHttpMessageHandler<TokenHandler>();
        /// });        
        /// ]]></code>
        /// </example>
        /// <param name="services">Service collection to register clients in.</param>
        /// <param name="configure"></param>
        /// <param name="useResilience">
        /// When <see langword="true"/> (the default), previously registered resilience handlers are removed and
        /// replaced with the client's standard HTTP resilience pipeline with retry, timeout, circuit breaker and
        /// rate limiting. Register application-wide HTTP client defaults before calling this method.
        /// When <see langword="false"/>, this package neither removes nor adds resilience handlers, allowing the
        /// application to own the resilience pipeline.
        /// </param>
        /// <param name="configureBuilder">
        /// Optionally configures every SOS HTTP client. If this callback adds a resilience handler,
        /// <paramref name="useResilience"/> should be <see langword="false"/>.
        /// </param>
        /// <returns></returns>
    public static IServiceCollection AddSosClients(
        this IServiceCollection services,
        Action<SosClientOptions> configure,
        bool useResilience = true,
        Action<IHttpClientBuilder>? configureBuilder = null)
        {
            services
                .AddOptions<SosClientOptions>()
                .Configure(configure)
                .Validate(
                    options => options.RequestLoggingLevel == LogLevel.None ||
                               options.MaxLoggedRequestBodyLength > 0,
                    $"{nameof(SosClientOptions.MaxLoggedRequestBodyLength)} must be greater than zero when request logging is enabled.");
            services.TryAddTransient<SosRequestHeadersHandler>();
            services.TryAddSingleton(TimeProvider.System);
            services.AddMemoryCache();
            RegisterObservationsApiClient<ISosObservationsClient, SosObservationsClient>(
                services,
                useResilience,                
                configureBuilder);
            RegisterObservationsApiClient<ISosObservationAggregationsClient, SosObservationAggregationsClient>(
                services,
                useResilience,
                configureBuilder,
                longRunning: true);
            RegisterObservationsApiClient<ISosAreasClient, SosAreasClient>(
                services,
                useResilience,
                configureBuilder);
            RegisterObservationsApiClient<ISosProjectsClient, SosProjectsClient>(
                services,
                useResilience,
                configureBuilder);
            RegisterObservationsApiClient<ISosVocabulariesClient, SosVocabulariesClient>(
                services,
                useResilience,
                configureBuilder);
            RegisterObservationsApiClient<ISosDataProvidersClient, SosDataProvidersClient>(
                services,
                useResilience,
                configureBuilder);
            RegisterObservationsApiClient<ISosSystemsStatusInfoClient, SosSystemsStatusInfoClient>(
                services,
                useResilience,
                configureBuilder);
            RegisterObservationsApiClient<ISosDataStewardshipClient, SosDataStewardshipClient>(
                services,
                useResilience,
                configureBuilder);
            RegisterObservationsApiClient<ISosApiInfoClient, SosApiInfoClient>(
                services,
                useResilience,
                configureBuilder);
            RegisterObservationsApiClient<ISosExportsClient, SosExportsClient>(
                services,
                useResilience,
                configureBuilder,
                longRunning: true);
            RegisterAnalysisApiClient<ISosAnalysisClient, SosAnalysisClient>(
                services,
                useResilience,
                configureBuilder,
                longRunning: true);
            services.TryAddTransient<ISosObservationSearchService, SosObservationSearchService>();            
            services.TryAddTransient<ISosLookupService, SosLookupService>();
            services.TryAddTransient<ISosStatusInfoService, SosStatusInfoService>();
            services.TryAddTransient<ISosAnalysisService, SosAnalysisService>();
            services.TryAddTransient<ISosExportService, SosExportService>();
            services.TryAddTransient<ISosWorkspaceFilterService, SosWorkspaceFilterService>();
            services.TryAddTransient<SosClient>();
            return services;
        }

        /// <summary>
        /// Registers SOS clients for ASP.NET Core web applications.
        /// Automatically relays the current user's access token from HttpContext.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// builder.Services.AddSosWebClients(options =>
        /// {
        ///     options.ObservationsApiBaseAddress = new Uri("https://sos-search.artdata.slu.se");
        /// });        
        /// ]]></code>
        /// </example>
        /// <param name="services">Service collection to register clients in.</param>
        /// <param name="configure"></param>
        /// <param name="useResilience">
        /// When <see langword="true"/> (the default), previously registered resilience handlers are removed and
        /// replaced with the client's standard HTTP resilience pipeline with retry, timeout, circuit breaker and
        /// rate limiting. Register application-wide HTTP client defaults before calling this method.
        /// When <see langword="false"/>, this package neither removes nor adds resilience handlers, allowing the
        /// application to own the resilience pipeline.
        /// </param>
        /// <param name="configureBuilder">
        /// Optionally configures every SOS HTTP client. If this callback adds a resilience handler,
        /// <paramref name="useResilience"/> should be <see langword="false"/>.
        /// </param>
        /// <returns></returns>
    public static IServiceCollection AddSosWebClients(
        this IServiceCollection services,
        Action<SosClientOptions> configure,
        bool useResilience = true,
        Action<IHttpClientBuilder>? configureBuilder = null)
        {
            services.AddHttpContextAccessor();
            services.TryAddSingleton<ISosAccessTokenProvider, HttpContextAccessTokenProvider>();
            services.TryAddTransient<SosAuthorizationHandler>();

            return services.AddSosClients(configure, useResilience, builder =>
            {
                builder.AddHttpMessageHandler<SosAuthorizationHandler>();
                configureBuilder?.Invoke(builder);
            });
    }

    private static void RegisterObservationsApiClient<TClient, TImpl>(
        IServiceCollection services,
        bool useResilience,        
        Action<IHttpClientBuilder>? configureBuilder,
        bool longRunning = false)
        where TClient : class
        where TImpl : class, TClient
    {
        RegisterClient<TClient, TImpl>(
            services,
            options => options.ObservationsApiBaseAddress,
            nameof(SosClientOptions.ObservationsApiBaseAddress),
            useResilience,            
            configureBuilder,
            longRunning);
    }

    private static void RegisterAnalysisApiClient<TClient, TImpl>(
        IServiceCollection services,
        bool useResilience,        
        Action<IHttpClientBuilder>? configureBuilder,
        bool longRunning = false)
        where TClient : class
        where TImpl : class, TClient
    {
        RegisterClient<TClient, TImpl>(
            services,
            options => options.AnalysisApiBaseAddress,
            nameof(SosClientOptions.AnalysisApiBaseAddress),
            useResilience,
            configureBuilder,
            longRunning);
    }

    private static void RegisterClient<TClient, TImpl>(
        IServiceCollection services,
        Func<SosClientOptions, Uri?> baseAddressSelector,
        string baseAddressOptionName,
        bool useResilience,        
        Action<IHttpClientBuilder>? configureBuilder,
        bool longRunning = false)
        where TClient : class
        where TImpl : class, TClient
    {
        var builder = services.AddHttpClient<TClient, TImpl>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SosClientOptions>>().Value;
            var baseAddress = baseAddressSelector(options)
                ?? throw new InvalidOperationException(
                    $"{nameof(SosClientOptions)}.{baseAddressOptionName} must be configured to register {typeof(TImpl).Name}.");

            client.BaseAddress = NormalizeBaseAddress(baseAddress, baseAddressOptionName);

            client.Timeout = useResilience
                ? Timeout.InfiniteTimeSpan
                : longRunning ? options.LongRunningTimeout : options.Timeout;
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
            new HttpClientHandler
            {
                AutomaticDecompression =
                    DecompressionMethods.Brotli |
                    DecompressionMethods.GZip |
                    DecompressionMethods.Deflate,

                UseCookies = false
            })
        .AddHttpMessageHandler<SosRequestHeadersHandler>();

        if (useResilience)
        {
#pragma warning disable EXTEXP0001
            builder.RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001

            builder
                .AddStandardResilienceHandler()
                .Configure((resilienceOptions, serviceProvider) =>
                {
                    var sosOptions = serviceProvider
                        .GetRequiredService<IOptionsMonitor<SosClientOptions>>()
                        .CurrentValue;

                    ApplySosResilienceOptions(
                        resilienceOptions,
                        sosOptions.Resilience,
                        longRunning ? sosOptions.LongRunningTimeout : null);
                });
        }

        configureBuilder?.Invoke(builder);
    }

    private static Uri NormalizeBaseAddress(Uri baseAddress, string optionName)
    {
        if (!baseAddress.IsAbsoluteUri ||
            (baseAddress.Scheme != Uri.UriSchemeHttp && baseAddress.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                $"{nameof(SosClientOptions)}.{optionName} must be an absolute HTTP or HTTPS URI.");
        }

        if (!string.IsNullOrEmpty(baseAddress.Query) || !string.IsNullOrEmpty(baseAddress.Fragment))
        {
            throw new InvalidOperationException(
                $"{nameof(SosClientOptions)}.{optionName} must not include a query string or fragment.");
        }

        return baseAddress.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
            ? baseAddress
            : new Uri(baseAddress.AbsoluteUri + "/", UriKind.Absolute);
    }

    /// <summary>
    /// Maps <see cref="SosResilienceOptions"/> values onto Polly's <see cref="HttpStandardResilienceOptions"/>.
    /// </summary>
    private static void ApplySosResilienceOptions(
        HttpStandardResilienceOptions resilienceOptions,
        SosResilienceOptions sosResilience,
        TimeSpan? longRunningTimeout)
    {
        var totalTimeout = longRunningTimeout ?? sosResilience.TotalRequestTimeout;
        var attemptTimeout = longRunningTimeout ?? sosResilience.AttemptTimeout;
        resilienceOptions.TotalRequestTimeout.Timeout = totalTimeout;
        resilienceOptions.AttemptTimeout.Timeout = attemptTimeout;

        resilienceOptions.Retry.MaxRetryAttempts = sosResilience.MaxRetryAttempts;
        resilienceOptions.Retry.Delay = sosResilience.RetryDelay;
        resilienceOptions.Retry.MaxDelay = sosResilience.RetryMaxDelay;
        resilienceOptions.Retry.UseJitter = true;
        var defaultShouldHandle = resilienceOptions.Retry.ShouldHandle;
        resilienceOptions.Retry.ShouldHandle = arguments =>
        {
            var request = arguments.Context.GetRequestMessage();
            return request is not null && SosClientHttp.SuppressesRetries(request)
                ? ValueTask.FromResult(false)
                : defaultShouldHandle(arguments);
        };

        resilienceOptions.CircuitBreaker.FailureRatio = sosResilience.CircuitBreakerFailureRatio;
        resilienceOptions.CircuitBreaker.MinimumThroughput = sosResilience.CircuitBreakerMinimumThroughput;
        resilienceOptions.CircuitBreaker.BreakDuration = sosResilience.CircuitBreakerBreakDuration;
        var minimumSamplingDuration = TimeSpan.FromTicks(attemptTimeout.Ticks * 2);
        resilienceOptions.CircuitBreaker.SamplingDuration =
            sosResilience.CircuitBreakerSamplingDuration >= minimumSamplingDuration
                ? sosResilience.CircuitBreakerSamplingDuration
                : minimumSamplingDuration;
    }

}
