using System.Net.Http.Headers;

namespace SOS.Api.Client.DependencyInjection.Authentication;

/// <summary>
/// DelegatingHandler that adds the access token from ISosAccessTokenProvider to outgoing requests.
/// </summary>
/// <param name="tokenProvider"></param>
internal sealed class SosAuthorizationHandler(ISosAccessTokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        if (!string.IsNullOrEmpty(token) && request.Headers.Authorization is null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
