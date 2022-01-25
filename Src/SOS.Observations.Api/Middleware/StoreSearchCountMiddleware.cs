using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SOS.Observations.Api.Middleware
{
    public class StoreSearchCountMiddleware
    {
        private readonly RequestDelegate _next;

        private object TryGetResponseCount(HttpContext context, string responseBody)
        {
            var match  = Regex.Match(context.Request.Path.Value, @"([^\/]+)$");
            switch (match?.Value?.ToLower())
            {
                case "cachedcount":
                case "taxonexistsindication":
                    return JsonDocument.Parse(responseBody).RootElement.EnumerateArray()
                        .Sum(i => double.Parse(i.GetProperty("observationCount").ToString()));
                case "count":
                    return responseBody;
                case "search":
                case "searchscroll":
                case "searchaggregated":
                case "taxonaggregation":
                    return JsonDocument.Parse(responseBody).RootElement.GetProperty("records").GetArrayLength();
                case "geogridaggregation":
                case "metricgridaggregation":
                    return JsonDocument.Parse(responseBody).RootElement.GetProperty("gridCellCount").ToString();
                case "geogridtaxaaggregation":
                    return JsonDocument.Parse(responseBody).RootElement.GetProperty("gridCells").GetArrayLength();
            }
          
            return null;
        }

        public StoreSearchCountMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/Observations", StringComparison.CurrentCultureIgnoreCase) ||
                  context.Request.Method.ToLower() != "post")
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;

            try
            {
                await using var memStream = new MemoryStream();

                context.Response.Body = memStream;

                await _next(context);

                memStream.Position = 0;
                var responseBody = new StreamReader(memStream).ReadToEnd();

                if (context.Response.StatusCode == 200 && !string.IsNullOrEmpty(responseBody))
                {
                    context.Items.Add("Response-count", TryGetResponseCount(context, responseBody));
                }

                memStream.Position = 0;
                await memStream.CopyToAsync(originalBody);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
    }
}
