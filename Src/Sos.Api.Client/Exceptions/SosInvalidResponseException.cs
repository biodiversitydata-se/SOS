using System.Net;

namespace SOS.Api.Client.Exceptions;

/// <summary>
/// Represents a successful SOS API response whose body does not match the expected contract.
/// </summary>
public sealed class SosInvalidResponseException : Exception
{
    /// <summary>Creates an exception for a successful response with an invalid body.</summary>
    public SosInvalidResponseException(
        string message,
        HttpStatusCode statusCode,
        string? responseBodyPreview = null,
        Uri? requestUri = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBodyPreview = responseBodyPreview;
        RequestUri = requestUri;
    }

    /// <summary>The HTTP status code returned by the API.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>A bounded preview of the invalid response body.</summary>
    public string? ResponseBodyPreview { get; }

    /// <summary>The request URI, when available.</summary>
    public Uri? RequestUri { get; }
}
