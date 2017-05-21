namespace Rscue.Api.Services
{
    using Rscue.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProviderRepository
    {
        Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> GetProviderByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken));

        Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> NewProviderAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken));

        Task<(Provider provider, RepositoryOutcomeAction outcomeAction, object error)> UpdateProviderAsync(Provider provider, CancellationToken cancellationToken = default(CancellationToken));
    }
}
