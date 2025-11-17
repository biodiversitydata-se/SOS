using MongoDB.Bson.Serialization.Conventions;

namespace SOS.DataStewardship.Api.Extensions;

internal static class MongoDbConventionExtensions
{
    extension(WebApplicationBuilder webApplicationBuilder)
    {
        internal WebApplicationBuilder SetupMongoDbConventions()
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

            return webApplicationBuilder;
        }
    }
}