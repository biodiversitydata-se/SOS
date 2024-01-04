namespace SOS.DataStewardship.Api.IntegrationTests.Core.Setup;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "Bearer";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test user"),
            new Claim(ClaimTypes.NameIdentifier, "1337")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, $"{AuthenticationScheme}Scheme");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}