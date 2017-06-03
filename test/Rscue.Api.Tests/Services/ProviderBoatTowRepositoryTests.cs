namespace Rscue.Api.Tests.Services
{
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Collections.Generic;
    using System.Text;
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
            var (providerResult, outcomeAction, error) = await _providerBoatTowRepository.GetByIdAsync(providerId, id);

            // assert
            Assert.Null(providerResult);
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
            _dataStore.EnsureProviderDoesNotExist(id);

            // act 
            var (providerResult, outcomeAction, error) = await _providerBoatTowRepository.GetByIdAsync(providerId, id);

            // assert
            Assert.Null(providerResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }
    }
}
