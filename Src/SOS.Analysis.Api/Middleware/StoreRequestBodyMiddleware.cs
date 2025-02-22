using Nest;
using SOS.Lib.JsonConverters;
using SOS.Shared.Api.Dtos.Filter;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.Analysis.Api.Middleware;

public class StoreRequestBodyMiddleware
{
    private readonly RequestDelegate _next;
    private const int ApplicationInsightsMaxSize = 8192;
    private readonly static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = {
                new JsonStringEnumConverter(),
                new GeoShapeConverter(),
                new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
            }
    };

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

                var body = await streamReader.ReadToEndAsync();

                // Rewind, so the core is not lost when it looks the body for the request
                context.Request.Body.Position = 0;

                if (body != null && body.Length > ApplicationInsightsMaxSize)
                {
                    // truncate the body to the max size
                    try
                    {
                        var searchFilter = JsonSerializer.Deserialize<SearchFilterInternalDto>(body, jsonSerializerOptions);
                        if (searchFilter?.Taxon?.Ids != null && searchFilter.Taxon.Ids.Any())
                        {
                            searchFilter.Taxon.Ids = [-1];
                            body = JsonSerializer.Serialize(searchFilter, jsonSerializerOptions);
                        }

                        if (body.Length > ApplicationInsightsMaxSize && searchFilter?.Geographics?.Geometries != null && searchFilter.Geographics.Geometries.Any())
                        {
                            searchFilter.Geographics.Geometries = [new PointGeoShape(new GeoCoordinate(-1, -1))];
                            body = JsonSerializer.Serialize(searchFilter, jsonSerializerOptions);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Deserialization failed: {ex.Message}");
                    }
                }

                context.Items.Add("Request-body", body);
            }
        }
        finally
        {
            await _next(context);
        }
    }
}