namespace Rscue.Api.Services
{
    using Rscue.Api.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProviderWorkerRepository
    {
        Task<(ProviderWorker providerWorker, RepositoryOutcomeAction outcomeAction, object error)> GetByIdAsync(string providerId, string id, CancellationToken cancellationToken = default(CancellationToken));

        Task<(IEnumerable<ProviderWorker> providerWorkers, RepositoryOutcomeAction outcomeAction, object error)> GetAllAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken));

        Task<(ProviderWorker providerWorker, RepositoryOutcomeAction outcomeAction, object error)> NewAsync(string providerId, ProviderWorker providerWorker, CancellationToken cancellationToken = default(CancellationToken));

        Task<(ProviderWorker providerWorker, RepositoryOutcomeAction outcomeAction, object error)> UpdateAsync(string providerId, ProviderWorker providerWorker, CancellationToken cancellationToken = default(CancellationToken));

        Task<(ProviderWorker providerWorker, RepositoryOutcomeAction outcomeAction, object error)> PatchAllButProviderWorkerImageStoreAsync(string providerId, ProviderWorker providerWorker, CancellationToken cancellationToken = default(CancellationToken));

        Task<(ProviderWorker providerWorker, RepositoryOutcomeAction outcomeAction, object error)> PatchLastKnownLocationAsync(string providerId, ProviderWorker providerWorker, CancellationToken cancellationToken = default(CancellationToken));
    }
}
