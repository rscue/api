﻿namespace Rscue.Api.Services
{
    using Extensions;
    using MongoDB.Driver;
    using Rscue.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class ProviderWorkerRepository : IProviderWorkerRepository
    {
        private readonly IMongoDatabase _mongoDatabase;

        public ProviderWorkerRepository(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<(IEnumerable<ProviderWorker> providerWorkers, RepositoryOutcomeAction outcomeAction, object error)> GetAllAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (providerId == null) throw new ArgumentNullException(nameof(providerId));
            var result = await(
                            await _mongoDatabase.Workers()
                                                .FindAsync(_ => _.ProviderId == providerId, cancellationToken: cancellationToken)
                            ).ToListAsync(cancellationToken);
            return (result, RepositoryOutcomeAction.OkNone, null);
        }

        public async Task<(ProviderWorker providerWorker, RepositoryOutcomeAction outcomeAction, object error)> GetByIdAsync(string providerId, string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (providerId == null) throw new ArgumentNullException(nameof(providerId));
            if (id == null) throw new ArgumentNullException(nameof(id));
            var result = await(
                            await _mongoDatabase.Workers()
                                                .FindAsync(_ => _.ProviderId == providerId && _.Id == id, cancellationToken: cancellationToken)
                            ).SingleOrDefaultAsync(cancellationToken);
            var outcomeAction = result != null ? RepositoryOutcomeAction.OkNone : RepositoryOutcomeAction.NotFoundNone;
            return (result, outcomeAction, null);
        }

        public async Task<(ProviderWorker providerWorker, RepositoryOutcomeAction outcomeAction, object error)> NewAsync(string providerId, ProviderWorker providerWorker, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (providerId == null) throw new ArgumentNullException(nameof(providerId));
            if (providerWorker == null) throw new ArgumentNullException(nameof(providerWorker));

            var providerExists = await(await _mongoDatabase.Providers().FindAsync(_ => _.Id == providerId, cancellationToken: cancellationToken)).AnyAsync(cancellationToken);
            if (!providerExists)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, new { cause = "Missing Provider", data = providerId });
            }

            providerWorker.Id = Guid.NewGuid().ToString("n");
            providerWorker.ProviderId = providerId;

            await _mongoDatabase
                .Workers()
                .InsertOneAsync(providerWorker, cancellationToken: cancellationToken);
            return (providerWorker, RepositoryOutcomeAction.OkCreated, null);
        }

        public async Task<(ProviderWorker providerWorker, RepositoryOutcomeAction outcomeAction, object error)> UpdateAsync(string providerId, ProviderWorker providerWorker, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (providerId == null) throw new ArgumentNullException(nameof(providerId));
            if (providerWorker == null) throw new ArgumentNullException(nameof(providerWorker));

            var providerExists = await(await _mongoDatabase.Providers().FindAsync(_ => _.Id == providerId, cancellationToken: cancellationToken)).AnyAsync(cancellationToken);
            if (!providerExists)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, new { cause = "Missing Provider", data = providerId });
            }

            providerWorker.ProviderId = providerId;

            var result = await _mongoDatabase
                .Workers()
                .ReplaceOneAsync(_ => _.Id == providerWorker.Id, providerWorker, cancellationToken: cancellationToken);

            return
                result.MatchedCount == 1 && result.MatchedCount == result.ModifiedCount
                    ? (providerWorker, RepositoryOutcomeAction.OkUpdated, null)
                    : ((ProviderWorker)null, RepositoryOutcomeAction.NotFoundNone, (object)null);
        }
    }
}
