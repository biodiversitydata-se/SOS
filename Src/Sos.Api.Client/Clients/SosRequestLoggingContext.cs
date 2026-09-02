using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;

namespace SOS.Api.Client.Clients;

internal sealed class SosRequestLoggingContext
{
    private const string TruncationMarker = "...[truncated]";
    private const string RedactedValue = "[REDACTED]";

    private readonly ILogger? _logger;
    private readonly LogLevel _logLevel;
    private readonly int _maxBodyLength;

    private SosRequestLoggingContext()
    {
        _logLevel = LogLevel.None;
    }

    public SosRequestLoggingContext(ILogger logger, IOptions<SosClientOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        var value = options.Value;
        if (value.RequestLoggingLevel != LogLevel.None && value.MaxLoggedRequestBodyLength <= 0)
        {
            throw new OptionsValidationException(
                nameof(SosClientOptions),
                typeof(SosClientOptions),
                [$"{nameof(SosClientOptions.MaxLoggedRequestBodyLength)} must be greater than zero when request logging is enabled."]);
        }

        _logger = logger;
        _logLevel = value.RequestLoggingLevel;
        _maxBodyLength = value.MaxLoggedRequestBodyLength;
    }

    public static SosRequestLoggingContext Disabled { get; } = new();

    public void LogRequest(
        HttpClient httpClient,
        HttpRequestMessage request,
        string requestBody)
    {
        if (_logLevel == LogLevel.None || _logger is null || !_logger.IsEnabled(_logLevel))
        {
            return;
        }

        var loggedBody = CreateLoggedBody(requestBody, _maxBodyLength);
        var requestUri = CreateSafeRequestUri(httpClient.BaseAddress, request.RequestUri);

        SosRequestLog.Request(
            _logger,
            _logLevel,
            request.Method.Method,
            requestUri,
            requestBody.Length,
            loggedBody.WasReducedOrTruncated,
            loggedBody.Value);
    }

    internal static LoggedRequestBody CreateLoggedBody(string requestBody, int maxBodyLength)
    {
        ArgumentNullException.ThrowIfNull(requestBody);
        if (maxBodyLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBodyLength));
        }

        if (requestBody.Length <= maxBodyLength)
        {
            return new LoggedRequestBody(requestBody, false);
        }

        try
        {
            var root = JsonNode.Parse(requestBody);
            if (root is null)
            {
                return HardTruncate(requestBody, maxBodyLength);
            }

            ReplaceTaxonIds(root);
            var reducedBody = root.ToJsonString();
            if (reducedBody.Length <= maxBodyLength)
            {
                return new LoggedRequestBody(reducedBody, true);
            }

            ReplaceCoordinates(root);
            reducedBody = root.ToJsonString();
            return reducedBody.Length <= maxBodyLength
                ? new LoggedRequestBody(reducedBody, true)
                : HardTruncate(reducedBody, maxBodyLength);
        }
        catch
        {
            return HardTruncate(requestBody, maxBodyLength);
        }
    }

    internal static string CreateSafeRequestUri(Uri? baseAddress, Uri? requestUri)
    {
        if (requestUri is null)
        {
            return string.Empty;
        }

        string uriText;
        try
        {
            uriText = requestUri.IsAbsoluteUri
                ? requestUri.AbsoluteUri
                : baseAddress is null
                    ? requestUri.ToString()
                    : new Uri(baseAddress, requestUri).AbsoluteUri;
        }
        catch
        {
            uriText = requestUri.ToString();
        }

        try
        {
            return RedactSensitiveQueryParameters(uriText);
        }
        catch
        {
            return RemoveQuery(uriText);
        }
    }

    private static void ReplaceTaxonIds(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToArray())
            {
                if (property.Value is JsonObject taxonObject &&
                    property.Key.Equals("taxon", StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceArrayProperty(taxonObject, "ids");
                }

                if (property.Value is not null)
                {
                    ReplaceTaxonIds(property.Value);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    ReplaceTaxonIds(item);
                }
            }
        }
    }

    private static void ReplaceCoordinates(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToArray())
            {
                if (property.Value is JsonArray &&
                    property.Key.Equals("coordinates", StringComparison.OrdinalIgnoreCase))
                {
                    jsonObject[property.Key] = CreatePlaceholderArray();
                }
                else if (property.Value is not null)
                {
                    ReplaceCoordinates(property.Value);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    ReplaceCoordinates(item);
                }
            }
        }
    }

    private static void ReplaceArrayProperty(JsonObject jsonObject, string propertyName)
    {
        foreach (var property in jsonObject.ToArray())
        {
            if (property.Value is JsonArray &&
                property.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            {
                jsonObject[property.Key] = CreatePlaceholderArray();
            }
        }
    }

    private static JsonArray CreatePlaceholderArray()
    {
        var result = new JsonArray();
        result.Add(-1);
        return result;
    }

    private static LoggedRequestBody HardTruncate(string value, int maxLength)
    {
        if (maxLength <= TruncationMarker.Length)
        {
            return new LoggedRequestBody(TruncationMarker[..maxLength], true);
        }

        return new LoggedRequestBody(
            value[..(maxLength - TruncationMarker.Length)] + TruncationMarker,
            true);
    }

    private static string RedactSensitiveQueryParameters(string uri)
    {
        var queryStart = uri.IndexOf('?');
        if (queryStart < 0)
        {
            return uri;
        }

        var fragmentStart = uri.IndexOf('#', queryStart + 1);
        var queryEnd = fragmentStart >= 0 ? fragmentStart : uri.Length;
        var query = uri[(queryStart + 1)..queryEnd];
        var parts = query.Split('&');

        for (var index = 0; index < parts.Length; index++)
        {
            var separator = parts[index].IndexOf('=');
            var encodedName = separator >= 0 ? parts[index][..separator] : parts[index];
            if (IsSensitiveQueryParameter(encodedName))
            {
                parts[index] = $"{encodedName}={RedactedValue}";
            }
        }

        var suffix = fragmentStart >= 0 ? uri[fragmentStart..] : string.Empty;
        return $"{uri[..(queryStart + 1)]}{string.Join('&', parts)}{suffix}";
    }

    private static bool IsSensitiveQueryParameter(string encodedName)
    {
        if (encodedName.Equals("encryptPassword", StringComparison.OrdinalIgnoreCase) ||
            encodedName.Equals("confirmEncryptPassword", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var name = Uri.UnescapeDataString(encodedName.Replace('+', ' '));
        return name.Equals("encryptPassword", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("confirmEncryptPassword", StringComparison.OrdinalIgnoreCase);
    }

    private static string RemoveQuery(string uri)
    {
        var queryStart = uri.IndexOf('?');
        return queryStart < 0 ? uri : uri[..queryStart];
    }
}

internal static partial class SosRequestLog
{
    [LoggerMessage(
        EventId = 1000,
        EventName = "SosApiRequest",
        Message = "Sending SOS request {HttpMethod} {RequestUri}. Request body length: {OriginalRequestBodyLength}. Request body truncated: {RequestBodyTruncated}. Request body: {RequestBody}",
        SkipEnabledCheck = true)]
    public static partial void Request(
        ILogger logger,
        LogLevel level,
        string httpMethod,
        string requestUri,
        int originalRequestBodyLength,
        bool requestBodyTruncated,
        string requestBody);
}

internal readonly record struct LoggedRequestBody(string Value, bool WasReducedOrTruncated);
