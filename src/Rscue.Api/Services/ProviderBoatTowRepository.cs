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
            var outcomeAction = result != null ? RepositoryOutcomeAction.OkNone : RepositoryOutcomeAction.NotFoundNone;
            return (result, outcomeAction, null);
        }

        public async Task<(ProviderBoatTow boatTow, RepositoryOutcomeAction outcomeAction, object error)> NewAsync(string providerId, ProviderBoatTow boatTow, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (providerId == null) throw new ArgumentNullException(nameof(providerId));
            if (boatTow == null) throw new ArgumentNullException(nameof(boatTow));

            var providerExists = await (await _mongoDatabase.Providers().FindAsync(_ => _.Id == providerId, cancellationToken: cancellationToken)).AnyAsync(cancellationToken);
            if (!providerExists)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, new { cause = "Missing Provider", data = providerId });
            }

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

            var providerExists = await (await _mongoDatabase.Providers().FindAsync(_ => _.Id == providerId, cancellationToken: cancellationToken)).AnyAsync(cancellationToken);
            if (!providerExists)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, new { cause = "Missing Provider", data = providerId });
            }

            boatTow.ProviderId = providerId;

            var result = await _mongoDatabase
                .BoatTows()
                .ReplaceOneAsync(_ => _.Id == boatTow.Id, boatTow, cancellationToken: cancellationToken);

            return
                result.MatchedCount == 1 && result.MatchedCount == result.ModifiedCount
                    ? (boatTow, RepositoryOutcomeAction.OkUpdated, null)
                    : ((ProviderBoatTow)null, RepositoryOutcomeAction.NotFoundNone, (object)null);
        }
    }
}
