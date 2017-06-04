namespace Rscue.Api.Services
{
    using Extensions;
    using MongoDB.Driver;
    using Rscue.Api.Models;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class ProviderRepository : IProviderRepository
    {
        private readonly IMongoDatabase _mongoDatabase;

        public ProviderRepository(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> GetByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var provider = await (await _mongoDatabase.Providers().FindAsync(_ => _.Id == id)).SingleOrDefaultAsync();
            var outcomeAction = provider != null ? RepositoryOutcomeAction.OkNone : RepositoryOutcomeAction.NotFoundNone;
            return (provider, outcomeAction, null);
        }

        public async Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> NewAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            provider.Id = Guid.NewGuid().ToString();
            await _mongoDatabase.Providers().InsertOneAsync(provider, cancellationToken: cancellationToken);
            return (provider, RepositoryOutcomeAction.OkCreated, null);
        }

        public async Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> UpdateAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            var result = await _mongoDatabase.Providers().ReplaceOneAsync(_ => _.Id == provider.Id, provider);
            return
                result.MatchedCount == 1 && result.MatchedCount == result.ModifiedCount
                    ? (provider, RepositoryOutcomeAction.OkUpdated, (object)null)
                    : (null, RepositoryOutcomeAction.NotFoundNone, null);
        }

        public async Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> PatchAllButProviderImageStoreAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            var result =
                await _mongoDatabase.Providers()
                    .FindOneAndUpdateAsync(
                        new FilterDefinitionBuilder<Provider>()
                            .Eq(_ => _.Id, provider.Id),
                        new UpdateDefinitionBuilder<Provider>()
                            .Set(_ => _.Address, provider.Address)
                            .Set(_ => _.City, provider.City)
                            .Set(_ => _.Email, provider.Email)
                            .Set(_ => _.Name, provider.Name)
                            .Set(_ => _.State, provider.State)
                            .Set(_ => _.ZipCode, provider.ZipCode),
                        new FindOneAndUpdateOptions<Provider>() { ReturnDocument = ReturnDocument.After },
                        cancellationToken);

            var outcomeAction = result != null ? RepositoryOutcomeAction.OkUpdated : RepositoryOutcomeAction.NotFoundNone;
            return (result, outcomeAction, null);
        }
    }
}
