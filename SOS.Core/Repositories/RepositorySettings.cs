namespace SOS.Core.Repositories
{
    public class RepositorySettings : IRepositorySettings
    {
        public string MongoDbConnectionString { get; set; }
        public string JobsDatabaseName { get; set; }
    }
}
