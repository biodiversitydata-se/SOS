using NLog.Web;

namespace SOS.Hangfire.JobServer
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting.Internal;

    namespace MyApplication.Common
    {
        // https://stackoverflow.com/questions/39573571/net-core-console-application-how-to-configure-appsettings-per-environment
        /*
         * <PackageReference I="Microsoft.Extensions.Hosting.Abstractions" Version="2.1.1" />
         * <PackageReference I="Microsoft.AspNetCore.Hosting.Abstractions" Version="2.1.1" />
         
         * <PackageReference I="Microsoft.Extensions.Configuration" Version="2.1.1" />
         * <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="2.1.1" />
         * <PackageReference I="Microsoft.Extensions.Configuration.Json" Version="2.1.1" />
         * <PackageReference I="Microsoft.Extensions.Hosting" Version="2.1.1" />
         
         */
        public static class ConfigurationFactory
        {
            /// <summary>
            /// Use for ASP.NET Core Web applications.
            /// </summary>
            /// <param name="config"></param>
            /// <param name="env"></param>
            /// <returns></returns>
            public static IConfigurationBuilder Configure(IConfigurationBuilder config, IHostingEnvironment env)
            {
                return Configure(config, env.EnvironmentName);
            }

            /// <summary>
            /// Use for .NET Core Console applications.
            /// </summary>
            /// <param name="config"></param>
            /// <param name="env"></param>
            /// <returns></returns>
            private static IConfigurationBuilder Configure(IConfigurationBuilder config, Microsoft.Extensions.Hosting.IHostingEnvironment env)
            {
                return Configure(config, env.EnvironmentName);
            }

            private static IConfigurationBuilder Configure(IConfigurationBuilder config, string environmentName)
            {
                return config
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
            }

            /// <summary>
            /// Use for .NET Core Console applications.
            /// </summary>
            /// <returns></returns>
            public static IConfiguration CreateConfiguration()
            {
                var env = new HostingEnvironment
                {
                    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    ApplicationName = AppDomain.CurrentDomain.FriendlyName,
                    ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
                    ContentRootFileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
                };

                var logger = NLogBuilder.ConfigureNLog($"nlog.{env.EnvironmentName}.config").GetCurrentClassLogger();
                logger.Debug("Starting service");

                var config = new ConfigurationBuilder();
                var configured = Configure(config, env);
                return configured.Build();
            }
        }
    }
}
