﻿using Microsoft.Extensions.Configuration;
using SOS.Lib.Configuration.Shared;

namespace SOS.Observations.Api.LiveIntegrationTests
{
    public class TestBase
    {
        protected MongoDbConfiguration GetMongoDbConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<TestBase>()
                .Build();

            var mongoDbConfiguration = config.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
            return mongoDbConfiguration;
        }
    }
}