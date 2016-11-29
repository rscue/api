﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.Linq;
using Rscue.Api.Hubs;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Route("provider/{providerId}/worker")]
    [Route("worker")]
    public class ProviderWorkerController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IConnectionManager _connectionManager;
        private readonly Auth0Settings _auth0Settings;
        private readonly AzureSettings _azureSettings;
        private readonly IMongoCollection<Provider> _providerCollection;

        public ProviderWorkerController(IMongoDatabase mongoDatabase, IOptions<Auth0Settings> appSettings,
            IOptions<AzureSettings> azureSettings, IConnectionManager connectionManager)
        {
            _mongoDatabase = mongoDatabase;
            _connectionManager = connectionManager;
            _auth0Settings = appSettings.Value;
            _azureSettings = azureSettings.Value;
            _providerCollection = mongoDatabase.GetCollection<Provider>("providers");
        }

        [HttpPost]
        public async Task<IActionResult> AddWorker(string providerId, [FromBody] WorkerViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var provider = await _providerCollection.Find(x => x.Id == providerId).SingleOrDefaultAsync();
                    if (provider == null)
                    {
                        return await Task.FromResult(NotFound("PROVIDER"));
                    }

                    model.Id = await CreateAuth0User(model);
                    var worker = new Worker
                    {
                        Id = model.Id,
                        Provider = new MongoDBRef("providers", provider.Id),
                        Name = model.Name,
                        LastName = model.LastName,
                        AvatarUri = model.AvatarUri,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber
                    };

                    await _mongoDatabase.GetCollection<Worker>("workers").InsertOneAsync(worker);

                    var uri = new Uri($"{Request.GetEncodedUrl()}/{model.Id}");
                    return await Task.FromResult(Created(uri, model));
                }

                return await Task.FromResult(BadRequest());
            }
            catch (Exception ex)
            {
                return await Task.FromResult(BadRequest(ex.Message));
            }
        }

        private async Task<string> CreateAuth0User(WorkerViewModel model)
        {
            var client = new ManagementApiClient(_auth0Settings.ManagementUserToken,
                new Uri($"https://{_auth0Settings.Domain}/api/v2"));
            var userRequest = new UserCreateRequest
            {
                Password = model.Password,
                Connection = Auth0Settings.Connection,
                FirstName = model.Name,
                LastName = model.LastName,
                Email = model.Email
            };

            var result = await client.Users.CreateAsync(userRequest);
            return result.UserId;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorker(string id, [FromBody] WorkerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var collection = _mongoDatabase.GetCollection<Worker>("workers");
                var exists = await collection.Find(x => x.Id == id).SingleOrDefaultAsync();

                if (exists == null)
                {
                    return await Task.FromResult(NotFound());
                }

                var provider = await _providerCollection.Find(x => x.Id == exists.Provider.Id).SingleOrDefaultAsync();
                if (provider == null)
                {
                    return await Task.FromResult(NotFound("PROVIDER"));
                }

                var worker = new Worker
                {
                    Id = model.Id,
                    Provider = new MongoDBRef("providers", provider.Id),
                    Name = model.Name,
                    LastName = model.LastName,
                    AvatarUri = model.AvatarUri,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    DeviceId = exists.DeviceId,
                    Status = model.Status
                };

                var providerId = worker.Provider.Id.ToString();
                _connectionManager.GetHubContext<WorkersHub>().Clients.User(providerId).updateWorkerStatus(id, model.Status.ToString());


                await collection.ReplaceOneAsync(x => x.Id == id, worker);
                return await Task.FromResult(Ok());
            }

            return await Task.FromResult(BadRequest());
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkers(string providerId)
        {
            var collection = _mongoDatabase.GetCollection<Worker>("workers");
            var models = collection.Find(x => x.Provider.Id == providerId);

            if (!await models.AnyAsync())
            {
                return await Task.FromResult(NotFound());
            }

            var list = await models.ToListAsync();
            var ret = new List<WorkerViewModel>();
            foreach (var worker in list)
            {
                ret.Add(new WorkerViewModel
                {
                    Id = worker.Id,
                    Name = worker.Name,
                    LastName = worker.LastName,
                    PhoneNumber = worker.PhoneNumber,
                    AvatarUri = worker.AvatarUri,
                    Email = worker.Email,
                    DeviceId = worker.DeviceId,
                    Location = worker.Location == null ? null : new LocationViewModel { Latitude = worker.Location.Latitude, Longitude = worker.Location.Longitude },
                    Status = worker.Status
                });
            }
            return await Task.FromResult(Ok(ret));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorker(string id)
        {
            var collection = _mongoDatabase.GetCollection<Worker>("workers");
            var worker = await collection.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (worker == null)
            {
                return await Task.FromResult(NotFound());
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

        [Route("profilepic/{id}")]
        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(string providerId, string id,
            [FromBody] AvatarViewModel avatar)
        {
            var provider =
                await
                    _mongoDatabase.GetCollection<Worker>("workers")
                        .Find(x => x.Id == id && x.Provider.Id == providerId)
                        .SingleOrDefaultAsync();
            if (provider == null)
            {
                return await Task.FromResult(NotFound());
            }

            var dataImage = avatar.ImageBase64.Split(',');
            var imageBytes = Convert.FromBase64String(dataImage[1]);
            var mimeString = dataImage[0].Split(':')[1].Split(';')[0];
            var extension = mimeString.Split('/')[1];
            var imageName = $"{id}.{extension}".Replace("|", "");
            var cloudStorageAccount = CloudStorageAccount.Parse(_azureSettings.StorageConnectionString);
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("profilepics");
            var blockBlob = blobContainer.GetBlockBlobReference(imageName);
            await blockBlob.UploadFromByteArrayAsync(imageBytes, 0, imageBytes.Length);

            var updateDefinitition = new UpdateDefinitionBuilder<Worker>().Set(x => x.AvatarUri, blockBlob.Uri);
            await _mongoDatabase.GetCollection<Worker>("workers").UpdateOneAsync(x => x.Id == id, updateDefinitition);

            return await Task.FromResult(Ok(blockBlob.Uri));
        }

        [Route("{id}/location")]
        [HttpPut]
        public async Task<IActionResult> UpdateWorkerCurrentLocation(string id, [FromBody] LocationViewModel location)
        {
            var collection = _mongoDatabase.GetCollection<Worker>("workers");
            var worker = await collection.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (worker == null)
            {
                return await Task.FromResult(NotFound());
            }

            var providerId = worker.Provider.Id.ToString();
            _connectionManager.GetHubContext<WorkersHub>().Clients.User(providerId).updateWorkerLocation(id, location);

            var updatedLocation = new GeoJson2DGeographicCoordinates(location.Longitude, location.Latitude);
            var updateDefinitition = new UpdateDefinitionBuilder<Worker>().Set(x => x.Location, updatedLocation);
            await collection.UpdateOneAsync(x => x.Id == id, updateDefinitition);

            return await Task.FromResult(Ok());
        }
    }
}