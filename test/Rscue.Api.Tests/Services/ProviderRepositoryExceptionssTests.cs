namespace Rscue.Api.Tests.Services
{
    using MongoDB.Driver;
    using Moq;
    using Rscue.Api.Services;
    using System;
    using System.Collections.Generic;
    using System.Text;
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
        public async Task GetImageBucketShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var providerRepository = new ProviderRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerRepository.GetProviderByIdAsync(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task NewImageBucketShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var providerRepository = new ProviderRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerRepository.NewProviderAsync(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

        [Fact]
        public async Task UpdateImageBucketShouldThrowArgumentNullExceptionOnNullKey()
        {
            // arrange
            var providerRepository = new ProviderRepository(_mongoDatabase);

            // act (deferred)
            var testCode = (Func<Task>)(async () => await providerRepository.UpdateProviderAsync(null));

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(testCode);
        }

    }
}
