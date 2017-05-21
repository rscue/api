namespace Rscue.Api.Tests.Services
{
    using MongoDB.Driver;
    using Rscue.Api.Services;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ImageBucketRepository")]
    public class ImageBucketRepositoryExceptionsTests
    {
        [Fact]
        public void ImageBucketRepositoryShouldNotBuildWithNullMongoDatabase()
        {
            // arrange
            var mongoDatabase = (IMongoDatabase)null;

            // act (deferred)
            var testCode = (Action)(() => new ImageBucketRepository(mongoDatabase));

            // assert
            Assert.Throws<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task GetImageBucketShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var mongoDatabase = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var imageBucketRepository = new ImageBucketRepository(mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await imageBucketRepository.GetImageBucket(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task NewImageBucketShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var mongoDatabase = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var imageBucketRepository = new ImageBucketRepository(mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await imageBucketRepository.NewImageBucket(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task UpdateImageBucketShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var mongoDatabase = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var imageBucketRepository = new ImageBucketRepository(mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await imageBucketRepository.UpdateImageBucket(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }
    }
}
