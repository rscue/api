namespace Rscue.Api.Services
{
    using Extensions;
    using MongoDB.Driver;
    using Rscue.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class ProviderBoatTowRepository : IProviderBoatTowRepository
    {
        private readonly IMongoDatabase _mongoDatabase;

        public ProviderBoatTowRepository(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<(IEnumerable<ProviderBoatTow> boatTows, RepositoryOutcomeAction outcomeAction, object error)> GetAllAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (providerId == null) throw new ArgumentNullException(nameof(providerId));
            var result = await (
                            await _mongoDatabase.BoatTows()
                                                .FindAsync(_ => _.ProviderId == providerId, cancellationToken: cancellationToken)
                            ).ToListAsync(cancellationToken);
            return (result, RepositoryOutcomeAction.OkNone, null);
        }

        public async Task<(ProviderBoatTow boatTow, RepositoryOutcomeAction outcomeAction, object error)> GetByIdAsync(string providerId, string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (providerId == null) throw new ArgumentNullException(nameof(providerId));
            if (id == null) throw new ArgumentNullException(nameof(id));
            var result = await (
                            await _mongoDatabase.BoatTows()
                                                .FindAsync(_ => _.ProviderId == providerId && _.Id == id, cancellationToken: cancellationToken)
                            ).SingleOrDefaultAsync(cancellationToken);
            return (result, RepositoryOutcomeAction.OkNone, null);
        }

        public async Task<(ProviderBoatTow boatTow, RepositoryOutcomeAction outcomeAction, object error)> NewAsync(string providerId, ProviderBoatTow boatTow, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (providerId == null) throw new ArgumentNullException(nameof(providerId));
            if (boatTow == null) throw new ArgumentNullException(nameof(boatTow));
            boatTow.Id = Guid.NewGuid().ToString("n");
            boatTow.ProviderId = providerId;
            await _mongoDatabase
                .BoatTows()
                .InsertOneAsync(boatTow, cancellationToken: cancellationToken);
            return (boatTow, RepositoryOutcomeAction.OkCreated, null);
        }

        public async Task<(ProviderBoatTow boatTow, RepositoryOutcomeAction outcomeAction, object error)> UpdateAsync(string providerId, ProviderBoatTow boatTow, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (providerId == null) throw new ArgumentNullException(nameof(providerId));
            if (boatTow == null) throw new ArgumentNullException(nameof(boatTow));
            var result = await _mongoDatabase
                .BoatTows()
                .ReplaceOneAsync(_ => _.Id == boatTow.Id, boatTow, cancellationToken: cancellationToken);
            var outcomeAction =
                result.MatchedCount == 1 && result.MatchedCount == result.ModifiedCount
                    ? RepositoryOutcomeAction.OkUpdated
                    : RepositoryOutcomeAction.NotFoundNone;

            return (boatTow, outcomeAction, null);
        }
    }
}
