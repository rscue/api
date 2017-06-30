namespace Rscue.Api.Extensions
{
    using MongoDB.Driver;
    using Rscue.Api.Models;

    public static class MongoDatabaseExtensions
    {
        public static IMongoCollection<Assignment> Assignments(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<Assignment>("assignments");

        public static IMongoCollection<Client> Clients(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<Client>("clients");

        public static IMongoCollection<ProviderWorker> Workers(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<ProviderWorker>("workers");

        public static IMongoCollection<Provider> Providers(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<Provider>("providers");

        public static IMongoCollection<ProviderBoatTow> BoatTows(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<ProviderBoatTow>("boattows");

        public static IMongoCollection<ImageBucket> ImageBuckets(this IMongoDatabase mongoDatabase) => mongoDatabase.GetCollection<ImageBucket>("image-buckets");
    }
}
