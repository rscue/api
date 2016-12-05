using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.Linq;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Route("assignment")]
    public class AssignmentController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ProviderAppSettings _providerAppSettings;

        public AssignmentController(IMongoDatabase mongoDatabase, IOptions<ProviderAppSettings> providerAppSettings)
        {
            _mongoDatabase = mongoDatabase;
            _providerAppSettings = providerAppSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> AddAssignment([FromBody] AssignmentViewModel assignment)
        {
            if (ModelState.IsValid)
            {
                var client = await _mongoDatabase.GetCollection<Client>("clients").Find(x => x.Id == assignment.ClientId).SingleOrDefaultAsync();
                if (client == null)
                {
                    return await Task.FromResult(NotFound($"Client with id {assignment.ClientId} was not found"));
                }

                var provider = await _mongoDatabase.GetCollection<Provider>("providers").Find(x => x.Id == assignment.ProviderId).SingleOrDefaultAsync();
                if (provider == null)
                {
                    return await Task.FromResult(NotFound($"Provider with id {assignment.ProviderId} was not found"));
                }

                var worker = await _mongoDatabase.GetCollection<Worker>("workers").Find(x => x.Id == assignment.WorkerId).SingleOrDefaultAsync();
                if (worker == null && !string.IsNullOrWhiteSpace(assignment.WorkerId))
                {
                    return await Task.FromResult(NotFound($"Worker with id {assignment.WorkerId} was not found"));
                }

                var model = new Assignment
                {
                    ProviderId = assignment.ProviderId,
                    ClientId = assignment.ClientId,
                    CreationDateTime = assignment.CreationDateTime,
                    Location = new GeoJson2DGeographicCoordinates(assignment.Longitude, assignment.Latitude),
                    Status = assignment.Status
                };

                if (!string.IsNullOrWhiteSpace(assignment.WorkerId))
                {
                    model.WorkerId = assignment.WorkerId;
                }

                await _mongoDatabase.GetCollection<Assignment>("assignments").InsertOneAsync(model);
                var uri = new Uri($"{Request.GetEncodedUrl()}/{model.Id.ToString()}");
                assignment.Id = model.Id.ToString();

                if (worker != null)
                {
                    var payload = PushNotificationHelpers.GetNewAssignmentPayload(worker.DeviceId, model.Id);
                    PushNotificationHelpers.Send(_providerAppSettings.ApplicationId, _providerAppSettings.SenderId, payload);
                }

                return await Task.FromResult(Created(uri, assignment));
            }

            return await Task.FromResult(BadRequest());
        }

        [HttpPut]
        [Route("{id}/status")]
        public async Task<IActionResult> UpdateAssignment(string id, [FromBody] AssignmentViewModel assignment)
        {
            var model = await _mongoDatabase.GetCollection<Assignment>("assignments").Find(x => x.Id == id).SingleOrDefaultAsync();

            if (model == null)
            {
                return await Task.FromResult(NotFound());
            }

            var worker = await _mongoDatabase.GetCollection<Worker>("workers").Find(x => x.Id == assignment.WorkerId).SingleOrDefaultAsync();
            if (worker == null && !string.IsNullOrWhiteSpace(assignment.WorkerId))
            {
                return await Task.FromResult(NotFound($"Worker with id {assignment.WorkerId} was not found"));
            }

            if (!string.IsNullOrWhiteSpace(assignment.WorkerId))
            {
                model.WorkerId = assignment.WorkerId;
                var payload = PushNotificationHelpers.GetNewAssignmentPayload(worker.DeviceId, model.Id);
                PushNotificationHelpers.Send(_providerAppSettings.ApplicationId, _providerAppSettings.SenderId, payload);
            }

            model.Status = assignment.Status;
            model.UpdateDateTime = assignment.UpdateDateTime;

            await _mongoDatabase.GetCollection<Assignment>("assignments").ReplaceOneAsync(x => x.Id == id, model);

            return await Task.FromResult(Ok());
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchAssignments([FromQuery] AssignmentSearchViewModel search)
        {
            var collection = _mongoDatabase.GetCollection<Assignment>("assignments");
            var workerCollection = _mongoDatabase.GetCollection<Worker>("workers");
            var clientCollection = _mongoDatabase.GetCollection<Client>("clients");

            var query = from assignment in collection.AsQueryable()
                        join worker in workerCollection on assignment.WorkerId equals worker.Id into workers
                        join client in clientCollection on assignment.ClientId equals client.Id into clients
                        select new AssignmentViewModel
                        {
                            Id = assignment.Id,
                            WorkerName = workers.First().Name + " " + workers.First().LastName,
                            ClientName = clients.First().Name + " " + clients.First().LastName,
                            CreationDateTime = assignment.CreationDateTime,
                            Status = assignment.Status
                        };

            if (search.StartDateTime.HasValue)
            {
                query = query.Where(x => x.CreationDateTime > search.StartDateTime.Value);
            }
            if (search.EndDateTime.HasValue)
            {
                query = query.Where(x => x.CreationDateTime < search.EndDateTime.Value);
            }
            if (search.Statuses != null && search.Statuses.Any())
            {
                query = query.Where(x => search.Statuses.Contains(x.Status));
            }

            var assignments = await query.ToListAsync();

            return await Task.FromResult(Ok(assignments));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetAssignment(string id)
        {
            var collection = _mongoDatabase.GetCollection<Assignment>("assignments");
            var workerCollection = _mongoDatabase.GetCollection<Worker>("workers");
            var clientCollection = _mongoDatabase.GetCollection<Client>("clients");

            var query = from assignment in collection.AsQueryable()
                        where assignment.Id == id
                        join worker in workerCollection on assignment.WorkerId equals worker.Id into workers
                        join client in clientCollection on assignment.ClientId equals client.Id into clients
                        select new
                        {
                            assignment.Id,
                            WorkerName = workers.First().Name + " " + workers.First().LastName,
                            ClientName = clients.First().Name + " " + clients.First().LastName,
                            assignment.CreationDateTime,
                            assignment.Status,
                            assignment.Location,
                            ClientAvatarUri = clients.First().AvatarUri,
                            ClientId = clients.First().Id
                        };

            var result = await query.SingleOrDefaultAsync();

            if (result == null)
            {
                return await Task.FromResult(NotFound());
            }

            var model = new AssignmentViewModel
            {
                Id = result.Id,
                Status = result.Status,
                CreationDateTime = result.CreationDateTime,
                ClientName = result.ClientName,
                WorkerName = result.WorkerName,
                Latitude = result.Location.Latitude,
                Longitude = result.Location.Longitude,
                ClientAvatarUri = result.ClientAvatarUri == null ? "assets/img/nobody.jpg" : result.ClientAvatarUri.ToString(),
                ClientId = result.ClientId
            };

            return await Task.FromResult(Ok(model));
        }

        [HttpPut]
        [Route("{id}/status/{status}")]
        public async Task<IActionResult> UpdateAssignmentStatus(string id, AssignmentStatus status)
        {
            var collection = _mongoDatabase.GetCollection<Assignment>("assignments");
            var assignment = await collection.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (assignment == null)
            {
                return await Task.FromResult(NotFound());
            }

            assignment.Status = status;
            await collection.ReplaceOneAsync(x => x.Id == id, assignment);

            return await Task.FromResult(Ok());
        }
    }
}