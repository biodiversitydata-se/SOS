using SOS.Lib.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.Extensions;

internal static class JsonSerializationExtensions
{
    internal static WebApplicationBuilder SetupJsonSerialization(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.AllowTrailingCommas = true;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.Converters.Add(new GeoShapeConverter());
        });

        return webApplicationBuilder;
    }
}
