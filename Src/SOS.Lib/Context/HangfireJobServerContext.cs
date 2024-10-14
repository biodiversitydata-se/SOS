using Microsoft.Extensions.Hosting;

namespace SOS.Lib.Context
{
    public static class HangfireJobServerContext
    {
        public static IHost Host { get; set; }
    }
}