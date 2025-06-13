namespace SOS.Status.Web.Managers;

using Microsoft.AspNetCore.Authentication;

public class TokenAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return null;

        var result = await context.AuthenticateAsync("Cookies");

        if (result.Succeeded && result?.Properties?.Items != null)
        {
            result.Properties.Items.TryGetValue(".Token.access_token", out var token);
            return token;
        }

        return null;
    }
}
