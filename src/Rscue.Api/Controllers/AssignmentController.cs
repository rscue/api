using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.Linq;
using Rscue.Api.Models;
using Rscue.Api.ViewModels;
using Enumerable = System.Linq.Enumerable;

namespace Rscue.Api.Controllers
{
    [Route("assignment")]
    public class AssignmentController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;

        public AssignmentController(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
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
    }
}