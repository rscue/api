using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Rscue.Api.Models;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Route("provider/{providerId}/fleet")]
    public class ProviderFleetController : Controller
    {
        private readonly IMongoCollection<Fleet> _collection;
        private readonly IMongoCollection<Provider> _providerCollection;

        public ProviderFleetController(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<Fleet>("fleets");
            _providerCollection = mongoDatabase.GetCollection<Provider>("providers");
        }

        [HttpPost]
        public async Task<IActionResult> AddFleet(string providerId, [FromBody] FleetViewModel fleet)
        {
            if (ModelState.IsValid)
            {
                var provider = await _providerCollection.Find(x => x.Id == providerId).SingleOrDefaultAsync();

                if (provider == null)
                {
                    return await Task.FromResult(NotFound("PROVIDER"));
                }

                var model = new Fleet
                {
                    BoatModel = fleet.BoatModel,
                    EngineType = fleet.EngineType,
                    Name = fleet.Name,
                    RegistrationNumber = fleet.RegistrationNumber,
                    ProviderId = provider.Id
                };

                await _collection.InsertOneAsync(model);
                return await Task.FromResult(Ok());
            }

            return await Task.FromResult(BadRequest());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFleet(string providerId, string id, [FromBody] FleetViewModel fleet)
        {
            if (ModelState.IsValid)
            {
                var provider = await _providerCollection.Find(x => x.Id == providerId).SingleOrDefaultAsync();

                if (provider == null)
                {
                    return await Task.FromResult(NotFound("PROVIDER"));
                }

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
                    Id = exists.Id,
                    ProviderId = provider.Id
                };

                await _collection.ReplaceOneAsync(x => x.Id == objId, model);
                return await Task.FromResult(Ok());
            }

            return await Task.FromResult(BadRequest());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFleet(string providerId, string id)
        {
            var objId = ObjectId.Parse(id);
            var fleet = await _collection.Find(x => x.Id == objId && x.ProviderId == providerId).SingleOrDefaultAsync();

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
        public async Task<IActionResult> GetFleets(string providerId)
        {
            var models = _collection.AsQueryable().Where(x => x.ProviderId == providerId).ToList().Select(x => new FleetViewModel
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