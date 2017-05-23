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

        public async Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> GetProviderByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var provider = await (await _mongoDatabase.Providers().FindAsync(_ => _.Id == id)).SingleOrDefaultAsync();
            var outcomeAction = provider != null ? RepositoryOutcomeAction.OkNone : RepositoryOutcomeAction.NotFoundNone;
            return (provider, outcomeAction, null);
        }

        public async Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> NewProviderAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            provider.Id = Guid.NewGuid().ToString();
            await _mongoDatabase.Providers().InsertOneAsync(provider, cancellationToken: cancellationToken);
            return (provider, RepositoryOutcomeAction.OkCreated, null);
        }

        public async Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> UpdateProviderAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            var result = await _mongoDatabase.Providers().ReplaceOneAsync(_ => _.Id == provider.Id, provider);
            return
                result.MatchedCount == 1 && result.MatchedCount == result.ModifiedCount
                    ? (provider, RepositoryOutcomeAction.OkUpdated, (object)null)
                    : (null, RepositoryOutcomeAction.NotFoundNone, null);
        }
    }
}
