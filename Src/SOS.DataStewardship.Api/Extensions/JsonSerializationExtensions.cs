using SOS.Lib.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.DataStewardship.Api.Extensions;

internal static class JsonSerializationExtensions
{
    extension(WebApplicationBuilder webApplicationBuilder)
    {
        internal WebApplicationBuilder SetupJsonSerialization()
        {
            webApplicationBuilder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.AllowTrailingCommas = true;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
                options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.Converters.Add(new GeometryConverter());
                options.SerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
            });

            return webApplicationBuilder;
        }
    }
}
