namespace Rscue.Api.Services
{
    using Extensions;
    using System;
    using System.Threading.Tasks;
    using Rscue.Api.Models;
    using MongoDB.Driver;
    using System.Threading;

    public class ImageBucketRepository : IImageBucketRepository
    {
        private readonly IMongoDatabase _mongoDatabase;

        public ImageBucketRepository(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<(ImageBucket imageBucket, RepositoryOutcomeAction outcomeAction, object error)> GetImageBucket(ImageBucketKey imageBucketKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageBucketKey == null) throw new ArgumentNullException(nameof(imageBucketKey));
            var imageBucket = await (await _mongoDatabase.ImageBuckets().FindAsync(_ => _.StoreBucket.Store == imageBucketKey.Store && _.StoreBucket.Bucket == imageBucketKey.Bucket,cancellationToken: cancellationToken)).SingleOrDefaultAsync(cancellationToken);
            return (imageBucket, imageBucket != null ? RepositoryOutcomeAction.OkNone : RepositoryOutcomeAction.NotFoundNone, null);
        }

        public async Task<(ImageBucket imageBucket, RepositoryOutcomeAction outcomeAction, object error)> NewImageBucket(ImageBucket imageBucket, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageBucket == null) throw new ArgumentNullException(nameof(imageBucket));
            imageBucket.StoreBucket.Bucket = Guid.NewGuid().ToString("n");
            await _mongoDatabase.ImageBuckets().InsertOneAsync(imageBucket, cancellationToken: cancellationToken);
            return (imageBucket, RepositoryOutcomeAction.OkCreated, null);
        }

        public async Task<(ImageBucket imageBucket, RepositoryOutcomeAction outcomeAction, object error)> UpdateImageBucket(ImageBucket imageBucket, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageBucket == null) throw new ArgumentNullException(nameof(imageBucket));
            var result = await _mongoDatabase.ImageBuckets().ReplaceOneAsync(_ => _.StoreBucket.Store == imageBucket.StoreBucket.Store && _.StoreBucket.Bucket == imageBucket.StoreBucket.Bucket, imageBucket, cancellationToken: cancellationToken);
            return
                result.ModifiedCount == 1 && result.ModifiedCount == result.MatchedCount
                    ? (imageBucket, RepositoryOutcomeAction.OkUpdated, (object)null)
                    : (null, RepositoryOutcomeAction.NotFoundNone, null);
        }
    }
}
