using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Rscue.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rscue.Api.Services
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly IMongoDatabase _mongoDatabase;

        public AssignmentRepository(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<(Assignment assignment, RepositoryOutcome outcome, string message)> GetAssignmentByIdAsync(string id, 
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
                    result == null ? RepositoryOutcome.NotFound : RepositoryOutcome.Ok, 
                    null);
        }

        public async Task<(Assignment assignment, RepositoryOutcome outcome, string message)> NewAssignmentAsync(Assignment assignment, 
                                                                                                                 CancellationToken cancellationToken = default(CancellationToken))
        {
            if (assignment == null) throw new ArgumentNullException(nameof(assignment));

            assignment.Client = await _mongoDatabase.Clients()
                                                    .Find(x => x.Id == assignment.ClientId)
                                                    .SingleOrDefaultAsync(cancellationToken);
            if (assignment.Client == null)
            {
                return (null, RepositoryOutcome.ValidationError, $"El cliente con el id '{assignment.ClientId}' no existe");
            }

            assignment.Provider = await _mongoDatabase.Providers()
                                               .Find(x => x.Id == assignment.ProviderId)
                                               .SingleOrDefaultAsync(cancellationToken);
            if (assignment.Provider == null)
            {
                return (null, RepositoryOutcome.ValidationError, $"El proveedor con id '{assignment.ProviderId}' no existe");
            }

            assignment.Worker = await _mongoDatabase.Workers()
                                                    .Find(x => x.Id == assignment.WorkerId)
                                                    .SingleOrDefaultAsync(cancellationToken);
            if (assignment.Worker == null && !string.IsNullOrWhiteSpace(assignment.WorkerId))
            {
                return (null, RepositoryOutcome.ValidationError, $"El trabajador con id '{assignment.WorkerId}' no existe");
            }

            await _mongoDatabase.Assignments()
                                .InsertOneAsync(assignment, null, cancellationToken);
            return (assignment, RepositoryOutcome.Created, null);
        }

        public async Task<(Assignment assignment, RepositoryOutcome outcome, string message)> PatchAssignmentAddImageAsync(string id, 
                                                                                                                           string imageLocation, 
                                                                                                                           CancellationToken cancellationToken = default(CancellationToken))
        {
            var updateResult = (UpdateResult)null;
            var assignment = (Assignment)null;
            do
            {

                assignment = await _mongoDatabase.Assignments().Find(x => x.Id == id).SingleOrDefaultAsync();
                var imageUrls = assignment.ImageUrls ?? new List<string>();
                imageUrls.Add(imageLocation);
                var updateDefinitition = new UpdateDefinitionBuilder<Assignment>().Set(x => x.ImageUrls, imageUrls).Set(x => x.UpdateDateTime, DateTimeOffset.Now);
                updateResult = await _mongoDatabase.Assignments().UpdateOneAsync(x => x.Id == id
                && x.UpdateDateTime == assignment.UpdateDateTime, updateDefinitition);
            } while (updateResult.ModifiedCount == 0);
            return (assignment, RepositoryOutcome.Ok, null);
        }

        public async Task<(Assignment assignment, RepositoryOutcome outcome, string message)> UpdateAssignmentAsync(Assignment assignment, 
                                                                                                                  CancellationToken cancellationToken = default(CancellationToken))
        {
            assignment.Client = await _mongoDatabase.Clients()
                                        .Find(x => x.Id == assignment.ClientId)
                                        .SingleOrDefaultAsync(cancellationToken);
            if (assignment.Client == null)
            {
                return (null, RepositoryOutcome.ValidationError, $"El cliente con el id '{assignment.ClientId}' no existe");
            }

            assignment.Provider = await _mongoDatabase.Providers()
                                               .Find(x => x.Id == assignment.ProviderId)
                                               .SingleOrDefaultAsync(cancellationToken);
            if (assignment.Provider == null)
            {
                return (null, RepositoryOutcome.ValidationError, $"El proveedor con id '{assignment.ProviderId}' no existe");
            }

            assignment.Worker = await _mongoDatabase.Workers()
                                                    .Find(x => x.Id == assignment.WorkerId)
                                                    .SingleOrDefaultAsync(cancellationToken);
            if (assignment.Worker == null && !string.IsNullOrWhiteSpace(assignment.WorkerId))
            {
                return (null, RepositoryOutcome.ValidationError, $"El trabajador con id '{assignment.WorkerId}' no existe");
            }

            await _mongoDatabase.Assignments().ReplaceOneAsync(x => x.Id == assignment.Id, assignment);
            return (assignment, RepositoryOutcome.Ok, null);
        }

        public async Task<(IEnumerable<Assignment> assignments, RepositoryOutcome outcome, string message)> SearchAssignmentAsync(DateTimeOffset? startDateTime, 
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

            var projection = assignments.Select(_ => new { assignment = _, worker = (Worker)null, client = (Client)null });
            if (populateClient)
            {
                projection = projection.Join(_mongoDatabase.Clients(), _ => _.assignment.ClientId, _ => _.Id, (_, client) => new { assignment = _.assignment, worker = _.worker, client = client });
            }

            if (populateWorker)
            {
                projection = projection.Join(_mongoDatabase.Workers(), _ => _.assignment.WorkerId, _ => _.Id, (_, worker) => new { assignment = _.assignment, worker = worker, client = _.client });
            }

            var output = await projection.Select(
                _ => new Assignment
                {
                    Id = _.assignment.Id,
                    Comments = _.assignment.Comments,
                    ClientId = _.assignment.ClientId,
                    CreationDateTime = _.assignment.CreationDateTime,
                    EstimatedTimeOfArrival = _.assignment.EstimatedTimeOfArrival,
                    ImageUrls = _.assignment.ImageUrls,
                    Location = _.assignment.Location,
                    ProviderId = _.assignment.ProviderId,
                    Status = _.assignment.Status,
                    UpdateDateTime = _.assignment.UpdateDateTime,
                    WorkerId = _.assignment.WorkerId,
                    Client = _.client,
                    Worker = _.worker
                }).ToListAsync();

            return (output, RepositoryOutcome.Ok, null);
        }
    }
}
