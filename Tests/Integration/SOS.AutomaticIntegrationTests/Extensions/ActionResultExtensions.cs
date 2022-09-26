using Microsoft.AspNetCore.Mvc;
using SOS.Lib.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOS.AutomaticIntegrationTests.Extensions
{
    public static class ActionResultExtensions
    {
        public static T GetResult<T>(this IActionResult response)
        {
            var okObjectResult = (OkObjectResult)response;
            /*  var strJson = JsonConvert.SerializeObject(okObjectResult.Value);


              var result = JsonConvert.DeserializeObject<T>(strJson );*/
            var jsonString = JsonSerializer.Serialize(okObjectResult.Value);

            var options = new JsonSerializerOptions { 
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new GeoShapeConverter());
            options.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
            options.Converters.Add(new JsonStringEnumConverter());
           
            return JsonSerializer.Deserialize<T>(jsonString, options)!;
        }

        public static byte[] GetFileContentResult(this IActionResult response)
        {
            var fileContentResult = (FileContentResult)response;
            return fileContentResult.FileContents;
        }

        public static T GetResultObject<T>(this IActionResult response)
        {
            var okObjectResult = (OkObjectResult)response;
            return (T)okObjectResult.Value;
        }
    }
}