using Microsoft.Extensions.Options;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Clients;
using System.Globalization;

namespace SOS.Api.Client.DependencyInjection.Handlers;

/// <summary>
/// Completes the standard headers for outgoing SOS API requests using the selected role
/// and configured application defaults. Explicit request headers take precedence.
/// </summary>
internal sealed class SosRequestHeadersHandler : DelegatingHandler
{
    private readonly ISosRoleIdProvider? _roleIdProvider;
    private readonly IOptions<SosClientOptions> _options;

    public SosRequestHeadersHandler(
        IEnumerable<ISosRoleIdProvider> roleIdProviders,
        IOptions<SosClientOptions> options)
    {
        var registeredProviders = roleIdProviders.Take(2).ToArray();
        if (registeredProviders.Length > 1)
        {
            throw new InvalidOperationException(
                $"Only one {nameof(ISosRoleIdProvider)} may be registered.");
        }

        _roleIdProvider = registeredProviders.SingleOrDefault();
        _options = options;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        SosClientHttp.AddHeaderIfMissing(
            request,
            SosClientHttp.AuthorizationRoleIdHeaderName,
            _roleIdProvider?.GetCurrentRoleId()?.ToString(CultureInfo.InvariantCulture));

        var clientOptions = _options.Value;

        SosClientHttp.AddHeaderIfMissing(
            request,
            SosClientHttp.AuthorizationApplicationIdentifierHeaderName,
            clientOptions.ApplicationIdentifier);
        SosClientHttp.AddHeaderIfMissing(
            request,
            SosClientHttp.RequestingSystemHeaderName,
            clientOptions.SystemName);
        SosClientHttp.ApplyDefaultAcceptHeader(request);

        return base.SendAsync(request, cancellationToken);
    }
}
