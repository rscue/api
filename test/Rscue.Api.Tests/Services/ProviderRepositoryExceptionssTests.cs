namespace Rscue.Api.Tests.Services
{
    using MongoDB.Driver;
    using Moq;
    using Rscue.Api.Services;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ProviderRepository")]
    public class ProviderRepositoryExceptionssTests
    {
        private readonly IMongoDatabase _mongoDatabase;

        public ProviderRepositoryExceptionssTests()
        {
            _mongoDatabase = new Mock<IMongoDatabase>().Object;
        }

        [Fact]
        public void ProviderRepositoryShouldNotBuildWithNullMongoDatabase()
        {
            // arrange
            // nothing to do

            // act (deferred)
            var testCode = (Action)(() => new ProviderRepository(null));

            // assert
            Assert.Throws<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task GetByIdAsyncShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var providerRepository = new ProviderRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerRepository.GetByIdAsync(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task NewAsyncShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var providerRepository = new ProviderRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerRepository.NewAsync(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task UpdateAsyncShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var providerRepository = new ProviderRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerRepository.UpdateAsync(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task PatchAllButProviderImageStoreAsyncShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var providerRepository = new ProviderRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerRepository.PatchAllButProviderImageStoreAsync(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }
    }
}
