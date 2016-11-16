using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Rscue.Api.Models;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Route("fleet")]
    public class FleetController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<Fleet> _collection;

        public FleetController(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _collection = _mongoDatabase.GetCollection<Fleet>("fleets");
        }

        [HttpPost]
        public async Task<IActionResult> AddFleet([FromBody] FleetViewModel fleet)
        {
            if (ModelState.IsValid)
            {
                var model = new Fleet
                {
                    BoatModel = fleet.BoatModel,
                    EngineType = fleet.EngineType,
                    Name = fleet.Name,
                    RegistrationNumber = fleet.RegistrationNumber
                };

                await _collection.InsertOneAsync(model);
                return await Task.FromResult(Ok());
            }

            return await Task.FromResult(BadRequest());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFleet(string id, [FromBody] FleetViewModel fleet)
        {
            if (ModelState.IsValid)
            {
                var objId = ObjectId.Parse(id);
                var exists = await _collection.Find(x => x.Id == objId).SingleOrDefaultAsync();

                if (exists == null)
                {
                    return await Task.FromResult(NotFound());
                }

                var model = new Fleet
                {
                    BoatModel = fleet.BoatModel,
                    EngineType = fleet.EngineType,
                    Name = fleet.Name,
                    RegistrationNumber = fleet.RegistrationNumber,
                    Id = exists.Id
                };

                await _collection.ReplaceOneAsync(x => x.Id == objId, model);
                return await Task.FromResult(Ok());
            }

            return await Task.FromResult(BadRequest());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFleet(string id)
        {
            var objId = ObjectId.Parse(id);
            var fleet = await _collection.Find(x => x.Id == objId).SingleOrDefaultAsync();

            if (fleet == null)
            {
                return await Task.FromResult(NotFound());
            }

            var model = new FleetViewModel
            {
                Id = fleet.Id.ToString(),
                Name = fleet.Name,
                RegistrationNumber = fleet.RegistrationNumber,
                BoatModel = fleet.BoatModel,
                EngineType = fleet.EngineType
            };

            return await Task.FromResult(Ok(model));
        }

        [HttpGet]
        public async Task<IActionResult> GetFleets()
        {
            var models = _collection.AsQueryable().ToList().Select(x => new FleetViewModel
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                RegistrationNumber = x.RegistrationNumber,
                EngineType = x.EngineType,
                BoatModel = x.BoatModel
            });

            if (!models.Any())
            {
                return await Task.FromResult(NotFound());
            }

            return await Task.FromResult(Ok(models));
        }
    }
}