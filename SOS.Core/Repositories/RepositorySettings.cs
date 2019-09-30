using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Core.Repositories
{
    public class RepositorySettings : IRepositorySettings
    {
        public string MongoDbConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string JobsDatabaseName { get; set; }
    }
}
