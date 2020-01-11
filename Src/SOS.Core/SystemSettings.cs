using System;
using System.Collections.Generic;
using System.Text;
using SOS.Core.Repositories;

namespace SOS.Core
{
    public static class SystemSettings
    {
        public static string MongoDbConnectionString { get; set; }

        public static string JobsDatabaseName { get; set; }

        public static void InitSettings(IRepositorySettings settings)
        {
            MongoDbConnectionString = settings.MongoDbConnectionString;
            JobsDatabaseName = settings.JobsDatabaseName;
        }

        public static RepositorySettings GetRepositorySettings()
        {
            return new RepositorySettings
            {
                JobsDatabaseName = JobsDatabaseName,
                MongoDbConnectionString = MongoDbConnectionString
            };
        }
    }

}
