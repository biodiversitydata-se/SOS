using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using SOS.Lib.JsonConverters;

namespace SOS.Observations.Api.Controllers
{    
    public class WriteJsonNullValuesAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext ctx)
        {
            if (ctx.Result is ObjectResult objectResult)
            {
                objectResult.Formatters.Add(new SystemTextJsonOutputFormatter(new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    Converters =
                    {
                        new JsonStringEnumConverter(),
                        new GeoShapeConverter(),
                        new NetTopologySuite.IO.Converters.GeoJsonConverterFactory() // Is this needed?
                    }
                }));                        
            }
        }
    }   
}