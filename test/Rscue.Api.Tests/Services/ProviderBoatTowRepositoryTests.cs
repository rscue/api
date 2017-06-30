namespace Rscue.Api.Tests.Services
{
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ProviderBoatTowRepository")]
    [Trait("DependsOn", "mongodb")]
    public class ProviderBoatTowRepositoryTests
    {
        private readonly IProviderBoatTowRepository _providerBoatTowRepository;
        private ITestDataStore _dataStore;

        public ProviderBoatTowRepositoryTests()
        {
            var mongoDatabase = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            _providerBoatTowRepository = new ProviderBoatTowRepository(mongoDatabase);
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

            var providerBoatTow = new ProviderBoatTow
            {
                Id = id,
                ProviderId = providerId,
                FuelCostPerKm = 13,
                RegistrationNumber = "1234ASD"
            };

            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderBoatTow(providerBoatTow);

            // act 
            var (providerBoatTowResult, outcomeAction, error) = await _providerBoatTowRepository.GetByIdAsync(providerId, id);

            // assert
            Assert.NotNull(providerBoatTowResult);
            Assert.Equal(RepositoryOutcomeAction.OkNone, outcomeAction);
            Assert.Null(error);
            Assert.Equal(providerBoatTow.Id, providerBoatTowResult.Id);
            Assert.Equal(providerBoatTow.ProviderId, providerBoatTowResult.ProviderId);
            Assert.Equal(providerBoatTow.FuelCostPerKm, providerBoatTowResult.FuelCostPerKm);
            Assert.Equal(providerBoatTow.Name, providerBoatTowResult.Name);
            Assert.Equal(providerBoatTow.RegistrationNumber, providerBoatTowResult.RegistrationNumber);
            Assert.Equal(providerBoatTow.BoatModel, providerBoatTowResult.BoatModel);
            Assert.Equal(providerBoatTow.EngineType, providerBoatTowResult.EngineType);
        }

        [Fact]
        public async Task TestGetByIdAsyncReturnsNullOnNonExistantProvider()
        {
            // arrange
            var providerId = "0000ffff0000ffff0000ffff0000ffff";
            var id = "0000ffff0000ffff0000ffff0000ffff";

            _dataStore.EnsureProviderBoatTowDoesNotExist(providerId, id);
            _dataStore.EnsureProviderDoesNotExist(providerId);

            // act 
            var (providerBoatTowResult, outcomeAction, error) = await _providerBoatTowRepository.GetByIdAsync(providerId, id);

            // assert
            Assert.Null(providerBoatTowResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }

        [Fact]
        public async Task TestGetByIdAsyncReturnsNullOnNonExistantProviderBoatTow()
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
            _dataStore.EnsureProviderBoatTowDoesNotExist(providerId, id);

            // act 
            var (providerBoatTowResult, outcomeAction, error) = await _providerBoatTowRepository.GetByIdAsync(providerId, id);

            // assert
            Assert.Null(providerBoatTowResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }

        [Fact]
        public async Task TesNewAsyncFailsToCreateProviderBoatTowWhenProviderDoesNotExist()
        {
            // arrange
            var providerId = "0000ffff0000ffff0000ffff0000ffff";

            var providerBoatTow = new ProviderBoatTow
            {
                ProviderId = providerId,
                FuelCostPerKm = 13,
                RegistrationNumber = "1234ASD"
            };

            _dataStore.EnsureProviderDoesNotExist(providerId);

            // act
            var (providerBoatTowResult, outcomeAction, error) = await _providerBoatTowRepository.NewAsync(providerId, providerBoatTow);

            // assert
            Assert.Null(providerBoatTowResult);
            Assert.Equal(RepositoryOutcomeAction.ValidationErrorNone, outcomeAction);
            Assert.NotNull(error);
        }

        [Fact]
        public async Task TesNewAsyncCreatesProviderBoatTowResult()
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

            var providerBoatTow = new ProviderBoatTow
            {
                ProviderId = providerId,
                FuelCostPerKm = 13,
                RegistrationNumber = "1234ASD"
            };

            _dataStore.EnsureProvider(provider);

            // act
            var (providerBoatTowResult, outcomeAction, error) = await _providerBoatTowRepository.NewAsync(providerId, providerBoatTow);

            // assert
            Assert.NotNull(providerBoatTowResult);
            Assert.Equal(RepositoryOutcomeAction.OkCreated, outcomeAction);
            Assert.Null(error);
            // here we check that what was returned is the same as we sent.
            Assert.NotNull(providerBoatTowResult.Id);
            Assert.Equal(providerBoatTow.ProviderId, providerBoatTowResult.ProviderId);
            Assert.Equal(providerBoatTow.FuelCostPerKm, providerBoatTowResult.FuelCostPerKm);
            Assert.Equal(providerBoatTow.Name, providerBoatTowResult.Name);
            Assert.Equal(providerBoatTow.RegistrationNumber, providerBoatTowResult.RegistrationNumber);
            Assert.Equal(providerBoatTow.BoatModel, providerBoatTowResult.BoatModel);
            Assert.Equal(providerBoatTow.EngineType, providerBoatTowResult.EngineType);
            Assert.True(
                _dataStore
                    .TestProviderBoatTow(_ =>
                        _.Id == providerBoatTow.Id &&
                        _.ProviderId == providerBoatTow.ProviderId &&
                        _.FuelCostPerKm == providerBoatTow.FuelCostPerKm &&
                        _.Name == providerBoatTow.Name &&
                        _.RegistrationNumber == providerBoatTow.RegistrationNumber &&
                        _.BoatModel == providerBoatTow.BoatModel &&
                        _.EngineType == providerBoatTow.EngineType));
        }

        [Fact]
        public async Task TestUpdateAsyncUpdatesProviderBoatTow()
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

            var providerBoatTow = new ProviderBoatTow
            {
                Id = id,
                ProviderId = providerId,
                FuelCostPerKm = 13,
                RegistrationNumber = "1234ASD"
            };

            var updatedProviderBoatTow = new ProviderBoatTow
            {
                Id = id,
                ProviderId = providerId,
                FuelCostPerKm = 13,
                RegistrationNumber = "1234ASDF"
            };

            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderBoatTow(providerBoatTow);

            // act 
            var (providerBoatTowResult, outcomeAction, error) = await _providerBoatTowRepository.UpdateAsync(providerId, updatedProviderBoatTow);

            // assert
            Assert.NotNull(providerBoatTowResult);
            Assert.Equal(RepositoryOutcomeAction.OkUpdated, outcomeAction);
            Assert.Null(error);

            Assert.Equal(updatedProviderBoatTow.Id, providerBoatTowResult.Id);
            Assert.Equal(updatedProviderBoatTow.ProviderId, providerBoatTowResult.ProviderId);
            Assert.Equal(updatedProviderBoatTow.FuelCostPerKm, providerBoatTowResult.FuelCostPerKm);
            Assert.Equal(updatedProviderBoatTow.Name, providerBoatTowResult.Name);
            Assert.Equal(updatedProviderBoatTow.RegistrationNumber, providerBoatTowResult.RegistrationNumber);
            Assert.Equal(updatedProviderBoatTow.BoatModel, providerBoatTowResult.BoatModel);
            Assert.Equal(updatedProviderBoatTow.EngineType, providerBoatTowResult.EngineType);

            Assert.True(
                _dataStore
                    .TestProviderBoatTow(_ =>
                        _.Id == updatedProviderBoatTow.Id &&
                        _.ProviderId == updatedProviderBoatTow.ProviderId &&
                        _.FuelCostPerKm == updatedProviderBoatTow.FuelCostPerKm &&
                        _.Name == updatedProviderBoatTow.Name &&
                        _.RegistrationNumber == updatedProviderBoatTow.RegistrationNumber &&
                        _.BoatModel == updatedProviderBoatTow.BoatModel &&
                        _.EngineType == updatedProviderBoatTow.EngineType));
        }

        [Fact]
        public async Task TestUpdateAsyncFailsToCreateProviderBoatTowWhenProviderDoesNotExist()
        {
            // arrange
            var providerId = "0000ffff0000ffff0000ffff0000ffff";
            var id = Guid.NewGuid().ToString("n");

            var providerBoatTow = new ProviderBoatTow
            {
                Id = id,
                ProviderId = providerId,
                FuelCostPerKm = 13,
                RegistrationNumber = "1234ASD"
            };

            _dataStore.EnsureProviderDoesNotExist(providerId);

            // act 
            var (updateProviderBoatTowResult, outcomeAction, error) = await _providerBoatTowRepository.UpdateAsync(providerId, providerBoatTow);

            // assert
            Assert.Null(updateProviderBoatTowResult);
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

            var providerBoatTow = new ProviderBoatTow
            {
                Id = id,
                ProviderId = providerId,
                FuelCostPerKm = 13,
                RegistrationNumber = "1234ASD"
            };

            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderBoatTowDoesNotExist(providerId, id);

            // act 
            var (updateProviderBoatTowResult, outcomeAction, error) = await _providerBoatTowRepository.UpdateAsync(providerId, providerBoatTow);

            // assert
            Assert.Null(updateProviderBoatTowResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }
    }
}
