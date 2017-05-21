namespace Rscue.Api.Tests.Services
{
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ImageBucketRepository")]
    [Trait("DependsOn", "mongodb")]
    public partial class ImageBucketRepositoryTests
    {
        private ImageBucketRepository _imageBucketRepository;
        private ITestDataStore _dataStore;

        public ImageBucketRepositoryTests()
        {
            var mongoDatabase = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            _imageBucketRepository = new ImageBucketRepository(mongoDatabase);
            _dataStore = new MongoTestDataStore(mongoDatabase);
        }

        [Fact]
        public async Task TestGetImageBucketCanRetrieveBucket()
        {
            // arrange
            var key = new ImageBucketKey
            {
                Store = "some-store",
                Bucket = Guid.NewGuid().ToString("n")
            };

            _dataStore.EnsureImageBucket(
                new ImageBucket
                {
                    StoreBucket = key,
                    ImageList = new List<string> { "one-profile-picture", "other-profile-picture" }
                });

            // act 
            var (imageBucket, outcomeAction, error) = await _imageBucketRepository.GetImageBucket(key);

            // assert
            Assert.NotNull(imageBucket);
            Assert.Equal(RepositoryOutcomeAction.OkNone, outcomeAction);
            Assert.Null(error);
            Assert.Equal(key.Store, imageBucket.StoreBucket.Store);
            Assert.Equal(key.Bucket, imageBucket.StoreBucket.Bucket);
            Assert.Collection(imageBucket.ImageList, 
                _ => Assert.Equal("one-profile-picture", _),
                _ => Assert.Equal("other-profile-picture", _));
        }

        [Fact]
        public async Task TestGetImageBucketReturnsNullOnNonExistantImageBucket()
        {
            // arrange
            var key = new ImageBucketKey
            {
                Store = "non-existant-store",
                Bucket = Guid.NewGuid().ToString("n")
            };

            _dataStore.EnsureImageBucketDoesNotExist(key);

            // act 
            var (imageBucket, outcomeAction, error) = await _imageBucketRepository.GetImageBucket(key);

            // assert
            Assert.Null(imageBucket);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }

        [Fact]
        public async Task TestNewImageBucketCreatesResult()
        {
            // arrange
            var key = new ImageBucketKey
            {
                Store = "some-store"
            };
            var imageBucket = new ImageBucket
            {
                StoreBucket = key,
                ImageList = new List<string> { "profile-picture-1", "profile-picture-2" }
            };

            // act
            var (newImageBucket, outcomeAction, error) = await _imageBucketRepository.NewImageBucket(imageBucket);

            // assert
            Assert.NotNull(newImageBucket);
            Assert.Equal(RepositoryOutcomeAction.OkCreated, outcomeAction);
            Assert.Null(error);
            Assert.Equal(key.Store, newImageBucket.StoreBucket.Store);
            Assert.NotNull(newImageBucket.StoreBucket.Bucket);
            Assert.Collection(newImageBucket.ImageList,
                _ => Assert.Equal("profile-picture-1", _),
                _ => Assert.Equal("profile-picture-2", _));
            Assert.True(
                _dataStore
                    .TestImageBucket(_ => 
                        _.StoreBucket.Store == newImageBucket.StoreBucket.Store && 
                        _.StoreBucket.Bucket == newImageBucket.StoreBucket.Bucket));
        }

        [Fact]
        public async Task TestUpdateImageBucketUpdatesExitentImageBucket()
        {
            // arrange
            var key = new ImageBucketKey
            {
                Store = "some-store",
                Bucket = Guid.NewGuid().ToString("n")
            };

            var imageBucketToUpdate =
                new ImageBucket
                {
                    StoreBucket = key,
                    ImageList = new List<string> { "profile-picture-3", "profile-picture-5" }
                };

            _dataStore.EnsureImageBucket(
                new ImageBucket
                {
                    StoreBucket = key,
                    ImageList = new List<string> { "profile-picture-3", "profile-picture-4" }
                });

            // act 
            var (updatedImageBucket, outcomeAction, error) = await _imageBucketRepository.UpdateImageBucket(imageBucketToUpdate);

            // assert
            Assert.NotNull(updatedImageBucket);
            Assert.Equal(RepositoryOutcomeAction.OkUpdated, outcomeAction);
            Assert.Null(error);
            Assert.Equal(key.Store, updatedImageBucket.StoreBucket.Store);
            Assert.Equal(key.Bucket, updatedImageBucket.StoreBucket.Bucket);
            Assert.Collection(updatedImageBucket.ImageList,
                _ => Assert.Equal("profile-picture-3", _),
                _ => Assert.Equal("profile-picture-5", _));
            Assert.True(
                _dataStore
                    .TestImageBucket(_ =>
                        _.StoreBucket.Store == updatedImageBucket.StoreBucket.Store &&
                        _.StoreBucket.Bucket == updatedImageBucket.StoreBucket.Bucket));
        }

        [Fact]
        public async Task TestUpdateImageBucketReturnsNullOnNonExistantIamgeBucket()
        {
            // arrange
            var key = new ImageBucketKey
            {
                Store = "non-existant-store",
                Bucket = Guid.NewGuid().ToString("n")
            };

            var imageBucketToUpdate =
                new ImageBucket
                {
                    StoreBucket = key,
                    ImageList = new List<string> { "profile-picture-5", "profile-picture-6" }
                };

            _dataStore.EnsureImageBucketDoesNotExist(key);

            // act 
            var (updatedImageBucket, outcomeAction, error) = await _imageBucketRepository.UpdateImageBucket(imageBucketToUpdate);

            // assert
            Assert.Null(updatedImageBucket);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }
    }
}
