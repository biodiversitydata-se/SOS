using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SOS.Lib.Services.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SOS.Lib.Services;

public static class JsonSerializationHelper
{
    /// <summary>
    /// Serialization options
    /// </summary>
    public static JsonSerializerOptions SerializerOptions
    {
        get
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter(null));

            return options;
        }
    }
}

/// <inheritdoc />
public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpClientService> _logger;

    /// <summary>
    /// Named client for standard requests. Register this name via <c>services.AddHttpClient(HttpClientService.DefaultClient)</c>.
    /// </summary>
    public const string DefaultClient = "SOS.Default";

    /// <summary>
    /// Named client that skips TLS certificate validation (used by <see cref="GetFileStreamAsync"/>).
    /// Register this name via <c>services.AddHttpClient(HttpClientService.InsecureClient)</c>.
    /// </summary>
    public const string InsecureClient = "SOS.Insecure";

    private HttpClient CreateClient(IDictionary<string, string> headerData = null, bool insecure = false)
    {
        var httpClient = _httpClientFactory.CreateClient(insecure ? InsecureClient : DefaultClient);

        if (headerData?.Any() != true)
        {
            return httpClient;
        }

        var userName = headerData
            .FirstOrDefault(hd => hd.Key.Equals("username", StringComparison.OrdinalIgnoreCase)).Value;
        var password = headerData
            .FirstOrDefault(hd => hd.Key.Equals("password", StringComparison.OrdinalIgnoreCase)).Value;

        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                AuthenticationSchemes.Basic.ToString(),
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}")));
        }

        var personalAccessToken = headerData
            .FirstOrDefault(hd => hd.Key.Equals("personalAccessToken", StringComparison.OrdinalIgnoreCase)).Value;
        if (!string.IsNullOrEmpty(personalAccessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($":{personalAccessToken}")));
        }

        foreach (var data in headerData.Where(hd =>
            !hd.Key.Equals("username", StringComparison.OrdinalIgnoreCase) &&
            !hd.Key.Equals("password", StringComparison.OrdinalIgnoreCase)))
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(data.Key, data.Value);
        }

        return httpClient;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="logger"></param>
    public HttpClientService(IHttpClientFactory httpClientFactory, ILogger<HttpClientService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void Dispose() { /* HttpClient lifetime is managed by IHttpClientFactory */ }

    /// <inheritdoc />
    public async Task<T> GetDataAsync<T>(Uri requestUri)
    {
        return await GetDataAsync<T>(requestUri, null);
    }

    /// <inheritdoc />
    public async Task<T> GetDataAsync<T>(
        Uri requestUri,
        IDictionary<string, string> headerData,
        TimeSpan? timeout = null,
        int? retryCount = null)
    {
        var httpClient = CreateClient(headerData);

        try
        {
            HttpResponseMessage response;

            if (timeout.HasValue || retryCount is > 0)
            {
                var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();

                if (retryCount is > 0)
                {
                    builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                    {
                        MaxRetryAttempts = retryCount.Value,
                        Delay = TimeSpan.FromSeconds(1),
                        BackoffType = DelayBackoffType.Linear,
                        ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<Exception>(),
                        OnRetry = args =>
                        {
                            _logger.LogWarning(args.Outcome.Exception,
                                "Retry {Retry} after {Delay}s for request {Uri}",
                                args.AttemptNumber + 1, args.RetryDelay.TotalSeconds, requestUri);
                            return ValueTask.CompletedTask;
                        }
                    });
                }

                if (timeout.HasValue)
                {
                    builder.AddTimeout(timeout.Value);
                }

                var pipeline = builder.Build();
                response = await pipeline.ExecuteAsync(async ct =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    return await httpClient.SendAsync(request, ct);
                });
            }
            else
            {
                response = await httpClient.GetAsync(requestUri);
            }

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "HttpClient.GetDataAsync() failed. RequestUri={RequestUri}, StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}, ResponseBody={ResponseBody}",
                    requestUri.AbsolutePath, response.StatusCode, response.ReasonPhrase, responseBody);
                return default;
            }

            return await JsonSerializer.DeserializeAsync<T>(
                await response.Content.ReadAsStreamAsync(),
                JsonSerializationHelper.SerializerOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HttpClient.GetDataAsync() failed. RequestUri={RequestUri}", requestUri.AbsolutePath);
        }

        return default;
    }

    /// <inheritdoc />
    public async Task<Stream> GetFileStreamAsync(Uri requestUri, IDictionary<string, string> headerData = null)
    {
        var httpClient = CreateClient(headerData, insecure: true);
        var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);
        return response.StatusCode == HttpStatusCode.OK ? await response.Content.ReadAsStreamAsync() : null;
    }

    /// <inheritdoc />
    public async Task<T> PostDataAsync<T>(Uri requestUri, object model)
    {
        return await PostDataAsync<T>(requestUri, model, null, "application/json");
    }

    /// <inheritdoc />
    public async Task<T> PostDataAsync<T>(Uri requestUri, object model, IDictionary<string, string> headerData)
    {
        return await PostDataAsync<T>(requestUri, model, headerData, "application/json");
    }

    /// <inheritdoc />
    public async Task<T> PostDataAsync<T>(Uri requestUri, object model, IDictionary<string, string> headerData, string contentType)
    {
        var httpClient = CreateClient(headerData);
        try
        {
            var httpResponseMessage = await httpClient.PostAsync(requestUri, new JsonContent(model, contentType));
            httpResponseMessage.EnsureSuccessStatusCode();
            return await JsonSerializer.DeserializeAsync<T>(
                await httpResponseMessage.Content.ReadAsStreamAsync(),
                JsonSerializationHelper.SerializerOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post to {RequestUri} failed", requestUri.AbsolutePath);
        }

        return default;
    }

    /// <inheritdoc />
    public async Task<T> PutDataAsync<T>(Uri requestUri, object model)
    {
        return await PutDataAsync<T>(requestUri, model, null, "application/json");
    }

    /// <inheritdoc />
    public async Task<T> PutDataAsync<T>(Uri requestUri, object model, IDictionary<string, string> headerData)
    {
        return await PutDataAsync<T>(requestUri, model, headerData, "application/json");
    }

    /// <inheritdoc />
    public async Task<T> PutDataAsync<T>(Uri requestUri, object model, IDictionary<string, string> headerData, string contentType)
    {
        var httpClient = CreateClient(headerData);
        try
        {
            var httpResponseMessage = await httpClient.PutAsync(requestUri, new JsonContent(model, contentType));
            httpResponseMessage.EnsureSuccessStatusCode();
            return await JsonSerializer.DeserializeAsync<T>(
                await httpResponseMessage.Content.ReadAsStreamAsync(),
                JsonSerializationHelper.SerializerOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HttpClient.PutDataAsync() failed. RequestUri={RequestUri}", requestUri.AbsolutePath);
        }

        return default;
    }

    /// <inheritdoc />
    public async Task<T> DeleteDataAsync<T>(Uri requestUri)
    {
        var httpClient = CreateClient();
        try
        {
            var httpResponseMessage = await httpClient.DeleteAsync(requestUri);
            return await JsonSerializer.DeserializeAsync<T>(
                await httpResponseMessage.Content.ReadAsStreamAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HttpClient.DeleteDataAsync() failed. RequestUri={RequestUri}", requestUri.AbsolutePath);
        }

        return default;
    }
}

/// <summary>
///     Create json content
/// </summary>
public class JsonContent : StringContent
{
    /// <summary>
    ///  Serialize object to json
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="contentType"></param>
    public JsonContent(object obj, string contentType) :
        base(JsonSerializer.Serialize(obj, JsonSerializationHelper.SerializerOptions), Encoding.UTF8, contentType)
    {
    }
}
