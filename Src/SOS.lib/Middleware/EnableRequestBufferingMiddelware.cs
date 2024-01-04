﻿using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SOS.Lib.Middleware
{
    public class EnableRequestBufferingMiddelware
    {
        private readonly RequestDelegate _next;

        public EnableRequestBufferingMiddelware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();
            await _next(context);
        }
    }
}
