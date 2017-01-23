using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Authorize]
    [Route("provider/{providerId}/worker")]
    public class ProviderWorkerController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly Auth0Settings _auth0Settings;
        private readonly AzureSettings _azureSettings;
        private readonly IMongoCollection<Provider> _providerCollection;

        public ProviderWorkerController(IMongoDatabase mongoDatabase, IOptions<Auth0Settings> appSettings,
            IOptions<AzureSettings> azureSettings)
        {
            _mongoDatabase = mongoDatabase;
            _auth0Settings = appSettings.Value;
            _azureSettings = azureSettings.Value;
            _providerCollection = mongoDatabase.GetCollection<Provider>("providers");
        }

        [HttpPost]
        [ProducesResponseType(typeof(WorkerViewModel), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> AddWorker(string providerId, [FromBody] WorkerViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var provider = await _providerCollection.Find(x => x.Id == providerId).SingleOrDefaultAsync();
                    if (provider == null)
                    {
                        return await Task.FromResult(NotFound($"No existe un proveedor con el id {providerId}"));
                    }

                    model.Id = await CreateAuth0User(model);
                    var worker = new Worker
                    {
                        Id = model.Id,
                        ProviderId = provider.Id,
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

                return await Task.FromResult(BadRequest(ModelState.GetErrors()));
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

        

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<WorkerViewModel>), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetWorkers(string providerId, [FromQuery] IEnumerable<WorkerStatus> status)
        {
            var collection = _mongoDatabase.GetCollection<Worker>("workers");
            var models = collection.AsQueryable().Where(x => x.ProviderId == providerId);

            if (status.Any())
            {
                models = models.Where(x => status.Contains(x.Status));
            }

            if (!await models.AnyAsync())
            {
                return await Task.FromResult(NotFound("No hay resultados"));
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

        

        [Route("{id}/profilepic")]
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateProfilePicture(string providerId, string id,
            [FromBody] AvatarViewModel avatar)
        {
            var provider = await _mongoDatabase.GetCollection<Worker>("workers")
                        .Find(x => x.Id == id && x.ProviderId == providerId)
                        .SingleOrDefaultAsync();
            if (provider == null)
            {
                return await Task.FromResult(NotFound($"No existe un trabajador con id {id} y proveedor id {providerId}"));
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
    }
}