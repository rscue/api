namespace Rscue.Api.Services
{
    using Rscue.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAssignmentRepository
    {
        Task<(Assignment assignment, RepositoryOutcomeAction outcomeAction, object error)> GetAssignmentByIdAsync(string id, bool populateClient = false, bool populateWorker = false, CancellationToken cancellationToken = default(CancellationToken));

        Task<(Assignment assignment, RepositoryOutcomeAction outcomeAction, object error)> NewAssignmentAsync(Assignment assignment, CancellationToken cancellationToken = default(CancellationToken));

        Task<(Assignment assignment, RepositoryOutcomeAction outcomeAction, object error)> UpdateAssignmentAsync(Assignment assignment, CancellationToken cancellationToken = default(CancellationToken));

        Task<(IEnumerable<Assignment> assignments, RepositoryOutcomeAction outcomeAction, object error)> SearchAssignmentAsync(DateTimeOffset? startDateTime, DateTimeOffset? endDateTime, IEnumerable<AssignmentStatus> statuses, bool populateClient = false, bool populateWorker = false, CancellationToken cancellationToken = default(CancellationToken));
    }
}
