namespace Rscue.Api.Tests.Services
{
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ProviderRepository")]
    [Trait("DependsOn", "mongodb")]
    public class ProviderRepositoryTests
    {
        private ProviderRepository _providerRepository;
        private ITestDataStore _dataStore;

        public ProviderRepositoryTests()
        {
            var mongoDatabase = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            _providerRepository = new ProviderRepository(mongoDatabase);
            _dataStore = new MongoTestDataStore(mongoDatabase);
        }

        [Fact]
        public async Task TestGetProviderByIdAsyncCanRetrievProvider()
        {
            // arrange
            var id = Guid.NewGuid().ToString("n");
            var provider = new Provider
            {
                Id = id,
                City = "Springfield",
                State = "Illinois",
                Name = "TowNow!",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey {  Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            _dataStore.EnsureProvider(provider);

            // act 
            var (providerResult, outcomeAction, error) = await _providerRepository.GetByIdAsync(id);

            // assert
            Assert.NotNull(providerResult);
            Assert.Equal(RepositoryOutcomeAction.OkNone, outcomeAction);
            Assert.Null(error);
            Assert.Equal(provider.Id, providerResult.Id);
            Assert.Equal(provider.City, providerResult.City);
            Assert.Equal(provider.State, providerResult.State);
            Assert.Equal(provider.Name, providerResult.Name);
            Assert.Equal(provider.Email, providerResult.Email);
            Assert.Equal(provider.ZipCode, providerResult.ZipCode);
            Assert.Equal(provider.ProviderImageBucketKey?.Store, providerResult.ProviderImageBucketKey?.Store);
            Assert.Equal(provider.ProviderImageBucketKey?.Bucket, providerResult.ProviderImageBucketKey?.Bucket);
            Assert.Equal(provider.Address, providerResult.Address);
        }

        [Fact]
        public async Task TestGetProviderByIdAsyncReturnsNullOnNonExistantProvider()
        {
            // arrange
            var id = "0000ffff0000ffff0000ffff0000ffff";

            _dataStore.EnsureProviderDoesNotExist(id);

            // act 
            var (providerResult, outcomeAction, error) = await _providerRepository.GetByIdAsync(id);

            // assert
            Assert.Null(providerResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }

        [Fact]
        public async Task TestNewProviderAsyncCreatesResult()
        {
            // arrange
            var provider = new Provider
            {
                City = "Springfield",
                State = "Illinois",
                Name = "TowNow!",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            // act
            var (providerResult, outcomeAction, error) = await _providerRepository.NewAsync(provider);

            // assert
            Assert.NotNull(providerResult);
            Assert.Equal(RepositoryOutcomeAction.OkCreated, outcomeAction);
            Assert.Null(error);
            // here we check that what was returned is the same as we sent.
            Assert.NotNull(providerResult.Id);
            Assert.Equal(provider.City, providerResult.City);
            Assert.Equal(provider.State, providerResult.State);
            Assert.Equal(provider.Name, providerResult.Name);
            Assert.Equal(provider.Email, providerResult.Email);
            Assert.Equal(provider.ZipCode, providerResult.ZipCode);
            Assert.Equal(provider.ProviderImageBucketKey?.Store, providerResult.ProviderImageBucketKey?.Store);
            Assert.Equal(provider.ProviderImageBucketKey?.Bucket, providerResult.ProviderImageBucketKey?.Bucket);
            Assert.Equal(provider.Address, providerResult.Address);
            Assert.True(
                _dataStore
                    .TestProvider(_ =>
                        _.Id == providerResult.Id &&
                        _.City == providerResult.City &&
                        _.Name == providerResult.Name &&
                        _.Email == providerResult.Email &&
                        _.ZipCode == providerResult.ZipCode &&
                        ((_.ProviderImageBucketKey != null && providerResult.ProviderImageBucketKey != null) || _.ProviderImageBucketKey.Store == providerResult.ProviderImageBucketKey.Store) &&
                        ((_.ProviderImageBucketKey != null && providerResult.ProviderImageBucketKey != null) || _.ProviderImageBucketKey.Bucket == providerResult.ProviderImageBucketKey.Bucket) &&
                        _.Address == providerResult.Address));
        }

        [Fact]
        public async Task TestUpdateAsyncUpdatesExitentImageBucket()
        {
            // arrange
            var id = Guid.NewGuid().ToString("n");
            var providerUpdate = new Provider
            {
                Id = id,
                City = "Springfield",
                State = "Illinois",
                Name = "Mr. Plow",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            _dataStore.EnsureProvider(
                new Provider
                {
                    Id = id,
                    City = "Springfield",
                    State = "Illinois",
                    Name = "TowNow!",
                    Email = "call@townow.com",
                    ZipCode = "2342",
                    ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                    Address = "742 Evergreen Terrace",
                });

            // act 
            var (providerResult, outcomeAction, error) = await _providerRepository.UpdateAsync(providerUpdate);

            // assert
            Assert.NotNull(providerResult);
            Assert.Equal(RepositoryOutcomeAction.OkUpdated, outcomeAction);
            Assert.Null(error);

            Assert.Equal(providerUpdate.Id, providerResult.Id);
            Assert.Equal(providerUpdate.City, providerResult.City);
            Assert.Equal(providerUpdate.State, providerResult.State);
            Assert.Equal(providerUpdate.Name, providerResult.Name);
            Assert.Equal(providerUpdate.Email, providerResult.Email);
            Assert.Equal(providerUpdate.ZipCode, providerResult.ZipCode);
            Assert.Equal(providerUpdate.ProviderImageBucketKey?.Store, providerResult.ProviderImageBucketKey?.Store);
            Assert.Equal(providerUpdate.ProviderImageBucketKey?.Bucket, providerResult.ProviderImageBucketKey?.Bucket);
            Assert.Equal(providerUpdate.Address, providerResult.Address);

            Assert.True(
                _dataStore
                    .TestProvider(_ =>
                        _.Id == providerResult.Id &&
                        _.City == providerResult.City &&
                        _.Name == providerResult.Name &&
                        _.Email == providerResult.Email &&
                        _.ZipCode == providerResult.ZipCode &&
                        ((_.ProviderImageBucketKey != null && providerResult.ProviderImageBucketKey != null) || _.ProviderImageBucketKey.Store == providerResult.ProviderImageBucketKey.Store) &&
                        ((_.ProviderImageBucketKey != null && providerResult.ProviderImageBucketKey != null) || _.ProviderImageBucketKey.Bucket == providerResult.ProviderImageBucketKey.Bucket) &&
                        _.Address == providerResult.Address));
        }

        [Fact]
        public async Task TestUpdateAsyncReturnsNullOnNonExistantProvider()
        {
            // arrange
            var id = "0000ffff0000ffff0000ffff0000ffff";

            var providerToUpdate = new Provider
            {
                Id = id,
                City = "Springfield",
                State = "Illinois",
                Name = "TowNow!",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            _dataStore.EnsureProviderDoesNotExist(id);

            // act 
            var (updateProvider, outcomeAction, error) = await _providerRepository.UpdateAsync(providerToUpdate);

            // assert
            Assert.Null(updateProvider);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }

        [Fact]
        public async Task TestPatchAllButProviderImageStoreAsyncUpdatesExitentProviderButProviderImageStore()
        {
            // arrange
            var id = Guid.NewGuid().ToString("n");
            var providerUpdate = new Provider
            {
                Id = id,
                City = "Springfield",
                State = "Illinois",
                Name = "Mr. Plow",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey { Store = "some-other-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            _dataStore.EnsureProvider(
                new Provider
                {
                    Id = id,
                    City = "Springfield",
                    State = "Illinois",
                    Name = "TowNow!",
                    Email = "call@townow.com",
                    ZipCode = "2342",
                    ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                    Address = "742 Evergreen Terrace",
                });

            // act 
            var (providerResult, outcomeAction, error) = await _providerRepository.PatchAllButProviderImageStoreAsync(providerUpdate);

            // assert
            Assert.NotNull(providerResult);
            Assert.Equal(RepositoryOutcomeAction.OkUpdated, outcomeAction);
            Assert.Null(error);

            Assert.Equal(providerUpdate.Id, providerResult.Id);
            Assert.Equal(providerUpdate.City, providerResult.City);
            Assert.Equal(providerUpdate.State, providerResult.State);
            Assert.Equal(providerUpdate.Name, providerResult.Name);
            Assert.Equal(providerUpdate.Email, providerResult.Email);
            Assert.Equal(providerUpdate.ZipCode, providerResult.ZipCode);
            Assert.NotEqual(providerUpdate.ProviderImageBucketKey?.Store, providerResult.ProviderImageBucketKey?.Store);
            Assert.NotEqual(providerUpdate.ProviderImageBucketKey?.Bucket, providerResult.ProviderImageBucketKey?.Bucket);
            Assert.Equal(providerUpdate.Address, providerResult.Address);

            Assert.True(
                _dataStore
                    .TestProvider(_ =>
                        _.Id == providerResult.Id &&
                        _.City == providerResult.City &&
                        _.Name == providerResult.Name &&
                        _.Email == providerResult.Email &&
                        _.ZipCode == providerResult.ZipCode &&
                        ((_.ProviderImageBucketKey != null && providerResult.ProviderImageBucketKey != null) || _.ProviderImageBucketKey.Store != providerResult.ProviderImageBucketKey.Store) &&
                        ((_.ProviderImageBucketKey != null && providerResult.ProviderImageBucketKey != null) || _.ProviderImageBucketKey.Bucket != providerResult.ProviderImageBucketKey.Bucket) &&
                        _.Address == providerResult.Address));
        }

        [Fact]
        public async Task TestPatchAllButProviderImageStoreAsyncReturnsNullOnNonExistantProvider()
        {
            // arrange
            var id = "0000ffff0000ffff0000ffff0000ffff";

            var providerToUpdate = new Provider
            {
                Id = id,
                City = "Springfield",
                State = "Illinois",
                Name = "TowNow!",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            _dataStore.EnsureProviderDoesNotExist(id);

            // act 
            var (updateProvider, outcomeAction, error) = await _providerRepository.PatchAllButProviderImageStoreAsync(providerToUpdate);

            // assert
            Assert.Null(updateProvider);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }
    }
}
