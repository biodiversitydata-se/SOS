using SOS.Lib.Extensions;
using System.Text;
using System.Text.RegularExpressions;

namespace SOS.Analysis.Api.Middleware;

public class StoreRequestBodyMiddleware
{
    private readonly RequestDelegate _next;
    private const int ApplicationInsightsMaxSize = 8192;

    public StoreRequestBodyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            if (new[] { "post", "put" }.Contains(context.Request.Method, StringComparer.CurrentCultureIgnoreCase) &&
                !context.Items.ContainsKey("Request-body") && context.Request.Body.CanRead && context.Request.Body.CanSeek)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);

                using var streamReader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true);

                string body = await streamReader.ReadToEndAsync();

                // Rewind, so the core is not lost when it looks the body for the request
                context.Request.Body.Position = 0;

                if (body != null && body.Length > ApplicationInsightsMaxSize)
                {
                    body = TruncateRequestBody(body);
                }

                context.Items.Add("Request-body", body);
            }
        }
        finally
        {
            await _next(context);
        }
    }

    private string TruncateRequestBody(string body)
    {
        // truncate the body to the max size
        try
        {
            body = body.CleanNewLineTab();
            if (body.Length < ApplicationInsightsMaxSize) return body;
            body = ReplaceTaxonIdsWithPlaceholder(body);
            if (body.Length < ApplicationInsightsMaxSize) return body;
            body = ReplaceCoordinatesWithPlaceholder(body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deserialization failed: {ex.Message}");
        }

        return body;
    }

    private static readonly Regex TaxonBlockRegex = new Regex(
        @"""taxon""\s*:\s*\{[^{}]*?""ids""\s*:\s*\[[^\]]*?\]",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex IdsArrayRegex = new Regex(
        @"""ids""\s*:\s*\[[^\]]*?\]",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex CoordinatesRegex = new Regex(
        @"""coordinates""\s*:\s*\[\s*(\[\s*(\[\s*[^]]*?\s*\]\s*,?\s*)+\s*\])\s*\]",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    static string ReplaceTaxonIdsWithPlaceholder(string json)
    {
        return TaxonBlockRegex.Replace(json, match =>
        {
            return IdsArrayRegex.Replace(match.Value, @"""ids"":[-1]");
        });
    }

    static string ReplaceCoordinatesWithPlaceholder(string json)
    {
        return CoordinatesRegex.Replace(json, @"""coordinates"":[-1]");
    }
}