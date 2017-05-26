namespace Rscue.Api.Services
{
    using Rscue.Api.Models;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProviderRepository
    {
        Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> GetByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken));

        Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> NewAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken));

        Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> UpdateAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken));

        Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> PatchAllButProviderImageStoreAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken));
    }
}
