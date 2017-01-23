﻿using System;
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
        [ProducesResponseType(typeof(FleetViewModel), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> AddFleet(string providerId, [FromBody] FleetViewModel fleet)
        {
            if (ModelState.IsValid)
            {
                var provider = await _providerCollection.Find(x => x.Id == providerId).SingleOrDefaultAsync();

                if (provider == null)
                {
                    return await Task.FromResult(NotFound($"No existe un proveedor con el id {providerId}"));
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
                var uri = new Uri($"{Request.GetEncodedUrl()}/{model.Id.ToString()}");
                fleet.Id = model.Id.ToString();
                return await Task.FromResult(Created(uri, fleet));
            }

            return await Task.FromResult(BadRequest(ModelState.GetErrors()));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateFleet(string providerId, string id, [FromBody] FleetViewModel fleet)
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

            return await Task.FromResult(BadRequest(ModelState.GetErrors()));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FleetViewModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetFleet(string providerId, string id)
        {
            var objId = ObjectId.Parse(id);
            var fleet = await _collection.Find(x => x.Id == objId && x.ProviderId == providerId).SingleOrDefaultAsync();

            if (fleet == null)
            {
                return await Task.FromResult(NotFound($"No existe una float con el id {id}"));
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
        [ProducesResponseType(typeof(IEnumerable<FleetViewModel>), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
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
                return await Task.FromResult(NotFound("No hay resultados"));
            }

            return await Task.FromResult(Ok(models));
        }
    }
}