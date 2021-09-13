using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SOS.Observations.Api.IntegrationTests.Extensions
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
    }
}