using MongoDB.Bson.Serialization.Conventions;

namespace SOS.DataStewardship.Api.Extensions;

internal static class MongoDbConventionExtensions
{
    internal static WebApplicationBuilder SetupMongoDbConventions(this WebApplicationBuilder webApplicationBuilder)
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