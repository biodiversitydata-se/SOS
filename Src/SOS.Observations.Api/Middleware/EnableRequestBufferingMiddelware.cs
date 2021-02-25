using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SOS.Observations.Api.Middleware
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
