using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace SOS.Api.Client.DependencyInjection.Authentication;

/// <summary>
/// Reads the current access token through <see cref="IHttpContextAccessor"/> for every outgoing request.
/// </summary>
/// <param name="httpContextAccessor"></param>
/// <param name="options"></param>
internal sealed class HttpContextAccessTokenProvider(
    IHttpContextAccessor httpContextAccessor,
    IOptions<SosClientOptions> options) : ISosAccessTokenProvider
{
    public async ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null) return null;
        return await context.GetTokenAsync(options.Value.AccessTokenName);
    }
}
