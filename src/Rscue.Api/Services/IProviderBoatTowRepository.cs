namespace Rscue.Api.Services
{
    using Rscue.Api.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProviderBoatTowRepository
    {
        Task<(IEnumerable<ProviderBoatTow> boatTows, RepositoryOutcomeAction outcomeAction, object error)> GetAllAsync(string providerId, CancellationToken cancellationToken = default(CancellationToken));

        Task<(ProviderBoatTow boatTow, RepositoryOutcomeAction outcomeAction, object error)> GetByIdAsync(string providerId, string id, CancellationToken cancellationToken = default(CancellationToken));

        Task<(ProviderBoatTow boatTow, RepositoryOutcomeAction outcomeAction, object error)> NewAsync(string providerId, ProviderBoatTow boatTow, CancellationToken cancellationToken = default(CancellationToken));

        Task<(ProviderBoatTow boatTow, RepositoryOutcomeAction outcomeAction, object error)> UpdateAsync(string providerId, ProviderBoatTow boatTow, CancellationToken cancellationToken = default(CancellationToken));
    }
}
