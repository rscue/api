using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.Linq;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Route("assignment")]
    public class AssignmentController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ProviderAppSettings _providerAppSettings;
        private readonly AzureSettings _azureSettings;

        public AssignmentController(IMongoDatabase mongoDatabase, IOptions<ProviderAppSettings> providerAppSettings, IOptions<AzureSettings> azureSettings)
        {
            _mongoDatabase = mongoDatabase;
            _providerAppSettings = providerAppSettings.Value;
            _azureSettings = azureSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> AddAssignment([FromBody] AssignmentViewModel assignment)
        {
            if (ModelState.IsValid)
            {
                var client = await _mongoDatabase.GetCollection<Client>("clients").Find(x => x.Id == assignment.ClientId).SingleOrDefaultAsync();
                if (client == null)
                {
                    return await Task.FromResult(NotFound($"Client with id {assignment.ClientId} was not found"));
                }

                var provider = await _mongoDatabase.GetCollection<Provider>("providers").Find(x => x.Id == assignment.ProviderId).SingleOrDefaultAsync();
                if (provider == null)
                {
                    return await Task.FromResult(NotFound($"Provider with id {assignment.ProviderId} was not found"));
                }

                var worker = await _mongoDatabase.GetCollection<Worker>("workers").Find(x => x.Id == assignment.WorkerId).SingleOrDefaultAsync();
                if (worker == null && !string.IsNullOrWhiteSpace(assignment.WorkerId))
                {
                    return await Task.FromResult(NotFound($"Worker with id {assignment.WorkerId} was not found"));
                }

                var model = new Assignment
                {
                    ProviderId = assignment.ProviderId,
                    ClientId = assignment.ClientId,
                    CreationDateTime = assignment.CreationDateTime,
                    Location = new GeoJson2DGeographicCoordinates(assignment.Longitude, assignment.Latitude),
                    Status = assignment.Status
                };

                if (!string.IsNullOrWhiteSpace(assignment.WorkerId))
                {
                    model.WorkerId = assignment.WorkerId;
                }

                await _mongoDatabase.GetCollection<Assignment>("assignments").InsertOneAsync(model);
                var uri = new Uri($"{Request.GetEncodedUrl()}/{model.Id.ToString()}");
                assignment.Id = model.Id.ToString();

                if (worker != null)
                {
                    var payload = PushNotificationHelpers.GetNewAssignmentPayload(worker.DeviceId, model.Id);
                    PushNotificationHelpers.Send(_providerAppSettings.ApplicationId, _providerAppSettings.SenderId, payload);
                }

                return await Task.FromResult(Created(uri, assignment));
            }

            return await Task.FromResult(BadRequest());
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateAssignment(string id, [FromBody] AssignmentViewModel assignment)
        {
            if (ModelState.IsValid)
            {
                var model = await _mongoDatabase.GetCollection<Assignment>("assignments")
                            .Find(x => x.Id == id)
                            .SingleOrDefaultAsync();

                if (model == null)
                {
                    return await Task.FromResult(NotFound());
                }

                var worker = await _mongoDatabase.GetCollection<Worker>("workers")
                            .Find(x => x.Id == assignment.WorkerId)
                            .SingleOrDefaultAsync();

                if (worker == null && !string.IsNullOrWhiteSpace(assignment.WorkerId))
                {
                    return await Task.FromResult(NotFound($"Worker with id {assignment.WorkerId} was not found"));
                }

                if (!string.IsNullOrWhiteSpace(assignment.WorkerId))
                {
                    model.WorkerId = assignment.WorkerId;
                    var payload = PushNotificationHelpers.GetNewAssignmentPayload(worker.DeviceId, model.Id);
                    PushNotificationHelpers.Send(_providerAppSettings.ApplicationId, _providerAppSettings.SenderId,
                        payload);
                }

                model.Status = assignment.Status;
                model.UpdateDateTime = assignment.UpdateDateTime;
                model.Comments = assignment.Comments;

                await _mongoDatabase.GetCollection<Assignment>("assignments").ReplaceOneAsync(x => x.Id == id, model);

                return await Task.FromResult(Ok());
            }

            return await Task.FromResult(BadRequest());
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchAssignments([FromQuery] AssignmentSearchViewModel search)
        {
            var collection = _mongoDatabase.GetCollection<Assignment>("assignments");
            var workerCollection = _mongoDatabase.GetCollection<Worker>("workers");
            var clientCollection = _mongoDatabase.GetCollection<Client>("clients");

            var query = from assignment in collection.AsQueryable()
                        join worker in workerCollection on assignment.WorkerId equals worker.Id into workers
                        join client in clientCollection on assignment.ClientId equals client.Id into clients
                        select new AssignmentViewModel
                        {
                            Id = assignment.Id,
                            WorkerName = workers.First().Name + " " + workers.First().LastName,
                            ClientName = clients.First().Name + " " + clients.First().LastName,
                            CreationDateTime = assignment.CreationDateTime,
                            Status = assignment.Status
                        };

            if (search.StartDateTime.HasValue)
            {
                query = query.Where(x => x.CreationDateTime > search.StartDateTime.Value);
            }
            if (search.EndDateTime.HasValue)
            {
                query = query.Where(x => x.CreationDateTime < search.EndDateTime.Value);
            }
            if (search.Statuses != null && search.Statuses.Any())
            {
                query = query.Where(x => search.Statuses.Contains(x.Status));
            }

            var assignments = await query.ToListAsync();

            return await Task.FromResult(Ok(assignments));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetAssignment(string id)
        {
            var collection = _mongoDatabase.GetCollection<Assignment>("assignments");
            var workerCollection = _mongoDatabase.GetCollection<Worker>("workers");
            var clientCollection = _mongoDatabase.GetCollection<Client>("clients");

            var query = from assignment in collection.AsQueryable()
                        where assignment.Id == id
                        join worker in workerCollection on assignment.WorkerId equals worker.Id into workers
                        join client in clientCollection on assignment.ClientId equals client.Id into clients
                        select new
                        {
                            assignment.Id,
                            WorkerName = workers.First().Name + " " + workers.First().LastName,
                            ClientName = clients.First().Name + " " + clients.First().LastName,
                            assignment.CreationDateTime,
                            assignment.Status,
                            assignment.Location,
                            ClientAvatarUri = clients.First().AvatarUri,
                            ClientId = clients.First().Id,
                            assignment.WorkerId,
                            assignment.ProviderId,
                            assignment.Comments,
                            assignment.ImageUrls
                        };

            var result = await query.SingleOrDefaultAsync();

            if (result == null)
            {
                return await Task.FromResult(NotFound());
            }

            var model = new AssignmentViewModel
            {
                Id = result.Id,
                Status = result.Status,
                CreationDateTime = result.CreationDateTime,
                ClientName = result.ClientName,
                WorkerName = result.WorkerName,
                Latitude = result.Location.Latitude,
                Longitude = result.Location.Longitude,
                ClientAvatarUri = result.ClientAvatarUri == null ? "assets/img/nobody.jpg" : result.ClientAvatarUri.ToString(),
                ClientId = result.ClientId,
                ProviderId = result.ProviderId,
                Comments = result.Comments,
                ImageUrls = result.ImageUrls
            };

            return await Task.FromResult(Ok(model));
        }

        [Route("{id}/incidentpic")]
        [HttpPost]
        public async Task<IActionResult> AddIncidentImage(string id, [FromBody] AvatarViewModel avatar)
        {
            var assignment = await _mongoDatabase.GetCollection<Assignment>("assignments").Find(x => x.Id == id).SingleOrDefaultAsync();
            if (assignment == null)
            {
                return await Task.FromResult(NotFound());
            }

            var dataImage = avatar.ImageBase64.Split(',');
            var imageBytes = Convert.FromBase64String(dataImage[1]);
            var mimeString = dataImage[0].Split(':')[1].Split(';')[0];
            var extension = mimeString.Split('/')[1];
            var imageName = $"{Guid.NewGuid().ToString("N")}.{extension}";
            var cloudStorageAccount = CloudStorageAccount.Parse(_azureSettings.StorageConnectionString);
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("incidentpics");
            var blockBlob = blobContainer.GetBlockBlobReference(imageName);
            await blockBlob.UploadFromByteArrayAsync(imageBytes, 0, imageBytes.Length);

            UpdateResult updateResult;
            do
            {
                assignment = await _mongoDatabase.GetCollection<Assignment>("assignments").Find(x => x.Id == id).SingleOrDefaultAsync();
                var imageUrls = assignment.ImageUrls ?? new List<string>();
                imageUrls.Add(blockBlob.Uri.ToString());
                var updateDefinitition = new UpdateDefinitionBuilder<Assignment>().Set(x => x.ImageUrls, imageUrls).Set(x => x.UpdateDateTime, DateTimeOffset.Now);
                updateResult = await _mongoDatabase.GetCollection<Assignment>("assignments").UpdateOneAsync(x => x.Id == id 
                && x.UpdateDateTime == assignment.UpdateDateTime, updateDefinitition);
            } while (updateResult.ModifiedCount == 0);

            return await Task.FromResult(Ok(blockBlob.Uri));
        }
    }
}