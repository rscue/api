using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using MongoDB.Driver;
using Rscue.Api.Models;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Route("client")]
    public class ClientController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly AzureSettings _appSettings;

        public ClientController(IMongoDatabase mongoDatabase, IOptions<AzureSettings> appSettings)
        {
            _mongoDatabase = mongoDatabase;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateClient([FromBody] Client client)
        {
            if (ModelState.IsValid)
            {
                var exists = await _mongoDatabase.GetCollection<Client>("clients").Find(x => x.Id == client.Id).SingleOrDefaultAsync();
                if (exists == null)
                {
                    await _mongoDatabase.GetCollection<Client>("clients").InsertOneAsync(client);

                    var uri = new Uri($"{Request.GetEncodedUrl()}/{client.Id}");
                    return await Task.FromResult(Created(uri, client));
                }

                await _mongoDatabase.GetCollection<Client>("clients").ReplaceOneAsync(x => x.Id == client.Id, client);
                return await Task.FromResult(Ok(client));
            }

            return await Task.FromResult(BadRequest());
        }

        [Route("{id}")]
        public async Task<IActionResult> GetClient(string id)
        {
            var client = await _mongoDatabase.GetCollection<Client>("clients").Find(x => x.Id == id).SingleOrDefaultAsync();
            if (client == null)
            {
                return await Task.FromResult(NotFound());
            }

            return await Task.FromResult(Ok(client));
        }

        [Route("profilepic/{id}")]
        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(string id, [FromBody] AvatarViewModel avatar)
        {
            var client = await _mongoDatabase.GetCollection<Client>("clients").Find(x => x.Id == id).SingleOrDefaultAsync();
            if (client == null)
            {
                return await Task.FromResult(NotFound());
            }

            var dataImage = avatar.ImageBase64.Split(',');
            var imageBytes = Convert.FromBase64String(dataImage[1]);
            var mimeString = dataImage[0].Split(':')[1].Split(';')[0];
            var extension = mimeString.Split('/')[1];
            var imageName = $"{id}.{extension}".Replace("|", "");
            var cloudStorageAccount = CloudStorageAccount.Parse(_appSettings.StorageConnectionString);
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("profilepics");
            var blockBlob = blobContainer.GetBlockBlobReference(imageName);
            await blockBlob.UploadFromByteArrayAsync(imageBytes, 0, imageBytes.Length);

            var updateDefinitition = new UpdateDefinitionBuilder<Client>().Set(x => x.AvatarUri, blockBlob.Uri);
            await _mongoDatabase.GetCollection<Client>("clients").UpdateOneAsync(x => x.Id == id, updateDefinitition);

            return Ok(blockBlob.Uri);
        }
    }
}