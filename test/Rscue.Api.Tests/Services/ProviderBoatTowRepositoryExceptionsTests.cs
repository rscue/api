namespace Rscue.Api.Tests.Services
{
    using MongoDB.Driver;
    using Moq;
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ProviderBoatTowRepository")]
    public class ProviderBoatTowRepositoryExceptionsTests
    {
        private readonly IMongoDatabase _mongoDatabase;

        public ProviderBoatTowRepositoryExceptionsTests()
        {
            _mongoDatabase = new Mock<IMongoDatabase>().Object;
        }

        [Fact]
        public void ProviderBoatTowRepositoryShouldNotBuildWithNullMongoDatabase()
        {
            // arrange
            // nothing to do

            // act (deferred)
            var testCode = (Action)(() => new ProviderBoatTowRepository(null));

            // assert
            Assert.Throws<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task GetByIdAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerBoatTowRepository = new ProviderBoatTowRepository(_mongoDatabase);
            var id = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerBoatTowRepository.GetByIdAsync(null, id));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task GetByIdAsyncShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var providerBoatTowRepository = new ProviderBoatTowRepository(_mongoDatabase);
            var providerId = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerBoatTowRepository.GetByIdAsync(providerId, null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task GetAllAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerBoatTowRepository = new ProviderBoatTowRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerBoatTowRepository.GetAllAsync(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task NewAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerBoatTowRepository = new ProviderBoatTowRepository(_mongoDatabase);
            var providerBoatTow = new ProviderBoatTow();

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerBoatTowRepository.NewAsync(null, providerBoatTow));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }
        [Fact]
        public async Task NewAsyncShouldThrowArgumentNullExceptionOnNullProviderBoatTow()
        {
            // arrange
            var providerBoatTowRepository = new ProviderBoatTowRepository(_mongoDatabase);
            var providerId = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerBoatTowRepository.NewAsync(providerId, null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task UpdateAsyncShouldThrowArgumentNullExceptionOnNullProviderId()
        {
            // arrange
            var providerBoatTowRepository = new ProviderBoatTowRepository(_mongoDatabase);
            var providerBoatTow = new ProviderBoatTow();

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerBoatTowRepository.UpdateAsync(null, providerBoatTow));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task UpdateAsyncShouldThrowArgumentNullExceptionOnNullProviderBoatTow()
        {
            // arrange
            var providerBoatTowRepository = new ProviderBoatTowRepository(_mongoDatabase);
            var providerId = Guid.NewGuid().ToString("n");

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerBoatTowRepository.UpdateAsync(providerId, null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

    }
}
