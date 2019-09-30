using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Core.Repositories
{
    public interface IRepositorySettings
    {
        string MongoDbConnectionString { get; set; }
        string DatabaseName { get; set; }
        string JobsDatabaseName { get; set; }
    }
}
