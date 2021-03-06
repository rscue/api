﻿namespace Rscue.Api.Tests.Services
{
    using MongoDB.Driver;
    using Moq;
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ProviderWorkerRepository")]
    public class ProviderWorkerRepositoryExceptionTests
    {
        private readonly IMongoDatabase _mongoDatabase;

        public ProviderWorkerRepositoryExceptionTests()
        {
            _mongoDatabase = new Mock<IMongoDatabase>().Object;
        }

        [Fact]
        public void ProviderWorkerRepositoryShouldNotBuildWithNullMongoDatabase()
        {
            // arrange
            // nothing to do

            // act (deferred)
            var testCode = (Action)(() => new ProviderWorkerRepository(null));

            // assert
            Assert.Throws<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task GetByIdAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var id = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.GetByIdAsync(null, id));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task GetByIdAsyncShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerId = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.GetByIdAsync(providerId, null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task GetAllAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.GetAllAsync(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task NewAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerWorker = new ProviderWorker();

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.NewAsync(null, providerWorker));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }
        [Fact]
        public async Task NewAsyncShouldThrowArgumentNullExceptionOnNullProviderWorker()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerId = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.NewAsync(providerId, null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task UpdateAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerWorker = new ProviderWorker();

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.UpdateAsync(null, providerWorker));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task UpdateAsyncShouldThrowArgumentNullExceptionOnNullProviderWorker()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerId = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.UpdateAsync(providerId, null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task PatchAllButProviderWorkerImageStoreAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerWorker = new ProviderWorker();

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.PatchAllButProviderWorkerImageStoreAsync(null, providerWorker));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task PatchAllButProviderWorkerImageStoreAsyncShouldThrowArgumentNullExceptionOnNullProviderWorker()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerId = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.PatchAllButProviderWorkerImageStoreAsync(providerId, null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task PatchLastKnownLocationAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerWorker = new ProviderWorker();

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.PatchLastKnownLocationAsync(null, providerWorker));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task PatchLastKnownLocationAsyncShouldThrowArgumentNullExceptionOnNullProviderWorker()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerId = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.PatchLastKnownLocationAsync(providerId, null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task PatchAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var id = Guid.NewGuid().ToString("n");
            var providerWorkerPatch = new Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<ProviderWorker>();

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.PatchAsync(null, id, providerWorkerPatch));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>("providerId", testCode);
        }

        [Fact]
        public async Task PatchAyncShouldThrowArgumentNullExceptionOnNullId()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerId = Guid.NewGuid().ToString("n");
            var providerWorkerPatch = new Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<ProviderWorker>();

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.PatchAsync(providerId, null, providerWorkerPatch));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>("id", testCode);
        }

        [Fact]
        public async Task PatchAyncShouldThrowArgumentNullExceptionOnNullProviderWorkerPatch()
        {
            // arrange
            var providerWorkerRepository = new ProviderWorkerRepository(_mongoDatabase);
            var providerId = Guid.NewGuid().ToString("n");
            var id = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerWorkerRepository.PatchAsync(providerId, id, null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>("providerWorkerPatch", testCode);
        }
    }
}
