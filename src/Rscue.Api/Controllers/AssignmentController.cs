using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Rscue.Api.Models;
using Rscue.Api.ViewModels;

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

                var model = new Assignment
                {
                    Provider = new MongoDBRef("providers", assignment.ProviderId),
                    Client = new MongoDBRef("clients", assignment.ClientId),
                    CreationDateTime = assignment.CreationDateTime,
                    Location = new GeoJson2DGeographicCoordinates(assignment.Longitude, assignment.Latitude),
                    Status = assignment.Status
                };

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
            var objId = ObjectId.Parse(id);
            var model = await _mongoDatabase.GetCollection<Assignment>("assignments").Find(x => x.Id == objId).SingleOrDefaultAsync();

            if (model == null)
            {
                return await Task.FromResult(NotFound());
            }

            model.Status = assignment.Status;
            model.UpdateDateTime = assignment.UpdateDateTime;

            await _mongoDatabase.GetCollection<Assignment>("assignments").ReplaceOneAsync(x => x.Id == objId, model);

            return await Task.FromResult(Ok());
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchAssignments([FromQuery] AssignmentSearchViewModel search)
        {
            var collection = _mongoDatabase.GetCollection<Assignment>("assignments");
            var builder = Builders<Assignment>.Filter;
            var filter = builder.Empty;
            if (search.StartDateTime.HasValue)
            {
                filter = filter & builder.Gte(x => x.CreationDateTime, search.StartDateTime.Value);
            }
            if (search.EndDateTime.HasValue)
            {
                filter = filter & builder.Lte(x => x.CreationDateTime, search.EndDateTime.Value);
            }
            if (search.Statuses != null)
            {
                filter = filter & builder.In(x => x.Status, search.Statuses);
            }

            var assignments = await collection.Find(filter).ToListAsync();

            if (assignments.Count == 0)
            {
                return await Task.FromResult(NotFound());
            }
           
            return await Task.FromResult(Ok(assignments));
        }        
    }
}