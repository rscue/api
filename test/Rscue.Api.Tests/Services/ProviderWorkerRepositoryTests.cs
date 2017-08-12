namespace Rscue.Api.Tests.Services
{
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.JsonPatch.Operations;
    using Newtonsoft.Json.Serialization;
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Extensions;

    [Collection("ProviderWorkerRepository")]
    [Trait("DependsOn", "mongodb")]
    public class ProviderWorkerRepositoryTests
    {
        private readonly IProviderWorkerRepository _providerWorkerRepository;
        private ITestDataStore _dataStore;

        public static IEnumerable<object[]> TestPatchAsyncPatchesProviderWorkerData
        {
            get
            {
                var contractResolver = new DefaultContractResolver();
                JsonPatchDocument<ProviderWorker> documentFromOperations(params Operation<ProviderWorker>[] operations)
                {
                    return new JsonPatchDocument<ProviderWorker>(new List<Operation<ProviderWorker>>(operations), contractResolver);
                }

                yield return new object[] { documentFromOperations(new Operation<ProviderWorker> { op = "replace", path = "/Name", value = "Ned" }) };
                yield return new object[] { documentFromOperations(new Operation<ProviderWorker> { op = "replace", path = "/LastName", value = "Flanders" }) };
                yield return new object[] { documentFromOperations(new Operation<ProviderWorker> { op = "replace", path = "/LastKnownLocation", value = new GeoLocation { Longitude = -64.8, Latitude = -31 } }) };
                yield return new object[] { documentFromOperations(new Operation<ProviderWorker> { op = "replace", path = "/Status", value = ProviderWorkerStatus.Working }) };
            }
        }

        public ProviderWorkerRepositoryTests()
        {
            var mongoDatabase = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            _providerWorkerRepository = new ProviderWorkerRepository(mongoDatabase);
            _dataStore = new MongoTestDataStore(mongoDatabase);
        }



        [Fact]
        public async Task TestGetByIdAsyncCanRetrieveProviderWorker()
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
        public async Task TestUpdateAsyncFailsToCreateProviderWorkerWhenProviderDoesNotExist()
        {
            // arrange
            var providerId = "0000ffff0000ffff0000ffff0000ffff";
            var id = Guid.NewGuid().ToString("n");

            var providerWorker = new ProviderWorker
            {
                Id = id,
                ProviderId = providerId,
                Name = "Homer",
                LastName = "Simpson"
            };

            _dataStore.EnsureProviderDoesNotExist(providerId);

            // act 
            var (updateProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.UpdateAsync(providerId, providerWorker);

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
            _dataStore.EnsureProviderWorkerDoesNotExist(providerId, id);

            // act 
            var (updateProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.UpdateAsync(providerId, providerWorker);

            // assert
            Assert.Null(updateProviderWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }

        [Fact]
        public async Task TestPatchAllButProviderWorkerImageStoreAsyncPatchesProviderWorker()
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
                LastName = "Simpson",
                ProviderWorkerImageBucketKey = new ImageBucketKey { Store = Constants.WORKER_IMAGES_STORE, Bucket = Guid.NewGuid().ToString("n") }
            };

            var updatedProviderWorker = new ProviderWorker
            {
                Id = id,
                ProviderId = providerId,
                Name = "Bart",
                LastName = "Simpson",
                ProviderWorkerImageBucketKey = new ImageBucketKey { Store = Constants.WORKER_IMAGES_STORE, Bucket = Guid.NewGuid().ToString("n") }
            };

            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderWorker(providerWorker);

            // act 
            var (providerWorkerResult, outcomeAction, error) = await _providerWorkerRepository.PatchAllButProviderWorkerImageStoreAsync(providerId, updatedProviderWorker);

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
            // we must test that ProviderWorkerImageBucketKey is not changed
            Assert.Equal(providerWorker.ProviderWorkerImageBucketKey?.Store, providerWorkerResult.ProviderWorkerImageBucketKey?.Store);
            Assert.Equal(providerWorker.ProviderWorkerImageBucketKey?.Bucket, providerWorkerResult.ProviderWorkerImageBucketKey?.Bucket);

            Assert.True(
                _dataStore
                    .TestProviderWorker(_ =>
                        _.Id == updatedProviderWorker.Id &&
                        _.ProviderId == updatedProviderWorker.ProviderId &&
                        _.LastKnownLocation == updatedProviderWorker.LastKnownLocation &&
                        _.LastName == updatedProviderWorker.LastName &&
                        _.Name == updatedProviderWorker.Name &&
                        _.PhoneNumber == updatedProviderWorker.PhoneNumber &&
                        _.Status == updatedProviderWorker.Status &&
                        (
                            _.ProviderWorkerImageBucketKey != null &&
                            providerWorker.ProviderWorkerImageBucketKey != null &&
                            _.ProviderWorkerImageBucketKey.Store == providerWorker.ProviderWorkerImageBucketKey.Store &&
                            _.ProviderWorkerImageBucketKey.Bucket == providerWorker.ProviderWorkerImageBucketKey.Bucket
                        )));
        }

        [Fact]
        public async Task TestPatchAllButProviderWorkerImageStoreAsyncFailsToUpdateProviderWorkerWhenProviderDoesNotExist()
        {
            // arrange
            var providerId = "0000ffff0000ffff0000ffff0000ffff";
            var id = Guid.NewGuid().ToString("n");

            var providerWorker = new ProviderWorker
            {
                Id = id,
                ProviderId = providerId,
                Name = "Homer",
                LastName = "Simpson"
            };

            _dataStore.EnsureProviderDoesNotExist(providerId);

            // act 
            var (updateProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.PatchAllButProviderWorkerImageStoreAsync(providerId, providerWorker);

            // assert
            Assert.Null(updateProviderWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.ValidationErrorNone, outcomeAction);
            Assert.NotNull(error);
        }

        [Fact]
        public async Task TestPatchAllButProviderWorkerImageStoreAsyncReturnsNullOnNonExistantProvider()
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
            _dataStore.EnsureProviderWorkerDoesNotExist(providerId, id);

            // act 
            var (updateProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.PatchAllButProviderWorkerImageStoreAsync(providerId, providerWorker);

            // assert
            Assert.Null(updateProviderWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }

        [Fact]
        public async Task TestPatchLastKnownLocationAsyncPatchesProviderWorker()
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
                LastName = "Simpson",
                LastKnownLocation = new GeoLocation { Longitude = -67, Latitude = 32 }
            };

            var updatedProviderWorker = new ProviderWorker
            {
                Id = id,
                ProviderId = providerId,
                Name = "Bart",
                LastName = "Simpson",
                LastKnownLocation = new GeoLocation { Longitude = -68, Latitude = 31 }
            };

            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderWorker(providerWorker);

            // act 
            var (providerWorkerResult, outcomeAction, error) = await _providerWorkerRepository.PatchLastKnownLocationAsync(providerId, updatedProviderWorker);

            // assert
            Assert.NotNull(providerWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.OkUpdated, outcomeAction);
            Assert.Null(error);

            Assert.Equal(providerWorker.Id, providerWorkerResult.Id);
            Assert.Equal(providerWorker.ProviderId, providerWorkerResult.ProviderId);
            // we must test that only LastKnownLocation has changed
            Assert.Equal(updatedProviderWorker.LastKnownLocation, providerWorkerResult.LastKnownLocation);
            Assert.Equal(providerWorker.LastName, providerWorkerResult.LastName);
            Assert.Equal(providerWorker.Name, providerWorkerResult.Name);
            Assert.Equal(providerWorker.PhoneNumber, providerWorkerResult.PhoneNumber);
            Assert.Equal(providerWorker.Status, providerWorkerResult.Status);
            Assert.Equal(providerWorker.ProviderWorkerImageBucketKey?.Store, providerWorkerResult.ProviderWorkerImageBucketKey?.Store);
            Assert.Equal(providerWorker.ProviderWorkerImageBucketKey?.Bucket, providerWorkerResult.ProviderWorkerImageBucketKey?.Bucket);

            Assert.True(
                _dataStore
                    .TestProviderWorker(_ =>
                        _.Id == providerWorker.Id &&
                        _.ProviderId == providerWorker.ProviderId &&
                        _.LastKnownLocation == updatedProviderWorker.LastKnownLocation &&
                        _.LastName == providerWorker.LastName &&
                        _.Name == providerWorker.Name &&
                        _.PhoneNumber == providerWorker.PhoneNumber &&
                        _.Status == providerWorker.Status));
        }

        [Fact]
        public async Task TestPatchLastKnownLocationAsyncFailsToUpdateProviderWorkerWhenProviderDoesNotExist()
        {
            // arrange
            var providerId = "0000ffff0000ffff0000ffff0000ffff";
            var id = Guid.NewGuid().ToString("n");

            var providerWorker = new ProviderWorker
            {
                Id = id,
                ProviderId = providerId,
                Name = "Homer",
                LastName = "Simpson"
            };

            _dataStore.EnsureProviderDoesNotExist(providerId);

            // act 
            var (updateProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.PatchLastKnownLocationAsync(providerId, providerWorker);

            // assert
            Assert.Null(updateProviderWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.ValidationErrorNone, outcomeAction);
            Assert.NotNull(error);
        }

        [Fact]
        public async Task TestPatchLastKnownLocationAsyncReturnsNullOnNonExistantProvider()
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
            _dataStore.EnsureProviderWorkerDoesNotExist(providerId, id);

            // act 
            var (updateProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.PatchLastKnownLocationAsync(providerId, providerWorker);

            // assert
            Assert.Null(updateProviderWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }

        /////

        [Theory]
        [MemberData(nameof(TestPatchAsyncPatchesProviderWorkerData))]
        public async Task TestPatchAsyncPatchesProviderWorker(JsonPatchDocument<ProviderWorker> providerWorkerPatch)
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
                LastName = "Simpson",
                LastKnownLocation = new GeoLocation { Longitude = -67, Latitude = 32 },
                Status = ProviderWorkerStatus.Idle,
            };

            var expectedProviderWorker = new ProviderWorker
            {
                Id = providerWorker.Id,
                ProviderId = providerWorker.ProviderId,
                Name = providerWorker.Name,
                LastName = providerWorker.LastName,
                LastKnownLocation = providerWorker.LastKnownLocation,
                Status = providerWorker.Status
            };

            providerWorkerPatch.ApplyTo(expectedProviderWorker);
            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderWorker(providerWorker);

            // act 
            var (patchedProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.PatchAsync(providerId, id, providerWorkerPatch);

            // assert
            Assert.NotNull(patchedProviderWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.OkUpdated, outcomeAction);
            Assert.Null(error);

            Assert.Equal(expectedProviderWorker.Id, patchedProviderWorkerResult.Id);
            Assert.Equal(expectedProviderWorker.ProviderId, patchedProviderWorkerResult.ProviderId);
            Assert.Equal(expectedProviderWorker.LastKnownLocation, patchedProviderWorkerResult.LastKnownLocation);
            Assert.Equal(expectedProviderWorker.LastName, patchedProviderWorkerResult.LastName);
            Assert.Equal(expectedProviderWorker.Name, patchedProviderWorkerResult.Name);
            Assert.Equal(expectedProviderWorker.PhoneNumber, patchedProviderWorkerResult.PhoneNumber);
            Assert.Equal(expectedProviderWorker.Status, patchedProviderWorkerResult.Status);
            Assert.Equal(expectedProviderWorker.ProviderWorkerImageBucketKey?.Store, patchedProviderWorkerResult.ProviderWorkerImageBucketKey?.Store);
            Assert.Equal(expectedProviderWorker.ProviderWorkerImageBucketKey?.Bucket, patchedProviderWorkerResult.ProviderWorkerImageBucketKey?.Bucket);

            Assert.True(
                _dataStore
                    .TestProviderWorker(_ =>
                        _.Id == expectedProviderWorker.Id &&
                        _.ProviderId == expectedProviderWorker.ProviderId &&
                        (
                            (
                                _.LastKnownLocation != null && 
                                expectedProviderWorker.LastKnownLocation != null && 
                                _.LastKnownLocation.Latitude == expectedProviderWorker.LastKnownLocation.Latitude &&
                                _.LastKnownLocation.Longitude == expectedProviderWorker.LastKnownLocation.Longitude) || 
                            (_.LastKnownLocation == null && expectedProviderWorker.LastKnownLocation == null)
                        ) &&
                        _.LastName == expectedProviderWorker.LastName &&
                        _.Name == expectedProviderWorker.Name &&
                        _.PhoneNumber == expectedProviderWorker.PhoneNumber &&
                        _.Status == expectedProviderWorker.Status));
        }

        [Fact]
        public async Task TestPatchAsyncFailsToUpdateProviderWorkerWhenProviderDoesNotExist()
        {
            // arrange
            var providerId = "0000ffff0000ffff0000ffff0000ffff";
            var id = Guid.NewGuid().ToString("n");
            var providerWorkerPatch =
                new JsonPatchDocument<ProviderWorker>(
                    new List<Operation<ProviderWorker>>
                    {
                        new Operation<ProviderWorker> { path = "/lastname", op = "replace", value = "Bart" }
                    }, new DefaultContractResolver());

            _dataStore.EnsureProviderDoesNotExist(providerId);

            // act 
            var (patchProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.PatchAsync(providerId, id, providerWorkerPatch);

            // assert
            Assert.Null(patchProviderWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.ValidationErrorNone, outcomeAction);
            Assert.NotNull(error);
        }

        [Fact]
        public async Task TestPatchAsyncReturnsNullOnNonExistantProvider()
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

            var providerWorkerPatch =
                new JsonPatchDocument<ProviderWorker>(
                    new List<Operation<ProviderWorker>>
                    {
                        new Operation<ProviderWorker> { path = "/lastname", op = "replace", value = "Bart" }
                    }, new DefaultContractResolver());

            _dataStore.EnsureProvider(provider);
            _dataStore.EnsureProviderWorkerDoesNotExist(providerId, id);

            // act 
            var (patchProviderWorkerResult, outcomeAction, error) = await _providerWorkerRepository.PatchAsync(providerId, id, providerWorkerPatch);

            // assert
            Assert.Null(patchProviderWorkerResult);
            Assert.Equal(RepositoryOutcomeAction.NotFoundNone, outcomeAction);
            Assert.Null(error);
        }
    }
}
