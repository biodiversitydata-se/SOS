﻿using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Conventions;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Shared;

namespace SOS.Import.LiveIntegrationTests
{
    public class TestBase
    {
        public TestBase()
        {
            // MongoDB conventions.
            ConventionRegistry.Register(
                "MongoDB Solution Conventions",
                new ConventionPack
                {
                    new IgnoreExtraElementsConvention(true),
                    new IgnoreIfNullConvention(true)
                },
                t => true);
        }

        protected ImportConfiguration GetImportConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var importConfiguration = config.GetSection(nameof(ImportConfiguration)).Get<ImportConfiguration>();
            return importConfiguration;
        }

        protected MongoDbConfiguration GetVerbatimDbConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var verbatimDbConfiguration = config.GetSection("VerbatimDbConfiguration").Get<MongoDbConfiguration>();
            return verbatimDbConfiguration;
        }

        protected MongoDbConfiguration GetProcessDbConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var exportConfiguration = config.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
            return exportConfiguration;
        }
    }
}