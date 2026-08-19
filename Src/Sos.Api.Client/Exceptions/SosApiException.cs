using System.Net;

namespace SOS.Api.Client.Exceptions;

/// <summary>
/// Represents an unsuccessful SOS API response.
/// </summary>
public sealed class SosApiException : Exception
{
    /// <summary>Creates an exception from an unsuccessful SOS API response.</summary>
    public SosApiException(
        string message,
        HttpStatusCode statusCode,
        string? responseBody = null,
        Uri? requestUri = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        RequestUri = requestUri;
    }

    /// <summary>The HTTP status code returned by the API.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// A bounded preview of the unsuccessful HTTP response body.
    /// </summary>
    public string? ResponseBody { get; }

    /// <summary>The request URI, when available.</summary>
    public Uri? RequestUri { get; }
}
