namespace Rscue.Api.Tests.Services
{
    using MongoDB.Driver;
    using Moq;
    using Rscue.Api.Services;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ImageBucketRepository")]
    public class ImageBucketRepositoryExceptionsTests
    {
        private readonly IMongoDatabase _mongoDatabase;

        public ImageBucketRepositoryExceptionsTests()
        {
            _mongoDatabase = new Mock<IMongoDatabase>().Object;
        }

        [Fact]
        public void ImageBucketRepositoryShouldNotBuildWithNullMongoDatabase()
        {
            // arrange
            // nothing to do

            // act (deferred)
            var testCode = (Action)(() => new ImageBucketRepository(null));

            // assert
            Assert.Throws<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task GetImageBucketShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var imageBucketRepository = new ImageBucketRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await imageBucketRepository.GetImageBucket(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task NewImageBucketShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var imageBucketRepository = new ImageBucketRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await imageBucketRepository.NewImageBucket(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task UpdateImageBucketShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var imageBucketRepository = new ImageBucketRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await imageBucketRepository.UpdateImageBucket(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }
    }
}
