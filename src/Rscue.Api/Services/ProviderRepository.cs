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
            var provider = await (await _mongoDatabase.Providers().FindAsync(_ => _.Id == id)).SingleOrDefaultAsync();
            var outcomeAction = provider != null ? RepositoryOutcomeAction.OkNone : RepositoryOutcomeAction.NotFoundNone;
            return (provider, outcomeAction, null);
        }

        public Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> NewProviderAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> UpdateProviderAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
