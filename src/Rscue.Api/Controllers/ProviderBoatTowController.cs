using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Authorize]
    [Route("provider/{providerId}/boattow")]
    public class ProviderBoatTowController : Controller
    {
        private readonly IMongoCollection<BoatTow> _collection;
        private readonly IMongoCollection<Provider> _providerCollection;

        public ProviderBoatTowController(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<BoatTow>("boattows");
            _providerCollection = mongoDatabase.GetCollection<Provider>("providers");
        }

        [HttpPost]
        [ProducesResponseType(typeof(BoatTowViewModel), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> AddBoatTow(string providerId, [FromBody] BoatTowViewModel boatTow)
        {
            if (ModelState.IsValid)
            {
                var provider = await _providerCollection.Find(x => x.Id == providerId).SingleOrDefaultAsync();

                if (provider == null)
                {
                    return await Task.FromResult(NotFound($"No existe un proveedor con el id {providerId}"));
                }

                var model = new BoatTow
                {
                    BoatModel = boatTow.BoatModel,
                    EngineType = boatTow.EngineType,
                    Name = boatTow.Name,
                    RegistrationNumber = boatTow.RegistrationNumber,
                    ProviderId = provider.Id
                };

                await _collection.InsertOneAsync(model);
                var uri = new Uri($"{Request.GetEncodedUrl()}/{model.Id.ToString()}");
                boatTow.Id = model.Id.ToString();
                return await Task.FromResult(Created(uri, boatTow));
            }

            return await Task.FromResult(BadRequest(ModelState.GetErrors()));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateBoatTow(string providerId, string id, [FromBody] BoatTowViewModel boatTow)
        {
            if (ModelState.IsValid)
            {
                var provider = await _providerCollection.Find(x => x.Id == providerId).SingleOrDefaultAsync();

                if (provider == null)
                {
                    return await Task.FromResult(NotFound($"No existe un proveedor con el id {providerId}"));
                }

                var objId = ObjectId.Parse(id);
                var exists = await _collection.Find(x => x.Id == objId).SingleOrDefaultAsync();

                if (exists == null)
                {
                    return await Task.FromResult(NotFound($"No existe una float con el id {id}"));
                }

                var model = new BoatTow
                {
                    BoatModel = boatTow.BoatModel,
                    EngineType = boatTow.EngineType,
                    Name = boatTow.Name,
                    RegistrationNumber = boatTow.RegistrationNumber,
                    Id = exists.Id,
                    ProviderId = provider.Id
                };

                await _collection.ReplaceOneAsync(x => x.Id == objId, model);
                return await Task.FromResult(Ok());
            }

            return await Task.FromResult(BadRequest(ModelState.GetErrors()));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BoatTowViewModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetBoatTow(string providerId, string id)
        {
            var objId = ObjectId.Parse(id);
            var boatTow = await _collection.Find(x => x.Id == objId && x.ProviderId == providerId).SingleOrDefaultAsync();

            if (boatTow == null)
            {
                return await Task.FromResult(NotFound($"No existe una float con el id {id}"));
            }

            var model = new BoatTowViewModel
            {
                Id = boatTow.Id.ToString(),
                Name = boatTow.Name,
                RegistrationNumber = boatTow.RegistrationNumber,
                BoatModel = boatTow.BoatModel,
                EngineType = boatTow.EngineType
            };

            return await Task.FromResult(Ok(model));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BoatTowViewModel>), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetBoatTows(string providerId)
        {
            var models = _collection.AsQueryable().Where(x => x.ProviderId == providerId).ToList().Select(x => new BoatTowViewModel
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                RegistrationNumber = x.RegistrationNumber,
                EngineType = x.EngineType,
                BoatModel = x.BoatModel
            });

            if (!models.Any())
            {
                return await Task.FromResult(NotFound("No hay resultados"));
            }

            return await Task.FromResult(Ok(models));
        }
    }
}