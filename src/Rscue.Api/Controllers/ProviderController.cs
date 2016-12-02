using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.ViewModels;
using Enumerable = System.Linq.Enumerable;

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
            var provider = await _mongoDatabase.GetCollection<Provider>("providers").Find(x => x.Id == id).SingleOrDefaultAsync();
            if (provider == null)
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

        [HttpGet]
        [Route("{id}/assignmentssummary")]
        public async Task<IActionResult> GetAssignmentsSummary(string id)
        {
            var collection = _mongoDatabase.GetCollection<Assignment>("assignments");
            var beginDay = DateTimeOffset.Now - DateTimeOffset.Now.TimeOfDay;
            var assignmentsCreated = await collection.Find(x => x.ProviderId == id && x.CreationDateTime > beginDay && (x.Status == AssignmentStatus.Created || x.Status == AssignmentStatus.Assigned)).CountAsync();
            var assignmentsInProgress = await collection.Find(x => x.ProviderId == id && x.CreationDateTime > beginDay && x.Status == AssignmentStatus.InProgress).CountAsync();
            var assignmentsCompleted = await collection.Find(x => x.ProviderId == id && x.CreationDateTime > beginDay && x.Status == AssignmentStatus.Completed).CountAsync();
            var assignmentsCancelled = await collection.Find(x => x.ProviderId == id && x.CreationDateTime > beginDay && x.Status == AssignmentStatus.Cancelled).CountAsync();

            var assignmentsSummary = new AssignmentsSummaryViewModel
            {
                Created = assignmentsCreated,
                InProgress = assignmentsInProgress,
                Completed = assignmentsCompleted,
                Cancelled = assignmentsCancelled
            };

            return await Task.FromResult(Ok(assignmentsSummary));
        }
    }

    public class AssignmentNotificationViewModel
    {
        public string Id { get; set; }
        public DateTimeOffset CreationDateTime { get; set; }
        public string ClientName { get; set; }
    }
}