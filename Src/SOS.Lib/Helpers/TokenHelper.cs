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
        string originalToken = token;
        try
        {
            token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (string.IsNullOrEmpty(token)) return false;
            //if (token.Count(c => c == '.') != 2)
            //{
            //    logger?.LogInformation("Invalid JWT format: {Token}", token);
            //    return false;
            //}

            var handler = new JsonWebTokenHandler();
            jwtToken = handler.ReadJsonWebToken(token);
            Uri tokenUri = new Uri(jwtToken.Issuer.TrimEnd('/'));
            Uri userAdmin2AuthUri = new Uri(userAdmin2AuthUrl.TrimEnd('/'));
            return Uri.Compare(tokenUri, userAdmin2AuthUri, UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0;
        }
        catch (Exception ex)
        {
            if (logger != null)
                logger.LogError(ex, "Failed to compare token issuer '{@issuer}' with expected authority '{@authority}'. Token={token}", jwtToken?.Issuer, userAdmin2AuthUrl, originalToken);
        }

        return false;
    }
}
