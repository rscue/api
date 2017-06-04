namespace Rscue.Api.Services
{
    using Rscue.Api.Models;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IImageBucketRepository
    {
        Task<(ImageBucket imageBucket, RepositoryOutcomeAction outcomeAction, object error)> GetImageBucket(ImageBucketKey imageBucketKey, CancellationToken cancellationToken = default(CancellationToken));

        Task<(ImageBucket imageBucket, RepositoryOutcomeAction outcomeAction, object error)> NewImageBucket(ImageBucket imageBucket, CancellationToken cancellationToken = default(CancellationToken));

        Task<(ImageBucket imageBucket, RepositoryOutcomeAction outcomeAction, object error)> UpdateImageBucket(ImageBucket imageBucket, CancellationToken cancellationToken = default(CancellationToken));
    }
}
