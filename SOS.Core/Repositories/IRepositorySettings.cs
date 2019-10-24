namespace SOS.Core.Repositories
{
    public interface IRepositorySettings
    {
        string MongoDbConnectionString { get; set; }
        string JobsDatabaseName { get; set; }
    }
}
