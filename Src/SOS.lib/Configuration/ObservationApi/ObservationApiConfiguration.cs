using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.ObservationApi
{
    public class ObservationApiConfiguration
    {
        public BlobStorageConfiguration BlobStorageConfiguration { get; set; }

        public MongoDbConfiguration ProcessedDbConfiguration { get; set; }

        public HangfireDbConfiguration HangfireDbConfiguration { get; set; }

        public ElasticSearchConfiguration SearchDbConfiguration { get; set; }

        public UserServiceConfiguration UserServiceConfiguration { get; set; }
    }
}