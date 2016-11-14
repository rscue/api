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
    [Route("provider")]
    public class ProviderController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly AzureSettings _appSettings;

        public ProviderController(IMongoDatabase mongoDatabase, IOptions<AzureSettings> appSettings)
        {
            _mongoDatabase = mongoDatabase;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> AddProvider([FromBody] Provider provider)
        {
            if (ModelState.IsValid)
            {
                var exists = await _mongoDatabase.GetCollection<Provider>("providers").Find(x => x.Id == provider.Id).SingleOrDefaultAsync();
                if (exists == null)
                {
                    await _mongoDatabase.GetCollection<Provider>("providers").InsertOneAsync(provider);

                    var uri = new Uri($"{Request.GetEncodedUrl()}/{provider.Id}");
                    return await Task.FromResult(Created(uri, provider));
                }

                await _mongoDatabase.GetCollection<Provider>("providers").ReplaceOneAsync(x => x.Id == provider.Id, provider);
                return await Task.FromResult(Ok(provider));
            }

            return await Task.FromResult(BadRequest());
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetProvider(string id)
        {
            var provider = await _mongoDatabase.GetCollection<Provider>("providers").Find(x => x.Id == id).SingleOrDefaultAsync();
            if (provider == null)
            {
                return await Task.FromResult(NotFound());
            }

            return await Task.FromResult(Ok(provider));
        }

        [Route("profilepic/{id}")]
        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(string id, [FromBody] AvatarViewModel avatar)
        {
            var client = await _mongoDatabase.GetCollection<Provider>("providers").Find(x => x.Id == id).SingleOrDefaultAsync();
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

            var updateDefinitition = new UpdateDefinitionBuilder<Provider>().Set(x => x.AvatarUri, blockBlob.Uri);
            await _mongoDatabase.GetCollection<Provider>("providers").UpdateOneAsync(x => x.Id == id, updateDefinitition);

            return await Task.FromResult(Ok(blockBlob.Uri));
        }
    }
}