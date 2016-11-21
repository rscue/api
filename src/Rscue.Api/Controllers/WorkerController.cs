using System;
using System.Threading.Tasks;
using Auth0.Core;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using MongoDB.Driver;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Route("worker")]
    public class WorkerController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly Auth0Settings _auth0Settings;
        private readonly AzureSettings _azureSettings;

        public WorkerController(IMongoDatabase mongoDatabase, IOptions<Auth0Settings> appSettings, IOptions<AzureSettings> azureSettings )
        {
            _mongoDatabase = mongoDatabase;
            _auth0Settings = appSettings.Value;
            _azureSettings = azureSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> AddWorker([FromBody]Worker model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Id = await CreateAuth0User(model);
                    await _mongoDatabase.GetCollection<Worker>("workers").InsertOneAsync(model);

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

        private async Task<string> CreateAuth0User(Worker model)
        {
            var client = new ManagementApiClient(_auth0Settings.ManagementUserToken, new Uri($"https://{_auth0Settings.Domain}/api/v2"));
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
        public async Task<IActionResult> UpdateWorker(string id, [FromBody] Worker model)
        {
            if (ModelState.IsValid)
            {
                var collection = _mongoDatabase.GetCollection<Worker>("workers");
                var exists = await collection.Find(x => x.Id == id).SingleOrDefaultAsync();

                if (exists == null)
                {
                    return await Task.FromResult(NotFound());
                }                

                await collection.ReplaceOneAsync(x => x.Id == id, model);
                return await Task.FromResult(Ok());
            }

            return await Task.FromResult(BadRequest());
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkers()
        {
            var collection = _mongoDatabase.GetCollection<Worker>("workers");
            var models = collection.AsQueryable();

            if (!await models.AnyAsync())
            {
                return await Task.FromResult(NotFound());
            }

            var list = await models.ToListAsync();
            return await Task.FromResult(Ok(list));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorker(string id)
        {
            var collection = _mongoDatabase.GetCollection<Worker>("workers");
            var model = await collection.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (model == null)
            {
                return await Task.FromResult(NotFound());
            }

            return await Task.FromResult(Ok(model));
        }

        [Route("profilepic/{id}")]
        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(string id, [FromBody] AvatarViewModel avatar)
        {
            var provider = await _mongoDatabase.GetCollection<Worker>("workers").Find(x => x.Id == id).SingleOrDefaultAsync();
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
    }
}