using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Rscue.Api.Hubs;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.ViewModels;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Rscue.Api.Controllers
{
    [Route("worker")]
    public class WorkerController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IConnectionManager _connectionManager;
        private readonly IMongoCollection<Provider> _providerCollection;

        public WorkerController(IMongoDatabase mongoDatabase, IConnectionManager connectionManager)
        {
            _mongoDatabase = mongoDatabase;
            _connectionManager = connectionManager;
            _providerCollection = mongoDatabase.GetCollection<Provider>("providers");
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateWorker(string id, [FromBody] WorkerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var collection = _mongoDatabase.GetCollection<Worker>("workers");
                var exists = await collection.Find(x => x.Id == id).SingleOrDefaultAsync();

                if (exists == null)
                {
                    return await Task.FromResult(NotFound($"No existe un trabajador con id {id}"));
                }

                var provider = await _providerCollection.Find(x => x.Id == exists.ProviderId).SingleOrDefaultAsync();
                if (provider == null)
                {
                    return await Task.FromResult(NotFound($"No existe un proveedor con id {exists.ProviderId}"));
                }

                var worker = new Worker
                {
                    Id = model.Id,
                    ProviderId = provider.Id,
                    Name = model.Name,
                    LastName = model.LastName,
                    AvatarUri = model.AvatarUri,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    DeviceId = exists.DeviceId,
                    Status = model.Status
                };

                var providerId = worker.ProviderId;
                _connectionManager.GetHubContext<WorkersHub>().Clients.User(providerId).updateWorkerStatus(id, model.Status.ToString());


                await collection.ReplaceOneAsync(x => x.Id == id, worker);
                return await Task.FromResult(Ok());
            }

            return await Task.FromResult(BadRequest(ModelState.GetErrors()));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(WorkerViewModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetWorker(string id)
        {
            var collection = _mongoDatabase.GetCollection<Worker>("workers");
            var worker = await collection.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (worker == null)
            {
                return await Task.FromResult(NotFound($"No existe un trabajador con id {id}"));
            }

            var model = new WorkerViewModel
            {
                Id = worker.Id,
                Name = worker.Name,
                DeviceId = worker.DeviceId,
                PhoneNumber = worker.PhoneNumber,
                AvatarUri = worker.AvatarUri,
                Email = worker.Email,
                LastName = worker.LastName,
                Location = worker.Location == null ? null : new LocationViewModel { Latitude = worker.Location.Latitude, Longitude = worker.Location.Longitude },
                Status = worker.Status
            };

            return await Task.FromResult(Ok(model));
        }

        [Route("{id}/location")]        
        [HttpPut]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateWorkerCurrentLocation(string id, [FromBody] LocationViewModel location)
        {
            var collection = _mongoDatabase.GetCollection<Worker>("workers");
            var worker = await collection.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (worker == null)
            {
                return await Task.FromResult(NotFound($"No existe un trabajador con id {id}"));
            }

            var providerId = worker.ProviderId;
            _connectionManager.GetHubContext<WorkersHub>().Clients.User(providerId).updateWorkerLocation(id, location);

            var updatedLocation = new GeoJson2DGeographicCoordinates(location.Longitude, location.Latitude);
            var updateDefinitition = new UpdateDefinitionBuilder<Worker>().Set(x => x.Location, updatedLocation);
            await collection.UpdateOneAsync(x => x.Id == id, updateDefinitition);

            return await Task.FromResult(Ok());
        }

        [Route("{id}/registerdevice")]
        [HttpPut]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> AddUpdateDeviceId(string id, [FromBody] DeviceRegistrationViewModel device)
        {
            var worker = await _mongoDatabase.GetCollection<Worker>("workers").Find(x => x.Id == id).SingleOrDefaultAsync();
            if (worker == null)
            {
                return await Task.FromResult(NotFound($"No existe un trabajador con id {id}"));
            }

            var updateDefinitition = new UpdateDefinitionBuilder<Worker>().Set(x => x.DeviceId, device.DeviceId);
            await _mongoDatabase.GetCollection<Worker>("workers").UpdateOneAsync(x => x.Id == id, updateDefinitition);

            return await Task.FromResult(Ok());
        }
    }
}
