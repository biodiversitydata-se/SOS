using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SOS.ContainerIntegrationTests.Stubs;
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string UserId = "UserId";
    public const string AuthenticationScheme = "Bearer";
    public const int DefaultTestUserId = 15;

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "Test user") };

        // Extract User ID from the request headers if it exists,
        // otherwise use the default User ID from the options.
        if (Context.Request.Headers.TryGetValue(UserId, out var userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId[0]));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, DefaultTestUserId.ToString()));
        }
        //claims.Add(new Claim(ClaimTypes.Email, _defaultUserId));
        claims.Add(new Claim("scope", "SOS.Observations.Protected"));



        //var claims = new[]
        //{
        //    new Claim(ClaimTypes.Name, "Test user"),
        //    new Claim(ClaimTypes.NameIdentifier, DefaultTestUserId.ToString()),
        //    new Claim("scope", "SOS.Observations.Protected")
        //};        
        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);        

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}