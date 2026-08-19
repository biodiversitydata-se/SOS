using SOS.Api.Client.Exceptions;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Api.Client.Clients;

internal static class SosClientHttp
{
    private const int ResponsePreviewSizeBytes = 4 * 1024;
    private static readonly HttpRequestOptionsKey<bool> SuppressDefaultAcceptHeaderOption =
        new("SOS.Api.Client.SuppressDefaultAcceptHeader");
    private static readonly HttpRequestOptionsKey<bool> SuppressRetriesOption =
        new("SOS.Api.Client.SuppressRetries");

    public const string AuthorizationRoleIdHeaderName = "X-Authorization-Role-Id";
    public const string AuthorizationApplicationIdentifierHeaderName =
        "X-Authorization-Application-Identifier";
    public const string RequestingSystemHeaderName = "X-Requesting-System";

    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, SerializerOptions);
    }

    public static StringContent CreateJsonContent(string json)
    {
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static void ThrowIfEmptyJson(string json, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Filter JSON must not be empty.", parameterName);
        }
    }

    public static void ApplySosRequestHeaders(
        this HttpRequestMessage request,
        int? roleId = null,
        string? authorizationApplicationIdentifier = null,
        SosAcceptProfile acceptProfile = SosAcceptProfile.Json)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Headers.Accept.Clear();
        request.Options.Set(
            SuppressDefaultAcceptHeaderOption,
            acceptProfile == SosAcceptProfile.Any);

        switch (acceptProfile)
        {
            case SosAcceptProfile.Json:
                AddAccept(request, "application/json");
                break;
            case SosAcceptProfile.GeoJson:
                AddAccept(request, "application/geo+json");
                AddAccept(request, "application/json");
                break;
            case SosAcceptProfile.TextOrJson:
                AddAccept(request, "text/plain");
                AddAccept(request, "application/json");
                break;
            case SosAcceptProfile.Any:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(acceptProfile), acceptProfile, null);
        }

        if (roleId.HasValue)
        {
            SetHeader(
                request,
                AuthorizationRoleIdHeaderName,
                roleId.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrWhiteSpace(authorizationApplicationIdentifier))
        {
            SetHeader(
                request,
                AuthorizationApplicationIdentifierHeaderName,
                authorizationApplicationIdentifier);
        }
    }

    public static bool SuppressesDefaultAcceptHeader(HttpRequestMessage request) =>
        request.Options.TryGetValue(SuppressDefaultAcceptHeaderOption, out var suppress) && suppress;

    public static void SuppressRetries(this HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Options.Set(SuppressRetriesOption, true);
    }

    public static bool SuppressesRetries(HttpRequestMessage request) =>
        request.Options.TryGetValue(SuppressRetriesOption, out var suppress) && suppress;

    public static void ApplyDefaultAcceptHeader(HttpRequestMessage request)
    {
        if (!SuppressesDefaultAcceptHeader(request) && !request.Headers.Accept.Any())
        {
            AddAccept(request, "application/json");
        }
    }

    public static void AddHeaderIfMissing(
        HttpRequestMessage request,
        string name,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && !request.Headers.Contains(name))
        {
            request.Headers.TryAddWithoutValidation(name, value);
        }
    }

    private static void AddAccept(HttpRequestMessage request, string mediaType) =>
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

    private static void SetHeader(HttpRequestMessage request, string name, string value)
    {
        request.Headers.Remove(name);
        request.Headers.TryAddWithoutValidation(name, value);
    }

    public static async Task<string> ReadResponseBodyAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public static async Task<string> ReadErrorResponsePreviewAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream(ResponsePreviewSizeBytes);
        var remaining = ResponsePreviewSizeBytes;
        var bytes = new byte[Math.Min(1024, ResponsePreviewSizeBytes)];
        while (remaining > 0)
        {
            var read = await responseStream.ReadAsync(
                bytes.AsMemory(0, Math.Min(bytes.Length, remaining)),
                cancellationToken);
            if (read == 0)
            {
                break;
            }

            await buffer.WriteAsync(bytes.AsMemory(0, read), cancellationToken);
            remaining -= read;
        }

        return Encoding.UTF8.GetString(buffer.GetBuffer(), 0, checked((int)buffer.Length));
    }

    public static async Task<T?> ReadJsonResponseAsync<T>(
        HttpResponseMessage response,
        Uri? requestUri,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await ReadErrorResponsePreviewAsync(response, cancellationToken);
            throw CreateApiException(response, responseBody, requestUri);
        }

        if (response.StatusCode == HttpStatusCode.NoContent ||
            response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var previewStream = new JsonResponsePreviewStream(
            responseStream,
            ResponsePreviewSizeBytes);

        try
        {
            return await JsonSerializer.DeserializeAsync<T>(
                previewStream,
                SerializerOptions,
                cancellationToken);
        }
        catch (JsonException ex)
        {
            await previewStream.DrainAsync(cancellationToken);
            if (!previewStream.HasNonWhitespaceContent)
            {
                return default;
            }

            throw new SosInvalidResponseException(
                "SOS API response body could not be deserialized.",
                response.StatusCode,
                previewStream.GetPreview(),
                requestUri,
                ex);
        }
    }

    public static async Task<string> ReadStringResponseAsync(
        HttpResponseMessage response,
        Uri? requestUri,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var responsePreview = await ReadErrorResponsePreviewAsync(response, cancellationToken);
            throw CreateApiException(response, responsePreview, requestUri);
        }

        var body = await ReadResponseBodyAsync(response, cancellationToken);

        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        try
        {
            return JsonSerializer.Deserialize<string>(body, SerializerOptions) ?? string.Empty;
        }
        catch (JsonException)
        {
            return body;
        }
    }

    public static SosApiException CreateApiException(
        HttpResponseMessage response,
        string responseBody,
        Uri? requestUri)
    {
        var message = $"SOS API request failed with HTTP {(int)response.StatusCode} ({response.StatusCode}).";
        return new SosApiException(message, response.StatusCode, responseBody, requestUri);
    }

    public static SosInvalidResponseException CreateInvalidResponseException(
        HttpResponseMessage response,
        Uri? requestUri,
        string? responseBodyPreview = null)
    {
        var message = $"SOS API returned HTTP {(int)response.StatusCode} ({response.StatusCode}) without the expected response body.";
        return new SosInvalidResponseException(
            message,
            response.StatusCode,
            responseBodyPreview,
            requestUri);
    }
}

internal enum SosAcceptProfile
{
    Json,
    GeoJson,
    TextOrJson,
    Any
}
