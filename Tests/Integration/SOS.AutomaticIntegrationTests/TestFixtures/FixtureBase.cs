using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using SOS.TestHelpers;

namespace SOS.AutomaticIntegrationTests.TestFixtures
{
    public class FixtureBase
    {
        protected InstallationEnvironment GetEnvironmentFromAppSettings()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var environment = config.GetSection("ApiTestConfiguration:Environment").Get<string>();
            var installationEnvironment = GetInstallationEnvironment(environment);
            return installationEnvironment;
        }

        protected InstallationEnvironment GetInstallationEnvironment(string installationEnvironment)
        {
            if (string.IsNullOrEmpty(installationEnvironment))
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name}() does not support the value {installationEnvironment}",
                    nameof(installationEnvironment));
            }

            switch (installationEnvironment.ToLower())
            {
                case "local":
                    return InstallationEnvironment.Local;
                case "dev":
                case "developmenttest":
                    return InstallationEnvironment.DevelopmentTest;
                case "st":
                case "systemtest":
                    return InstallationEnvironment.SystemTest;
                case "prod":
                case "production":
                    return InstallationEnvironment.Production;
                default:
                    throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name}() does not support the value {installationEnvironment}",
                        nameof(installationEnvironment));
            }
        }

        protected string GetConfigPrefix(InstallationEnvironment installationEnvironment)
        {
            switch (installationEnvironment)
            {
                case InstallationEnvironment.Local:
                    return "Local";
                case InstallationEnvironment.DevelopmentTest:
                    return "Dev";
                case InstallationEnvironment.SystemTest:
                    return "ST";
                case InstallationEnvironment.Production:
                    return "Prod";
                default:
                    throw new ArgumentException(
                        $"{MethodBase.GetCurrentMethod()?.Name}() does not support the value {installationEnvironment}", nameof(InstallationEnvironment));
            }
        }

        protected IConfiguration GetAppSettings()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<FixtureBase>()
                .Build();

            return config;
        }
    }
}