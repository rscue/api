namespace Rscue.Api.Tests.Services
{
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ProviderWorkerRepository")]
    [Trait("DependsOn", "mongodb")]
    public class ProviderWorkerRepositoryTests
    {
        private readonly IProviderWorkerRepository _providerWorkerRepository;
        private ITestDataStore _dataStore;

        public ProviderWorkerRepositoryTests()
        {
            var mongoDatabase = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            _providerWorkerRepository = new ProviderWorkerRepository(mongoDatabase);
            _dataStore = new MongoTestDataStore(mongoDatabase);
        }

        [Fact]
        public async Task TestGetByIdAsyncCanRetrieveProviderBoatTow()
        {
            // arrange
            var providerId = Guid.NewGuid().ToString("n");
            var id = Guid.NewGuid().ToString("n");
            var provider = new Provider
            {
                Id = providerId,
                City = "Springfield",
                State = "Illinois",
                Name = "TowNow!",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            var providerWorker = new ProviderWorker
            {
                Id = id,
                ProviderId = providerId,
                Name = "Homer",
                LastName = "Simpson"
            };

            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderWorker(providerWorker);

            // act 
            var (providerWorkerResult, outcomeAction, error) = await _providerWorkerRepository.GetByIdAsync(providerId, id);

            // assert
            Assert.NotNull(providerWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.OkNone, outcomeAction);
            Assert.Null(error);
            Assert.Equal(providerWorker.Id, providerWorkerResult.Id);
            Assert.Equal(providerWorker.ProviderId, providerWorkerResult.ProviderId);
            Assert.Equal(providerWorker.LastKnownLocation, providerWorkerResult.LastKnownLocation);
            Assert.Equal(providerWorker.LastName, providerWorkerResult.LastName);
            Assert.Equal(providerWorker.Name, providerWorkerResult.Name);
            Assert.Equal(providerWorker.PhoneNumber, providerWorkerResult.PhoneNumber);
            Assert.Equal(providerWorker.Status, providerWorkerResult.Status);
        }

        [Fact]
        public async Task TestGetByIdAsyncReturnsNullOnNonExistantProvider()
        {
            // arrange
            var providerId = "0000ffff0000ffff0000ffff0000ffff";
            var id = "0000ffff0000ffff0000ffff0000ffff";

            _dataStore.EnsureProviderWorkerDoesNotExist(providerId, id);
            _dataStore.EnsureProviderDoesNotExist(providerId);

            // act 
            var (providerWorkerResult, outcomeAction, error) = await _providerWorkerRepository.GetByIdAsync(providerId, id);

            // assert
            Assert.Null(providerWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }

        [Fact]
        public async Task TestGetByIdAsyncReturnsNullOnNonExistantProviderWorker()
        {
            // arrange
            var providerId = Guid.NewGuid().ToString("n");
            var id = "0000ffff0000ffff0000ffff0000ffff";

            var provider = new Provider
            {
                Id = providerId,
                City = "Springfield",
                State = "Illinois",
                Name = "TowNow!",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderWorkerDoesNotExist(providerId, id);

            // act 
            var (providerWorkerResult, outcomeAction, error) = await _providerWorkerRepository.GetByIdAsync(providerId, id);

            // assert
            Assert.Null(providerWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }

        [Fact]
        public async Task TesNewAsyncFailsToCreateProviderWorkerWhenProviderDoesNotExist()
        {
            // arrange
            var providerId = "0000ffff0000ffff0000ffff0000ffff";

            var providerWorker = new ProviderWorker
            {
                ProviderId = providerId,
                Name = "Homer",
                LastName = "Simpson"
            };

            _dataStore.EnsureProviderDoesNotExist(providerId);

            // act
            var (providerWorkerResult, outcomeAction, error) = await _providerWorkerRepository.NewAsync(providerId, providerWorker);

            // assert
            Assert.Null(providerWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.ValidationErrorNone, outcomeAction);
            Assert.NotNull(error);
        }

        [Fact]
        public async Task TesNewAsyncCreatesProviderWorkerResult()
        {
            // arrange
            var providerId = Guid.NewGuid().ToString("n");
            var provider = new Provider
            {
                Id = providerId,
                City = "Springfield",
                State = "Illinois",
                Name = "TowNow!",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            var providerWorker = new ProviderWorker
            {
                ProviderId = providerId,
                Name = "Homer",
                LastName = "Simpson",
            };

            _dataStore.EnsureProvider(provider);

            // act
            var (providerWorkerResult, outcomeAction, error) = await _providerWorkerRepository.NewAsync(providerId, providerWorker);

            // assert
            Assert.NotNull(providerWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.OkCreated, outcomeAction);
            Assert.Null(error);
            // here we check that what was returned is the same as we sent.
            Assert.NotNull(providerWorkerResult.Id);
            Assert.Equal(providerWorker.ProviderId, providerWorkerResult.ProviderId);
            Assert.Equal(providerWorker.LastKnownLocation, providerWorkerResult.LastKnownLocation);
            Assert.Equal(providerWorker.LastName, providerWorkerResult.LastName);
            Assert.Equal(providerWorker.Name, providerWorkerResult.Name);
            Assert.Equal(providerWorker.PhoneNumber, providerWorkerResult.PhoneNumber);
            Assert.Equal(providerWorker.Status, providerWorkerResult.Status);
            Assert.True(
                _dataStore
                    .TestProviderWorker(_ =>
                        _.Id == providerWorker.Id &&
                        _.ProviderId == providerWorker.ProviderId &&
                        _.LastKnownLocation == providerWorker.LastKnownLocation &&
                        _.LastName == providerWorker.LastName &&
                        _.Name == providerWorker.Name &&
                        _.PhoneNumber == providerWorker.PhoneNumber &&
                        _.Status == providerWorker.Status));
        }

        [Fact]
        public async Task TestUpdateAsyncUpdatesProviderWorker()
        {
            // arrange
            var providerId = Guid.NewGuid().ToString("n");
            var id = Guid.NewGuid().ToString("n");
            var provider = new Provider
            {
                Id = providerId,
                City = "Springfield",
                State = "Illinois",
                Name = "TowNow!",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            var providerWorker = new ProviderWorker
            {
                Id = id,
                ProviderId = providerId,
                Name = "Homer",
                LastName = "Simpson"
            };

            var updatedProviderWorker = new ProviderWorker
            {
                Id = id,
                ProviderId = providerId,
                Name = "Bart",
                LastName = "Simpson"
            };

            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderWorker(providerWorker);

            // act 
            var (providerWorkerResult, outcomeAction, error) = await _providerWorkerRepository.UpdateAsync(providerId, updatedProviderWorker);

            // assert
            Assert.NotNull(providerWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.OkUpdated, outcomeAction);
            Assert.Null(error);

            Assert.Equal(updatedProviderWorker.Id, providerWorkerResult.Id);
            Assert.Equal(updatedProviderWorker.ProviderId, providerWorkerResult.ProviderId);
            Assert.Equal(updatedProviderWorker.LastKnownLocation, providerWorkerResult.LastKnownLocation);
            Assert.Equal(updatedProviderWorker.LastName, providerWorkerResult.LastName);
            Assert.Equal(updatedProviderWorker.Name, providerWorkerResult.Name);
            Assert.Equal(updatedProviderWorker.PhoneNumber, providerWorkerResult.PhoneNumber);
            Assert.Equal(updatedProviderWorker.Status, providerWorkerResult.Status);

            Assert.True(
                _dataStore
                    .TestProviderWorker(_ =>
                        _.Id == updatedProviderWorker.Id &&
                        _.ProviderId == updatedProviderWorker.ProviderId &&
                        _.LastKnownLocation == updatedProviderWorker.LastKnownLocation &&
                        _.LastName == updatedProviderWorker.LastName &&
                        _.Name == updatedProviderWorker.Name &&
                        _.PhoneNumber == updatedProviderWorker.PhoneNumber &&
                        _.Status == updatedProviderWorker.Status));
        }

        [Fact]
        public async Task TestUpdateAsyncFailsToCreateProviderBoatTowWhenProviderDoesNotExist()
        {
            // arrange
            var providerId = "0000ffff0000ffff0000ffff0000ffff";
            var id = Guid.NewGuid().ToString("n");

            var providerBoatTow = new ProviderWorker
            {
                Id = id,
                ProviderId = providerId,
                Name = "Homer",
                LastName = "Simpson"
            };

            _dataStore.EnsureProviderDoesNotExist(providerId);

            // act 
            var (updateProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.UpdateAsync(providerId, providerBoatTow);

            // assert
            Assert.Null(updateProviderWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.ValidationErrorNone, outcomeAction);
            Assert.NotNull(error);
        }

        [Fact]
        public async Task TestUpdateAsyncReturnsNullOnNonExistantProvider()
        {
            // arrange
            var providerId = Guid.NewGuid().ToString("n");
            var id = "0000ffff0000ffff0000ffff0000ffff";
            var provider = new Provider
            {
                Id = providerId,
                City = "Springfield",
                State = "Illinois",
                Name = "TowNow!",
                Email = "call@townow.com",
                ZipCode = "2342",
                ProviderImageBucketKey = new ImageBucketKey { Store = "some-store", Bucket = Guid.NewGuid().ToString("n") },
                Address = "742 Evergreen Terrace",
            };

            var providerWorker = new ProviderWorker
            {
                Id = id,
                ProviderId = providerId,
                Name = "Homer",
                LastName = "Simpson"
            };

            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderBoatTowDoesNotExist(providerId, id);

            // act 
            var (updateProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.UpdateAsync(providerId, providerWorker);

            // assert
            Assert.Null(updateProviderWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }
    }
}
