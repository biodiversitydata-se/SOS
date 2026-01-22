using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace SOS.Status.Web.Cache;

public class ConfigureCookieAuthOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
{
    private readonly ITicketStore _ticketStore;

    public ConfigureCookieAuthOptions(ITicketStore ticketStore)
    {
        _ticketStore = ticketStore;
    }

    // Called for named options; null name means default
    public void Configure(string? name, CookieAuthenticationOptions options)
    {
        if (name != CookieAuthenticationDefaults.AuthenticationScheme) return;
        options.SessionStore = _ticketStore;
    }

    // IConfigureNamedOptions requires this overload too
    public void Configure(CookieAuthenticationOptions options) => Configure(Options.DefaultName, options);
}
