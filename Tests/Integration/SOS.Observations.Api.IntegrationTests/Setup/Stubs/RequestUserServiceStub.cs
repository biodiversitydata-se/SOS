using Microsoft.IdentityModel.JsonWebTokens;
using SOS.Lib.Models.UserService;
using SOS.Lib.Services.Interfaces;

namespace SOS.Observations.Api.IntegrationTests.Setup.Stubs;

/// <summary>
/// Routes calls to the stub selected by the HTTP client. The router can be shared
/// by singleton services without capturing another client's permissions.
/// </summary>
internal sealed class RequestUserServiceStub(
    IHttpContextAccessor contextAccessor,
    IReadOnlyDictionary<string, IUserService> stubs) : IUserService
{
    internal const string HeaderName = "X-Test-User-Service";

    private IUserService Current
    {
        get
        {
            var key = contextAccessor.HttpContext?.Request.Headers[HeaderName].ToString();
            return key != null && stubs.TryGetValue(key, out var stub)
                ? stub
                : throw new InvalidOperationException("The request must select a registered user service stub.");
        }
    }

    public Task<UserModel> GetUserAsync() => Current.GetUserAsync();

    public Task<UserModel> GetUserByIdAsync(int userId) => Current.GetUserByIdAsync(userId);

    public Task<IEnumerable<AuthorityModel>> GetUserAuthoritiesAsync(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE") =>
        Current.GetUserAuthoritiesAsync(userId, authorizationApplicationIdentifier, cultureCode);

    public Task<IEnumerable<RoleModel>> GetUserRolesAsync(int userId, string authorizationApplicationIdentifier = null, string cultureCode = "sv-SE") =>
        Current.GetUserRolesAsync(userId, authorizationApplicationIdentifier, cultureCode);

    public Task<PersonModel> GetPersonAsync(int personId, string cultureCode = "sv-SE") => Current.GetPersonAsync(personId, cultureCode);

    public Task<JsonWebToken> GetClientCredentialsAccessTokenAsync() => Current.GetClientCredentialsAccessTokenAsync();
}
