using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SOS.AutomaticIntegrationTests.Extensions
{
    public static class ActionResultExtensions
    {
        public static T GetResult<T>(this IActionResult response)
        {
            var okObjectResult = (OkObjectResult)response;
            var strJson = JsonConvert.SerializeObject(okObjectResult.Value);
            var result = JsonConvert.DeserializeObject<T>(strJson);
            return result;
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