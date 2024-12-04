using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using System;

namespace SOS.Lib.Helpers;
public static class TokenHelper
{
    public static bool IsUserAdmin2Token(string token, string userAdmin2AuthUrl, ILogger logger = null)
    {
        if (string.IsNullOrEmpty(token)) return false;
        JsonWebToken jwtToken = null;

        try
        {
            token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            var handler = new JsonWebTokenHandler();
            jwtToken = handler.ReadJsonWebToken(token);
            Uri tokenUri = new Uri(jwtToken.Issuer.TrimEnd('/'));
            Uri userAdmin2AuthUri = new Uri(userAdmin2AuthUrl.TrimEnd('/'));
            return Uri.Compare(tokenUri, userAdmin2AuthUri, UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0;
        }
        catch (Exception ex)
        {
            if (logger != null)
                logger.LogError(ex, "Failed to compare token issuer '{@issuer}' with expected authority '{@authority}'.", jwtToken?.Issuer, userAdmin2AuthUrl);
        }

        return false;
    }
}
