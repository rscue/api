namespace Rscue.Api.Controllers
{
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [Authorize]
    [Route("image")]
    public class ImagesController : Controller
    {
        private static readonly List<string> emptyStringList = new List<string>();

        private readonly IImageBucketRepository _imageBucketRepository;
        private readonly IImageStore _imageStore;

        public ImagesController(IImageBucketRepository imageBucketRepository, IImageStore imageStore)
        {
            _imageBucketRepository = imageBucketRepository ?? throw new ArgumentNullException(nameof(imageBucketRepository));
            _imageStore = imageStore ?? throw new ArgumentNullException(nameof(imageStore));
        }

        [HttpGet("{store:required}/{bucket:required}", Name = "GetImages")]
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetImages(string store, string bucket, CancellationToken cancellationToken = default(CancellationToken))
        {
            var (imageBucket, outcomeAction, error) = await _imageBucketRepository.GetImageBucket(new ImageBucketKey { Store = store, Bucket = bucket }, cancellationToken);
            var imageList = imageBucket?.ImageList ?? new List<string>();
            if (outcomeAction == RepositoryOutcomeAction.OkNone)
            {
                imageList = imageList.Select(_ => this.Url.RouteUrl("GetImage", new { store, bucket, name = _ })).ToList();
            }

            return this.FromRepositoryOutcome(outcomeAction, error, imageList);
        }

        [HttpGet("{store:required}/{bucket:required}/{name:required}", Name = "GetImage")]
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public IActionResult GetImage(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new StreamResult(
                async () => await _imageStore.GetImageContentTypeAsync(store, bucket, name, cancellationToken),
                async _ =>  await _imageStore.DownloadImageAsync(store, bucket, name, _, cancellationToken));
        }

        [HttpPost("{store:required}/{bucket:required}")]
        [ProducesResponseType(typeof(void), 201)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> NewImage(string store, string bucket, RawContent rawContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var name = Guid.NewGuid().ToString("n");
            var (imageBucket, outcomeAction, error) = await _imageBucketRepository.GetImageBucket(new ImageBucketKey { Store = store, Bucket = bucket }, cancellationToken);
            if (outcomeAction != RepositoryOutcomeAction.OkNone)
            {
                return BadRequest("Unknown store/bucket pair"); // 'cause POST should not return 404's
            }

            await _imageStore.UploadImageAsync(store, bucket, name, rawContent.ContentType, rawContent.Content, cancellationToken);
            imageBucket.ImageList.Add(name);
            (imageBucket, outcomeAction, error) = await _imageBucketRepository.UpdateImageBucket(imageBucket, CancellationToken.None);
            if (outcomeAction != RepositoryOutcomeAction.OkUpdated)
            {
                return this.StatusCode(500, error);
            }

            return this.CreatedAtRoute("GetImage", new { store, bucket, name }, null);
        }

        [HttpDelete("{store:required}/{bucket:required}/{name:required}")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(void), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> DeleteImage(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            var (imageBucket, outcome, error) = await _imageBucketRepository.GetImageBucket(new ImageBucketKey { Store = store, Bucket = bucket }, cancellationToken);
            if (outcome != RepositoryOutcomeAction.OkNone)
            {
                return this.FromRepositoryOutcome(outcome, error);
            }

            var removed = imageBucket.ImageList.Remove(name);
            if (!removed)
            {
                return this.NotFound();
            }

            (imageBucket, outcome, error) = await _imageBucketRepository.UpdateImageBucket(imageBucket, CancellationToken.None);
            if (outcome != RepositoryOutcomeAction.OkUpdated)
            {
                return this.StatusCode(500, error);

            }
            await _imageStore.DeleteImageAsync(store, bucket, name, CancellationToken.None);
            return this.NoContent();
        }
    }
}
