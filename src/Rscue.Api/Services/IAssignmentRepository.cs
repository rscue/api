using Rscue.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rscue.Api.Services
{
    public interface IAssignmentRepository
    {
        Task<(Assignment assignment, RepositoryOutcome outcome, string message)> GetAssignmentByIdAsync(string id, bool populateClient = false, bool populateWorker = false, CancellationToken cancellationToken = default(CancellationToken));

        Task<(Assignment assignment, RepositoryOutcome outcome, string message)> NewAssignmentAsync(Assignment assignment, CancellationToken cancellationToken = default(CancellationToken));

        Task<(Assignment assignment, RepositoryOutcome outcome, string message)> SaveAssignmentAsync(Assignment assignment, CancellationToken cancellationToken = default(CancellationToken));

        Task<(Assignment assignment, RepositoryOutcome outcome, string message)> PatchAssignmentAddImageAsync(string id, string images, CancellationToken cancellationToken = default(CancellationToken));

        Task<(IEnumerable<Assignment> assignments, RepositoryOutcome outcome, string message)> SearchAssignmentAsync(DateTimeOffset? startDateTime, DateTimeOffset? endDateTime, IEnumerable<AssignmentStatus> statuses, bool populateClient = false, bool populateWorker = false, CancellationToken cancellationToken = default(CancellationToken));
    }
}
