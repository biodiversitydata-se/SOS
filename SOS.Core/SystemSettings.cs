using System;
using System.Collections.Generic;
using System.Text;
using SOS.Core.Repositories;

namespace SOS.Core
{
    public static class SystemSettings
    {
        public static string MongoDbConnectionString { get; set; }
        public static string DatabaseName { get; set; }
        public static string JobsDatabaseName { get; set; }

        public static void InitSettings(IRepositorySettings settings)
        {
            MongoDbConnectionString = settings.MongoDbConnectionString;
            DatabaseName = settings.DatabaseName;
            JobsDatabaseName = settings.JobsDatabaseName;
        }

        public static RepositorySettings GetRepositorySettings()
        {
            return new RepositorySettings
            {
                DatabaseName = DatabaseName,
                JobsDatabaseName = JobsDatabaseName,
                MongoDbConnectionString = MongoDbConnectionString
            };
        }
    }

}
