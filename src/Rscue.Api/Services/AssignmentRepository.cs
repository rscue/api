namespace Rscue.Api.Services
{
    using Extensions;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using Rscue.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly IMongoDatabase _mongoDatabase;

        public AssignmentRepository(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<(Assignment assignment, RepositoryOutcomeAction outcomeAction, object error)> GetAssignmentByIdAsync(string id, 
                                                                                                                               bool populateClient = false, 
                                                                                                                               bool populateWorker = false, 
                                                                                                                               CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var result = await _mongoDatabase.Assignments()
                                             .Find(x => x.Id == id)
                                             .SingleOrDefaultAsync(cancellationToken);
            if (result != null)
            {
                if (populateClient)
                {
                    result.Client = await _mongoDatabase.Clients()
                                                        .Find(x => x.Id == result.ClientId)
                                                        .SingleOrDefaultAsync(cancellationToken);
                }

                if (populateWorker)
                {
                    result.Worker = await _mongoDatabase.Workers()
                                                        .Find(x => x.Id == result.WorkerId)
                                                        .SingleOrDefaultAsync(cancellationToken);
                }
            }

            return (result, 
                    result == null ? RepositoryOutcomeAction.NotFoundNone : RepositoryOutcomeAction.OkNone, 
                    null);
        }

        public async Task<(Assignment assignment, RepositoryOutcomeAction outcomeAction, object error)> NewAssignmentAsync(Assignment assignment, 
                                                                                                                           CancellationToken cancellationToken = default(CancellationToken))
        {
            if (assignment == null) throw new ArgumentNullException(nameof(assignment));

            if (assignment.Status != AssignmentStatus.Created && assignment.Status != AssignmentStatus.Assigned)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, "An assignment can only be created in status 'Created' or 'Assigned'");
            }

            object error;
            if ((error = ValidateStatusAndStatusReason(assignment.Status, assignment.StatusReason) )!= null)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, error);
            }

            if ((error = ValidateStatusData(assignment)) != null)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, error);
            }

            if ((error = await PopulateAssignmentDependants(assignment, cancellationToken)) != null)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, error);
            }

            assignment.Id = Guid.NewGuid().ToString("n");

            await _mongoDatabase.Assignments()
                                .InsertOneAsync(assignment, null, cancellationToken);

            return (assignment, RepositoryOutcomeAction.OkCreated, null);
        }

        public async Task<(Assignment assignment, RepositoryOutcomeAction outcomeAction, object error)> UpdateAssignmentAsync(Assignment assignment, 
                                                                                                                              CancellationToken cancellationToken = default(CancellationToken))
        {
            if (assignment.Status == AssignmentStatus.Created)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, "An assignment cannot be updated to status 'Created'");
            }

            object error;
            if ((error = ValidateStatusAndStatusReason(assignment.Status, assignment.StatusReason)) != null)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, error);
            }

            if ((error = ValidateStatusData(assignment)) != null)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, error);
            }

            if ((error = await PopulateAssignmentDependants(assignment, cancellationToken)) != null)
            {
                return (null, RepositoryOutcomeAction.ValidationErrorNone, error);
            }

            var provider = assignment.Provider;
            var client = assignment.Client;
            var worker = assignment.Worker;
            var boatTow = assignment.BoatTow;

            assignment = await _mongoDatabase
                .Assignments()
                .FindOneAndUpdateAsync(
                    x => x.Id == assignment.Id,
                    new UpdateDefinitionBuilder<Assignment>()
                        .Set(_ => _.ClientId, assignment.ClientId)
                        .Set(_ => _.ProviderId, assignment.ProviderId)
                        .Set(_ => _.WorkerId, assignment.WorkerId)
                        .Set(_ => _.BoatTowId, assignment.BoatTowId)
                        .Set(_ => _.Comments, assignment.Comments)
                        .Set(_ => _.EstimatedTimeOfArrival, assignment.EstimatedTimeOfArrival)
                        .Set(_ => _.InitialLocation, assignment.InitialLocation)
                        .Set(_ => _.ServiceLocation, assignment.ServiceLocation)
                        .Set(_ => _.Status, assignment.Status)
                        .Set(_ => _.StatusReason, assignment.StatusReason)
                        .Set(_ => _.UpdateDateTime, assignment.UpdateDateTime));

            assignment.Provider = provider;
            assignment.Client = client;
            assignment.Worker = worker;
            assignment.BoatTow = boatTow;

            return (assignment, RepositoryOutcomeAction.OkUpdated, null);
        }

        public async Task<(IEnumerable<Assignment> assignments, RepositoryOutcomeAction outcomeAction, object error)> SearchAssignmentAsync(DateTimeOffset? startDateTime,
                                                                                                                                            DateTimeOffset? endDateTime,
                                                                                                                                            IEnumerable<AssignmentStatus> statuses,
                                                                                                                                            bool populateClient = false,
                                                                                                                                            bool populateWorker = false,
                                                                                                                                            CancellationToken cancellationToken = default(CancellationToken))
        {
            var assignments = _mongoDatabase.Assignments().AsQueryable();
            if (startDateTime.HasValue)
            {
                assignments = assignments.Where(x => x.CreationDateTime > startDateTime.Value);
            }
            if (endDateTime.HasValue)
            {
                assignments = assignments.Where(x => x.CreationDateTime < endDateTime.Value);
            }
            if (statuses != null && statuses.Any())
            {
                assignments = assignments.Where(x => statuses.Contains(x.Status));
            }

            assignments = assignments.OrderBy(_ => _.CreationDateTime);

            var tasks = new Task[] { Task.CompletedTask, Task.CompletedTask };
            var clients = (Dictionary<string, Client>)null;
            var workers = (Dictionary<string, ProviderWorker>)null;

            var output = await assignments.ToListAsync();
            if (populateClient)
            {
                var clientIds = output.Where(_ => _.ClientId != null).Select(_ => _.ClientId).ToList();
                tasks[0] =
                    _mongoDatabase.Clients().FindAsync(_ => clientIds.Contains(_.Id))
                        .ContinueWith(_ => _.Result.ToListAsync(), TaskContinuationOptions.OnlyOnRanToCompletion)
                        .ContinueWith(_ => clients = _.Result.Result.ToDictionary(__ => __.Id), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            else
            {
                clients = new Dictionary<string, Client>();
            }

            if (populateWorker)
            {
                var workerIds = output.Where(_ => _.WorkerId != null).Select(_ => _.WorkerId).ToList();
                tasks[1] =
                    _mongoDatabase.Workers().FindAsync(_ => workerIds.Contains(_.Id))
                        .ContinueWith(_ => _.Result.ToListAsync(), TaskContinuationOptions.OnlyOnRanToCompletion)
                        .ContinueWith(_ => workers = _.Result.Result.ToDictionary(__ => __.Id), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            else
            {
                workers = new Dictionary<string, ProviderWorker>();
            }

            await Task.WhenAll(tasks);

            foreach (var item in output)
            {
                item.Client = clients.GetValueOrDefault(item.ClientId);
                item.Worker = workers.GetValueOrDefault(item.WorkerId);
            }
            
            return (output, RepositoryOutcomeAction.OkNone, null);
        }

        private static object ValidateStatusAndStatusReason(AssignmentStatus status, AssignmentStatusReason reason)
        {
            switch (status)
            {
                case AssignmentStatus.Created:
                    if (reason != AssignmentStatusReason.New)
                    {
                        return "An assignment in status 'Created' must have StatusReason set to 'New'";
                    }

                    break;
                case AssignmentStatus.Cancelled:
                    if (reason != AssignmentStatusReason.CancelledByProvider &&
                        reason != AssignmentStatusReason.CancelledByUser &&
                        reason != AssignmentStatusReason.CancelledByWorker)
                    {
                        return "An assignment in status 'Cancelled' must have StatusReason set to 'CancelledByProvider', 'CancelledByUser' or 'CancelledByWorker'";
                    }

                    break;
                case AssignmentStatus.InProgress:
                    if (reason != AssignmentStatusReason.WorkerEnRoute)
                    {
                        return "An assignment in status 'InProgress' must have StatusReason set to 'WorkerEnRoute'";
                    }

                    break;
                case AssignmentStatus.Completed:
                    if (reason != AssignmentStatusReason.ServiceCompleted &&
                        reason != AssignmentStatusReason.ClosedBySystem)
                        
                    {
                        return "An assignment in status 'Completed' must have StatusReason set to 'ServiceCompleted' or 'ClosedBySystem'";
                    }

                    break;
                case AssignmentStatus.Assigned:
                    if (reason == AssignmentStatusReason.WorkerAssigned)
                    {
                        return "An assignment in status 'Assigned' must have StatusReason set to 'WorkerAssigned'";
                    }

                    break;
            }

            return null;
        }

        private static object ValidateStatusData(Assignment assignment)
        {
            if (assignment.Status == AssignmentStatus.Assigned)
            {
                if (assignment.WorkerId == null)
                {
                    return "An assignment in status 'Assigned' must have a worker assigned";
                }

                if (assignment.BoatTowId == null)
                {
                    return "An assignment in status 'Assigned' must have a BoatTow assigned";
                }
            }

            return null;
        }

        private async Task<object> PopulateAssignmentDependants(Assignment assignment, CancellationToken cancellationToken)
        {
            assignment.Client = await _mongoDatabase.Clients()
                                        .Find(x => x.Id == assignment.ClientId)
                                        .SingleOrDefaultAsync(cancellationToken);
            if (assignment.Client == null)
            {
                return $"Client with id '{assignment.ClientId}' does not exist.";
            }

            assignment.Provider = await _mongoDatabase.Providers()
                                                      .Find(x => x.Id == assignment.ProviderId)
                                                      .SingleOrDefaultAsync(cancellationToken);
            if (assignment.Provider == null)
            {
                return $"Provider with id '{assignment.ProviderId}' does not exist.";
            }

            if (assignment.WorkerId != null)
            {
                assignment.Worker = await _mongoDatabase.Workers()
                                                        .Find(x => x.Id == assignment.WorkerId)
                                                        .SingleOrDefaultAsync(cancellationToken);
                if (assignment.Worker == null)
                {
                    return $"ProviderWorker with id '{assignment.WorkerId}' does not exist.";
                }
            }

            if (assignment.BoatTowId != null)
            {
                assignment.BoatTow = await _mongoDatabase.BoatTows()
                                                        .Find(x => x.Id == assignment.BoatTowId)
                                                        .SingleOrDefaultAsync(cancellationToken);

                if (assignment.BoatTow == null)
                {
                    return $"ProviderBoatTow with id '{assignment.BoatTowId}' does not exist.";
                }
            }

            return null;
        }
    }
}
