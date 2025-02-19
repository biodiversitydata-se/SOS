﻿using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Middleware
{
    public class StoreRequestBodyMiddleware
    {
        private readonly RequestDelegate _next;

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

                    context.Items.Add("Request-body", body);
                }
            }
            finally
            {
                await _next(context);
            }
        }
    }
}
