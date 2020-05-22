using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.ObservationApi
{
    public class ObservationApiConfiguration
    {
        public BlobStorageConfiguration BlobStorageConfiguration { get; set; }

        public MongoDbConfiguration ProcessedDbConfiguration { get; set; }

        public MongoDbConfiguration HangfireDbConfiguration { get; set; }

        public ElasticSearchConfiguration SearchDbConfiguration { get; set; }
    }
}
