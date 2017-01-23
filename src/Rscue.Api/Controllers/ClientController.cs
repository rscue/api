using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using MongoDB.Driver;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Route("client")]
    public class ClientController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IRequestCultureProvider _requestCultureProvider;
        private readonly AzureSettings _appSettings;

        public ClientController(IMongoDatabase mongoDatabase, IOptions<AzureSettings> appSettings)
        {
            _mongoDatabase = mongoDatabase;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ClientViewModel), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> AddClient([FromBody] ClientViewModel client)
        {
            if (ModelState.IsValid)
            {
                var exists = await _mongoDatabase.GetCollection<Client>("clients").Find(x => x.Id == client.Id).SingleOrDefaultAsync();
                if (exists != null)
                {
                    return await Task.FromResult(BadRequest($"El cliente con Id {client.Id} ya existe"));
                }

                var model = new Client
                {
                    Id = client.Id ?? Guid.NewGuid().ToString(),
                    Email = client.Email,
                    Name = client.Name,
                    LastName = client.LastName,
                    AvatarUri = client.AvatarUri,
                    BoatModel = client.BoatModel,
                    DeviceId = client.DeviceId,
                    EngineType = client.EngineType,
                    HullSize = client.HullSize,
                    PhoneNumber = client.PhoneNumber,
                    RegistrationNumber = client.RegistrationNumber,
                    VehicleType = client.VehicleType,
                    InsuranceCompany = client.InsuranceCompany,
                    PolicyNumber = client.PolicyNumber
                };
                await _mongoDatabase.GetCollection<Client>("clients").InsertOneAsync(model);

                var uri = new Uri($"{Request.GetEncodedUrl()}/{model.Id}");
                client.Id = model.Id;
                return await Task.FromResult(Created(uri, client));                
            }

            return await Task.FromResult(BadRequest(ModelState.GetErrors()));
        }

        [HttpPut]
        [ProducesResponseType(typeof(ClientViewModel), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateClient([FromBody] ClientViewModel client)
        {
            if (ModelState.IsValid)
            {
                var exists = await _mongoDatabase.GetCollection<Client>("clients").Find(x => x.Id == client.Id).SingleOrDefaultAsync();
                if (exists == null)
                {
                    return await Task.FromResult(BadRequest($"El cliente con Id {client.Id} no existe"));
                }

                var model = new Client
                {
                    Id = client.Id,
                    Email = client.Email,
                    Name = client.Name,
                    LastName = client.LastName,
                    AvatarUri = client.AvatarUri,
                    BoatModel = client.BoatModel,
                    DeviceId = client.DeviceId,
                    EngineType = client.EngineType,
                    HullSize = client.HullSize,
                    PhoneNumber = client.PhoneNumber,
                    RegistrationNumber = client.RegistrationNumber,
                    VehicleType = client.VehicleType,
                    PolicyNumber = client.PolicyNumber,
                    InsuranceCompany = client.InsuranceCompany
                };
                await _mongoDatabase.GetCollection<Client>("clients").ReplaceOneAsync(x => x.Id == client.Id, model);
                return await Task.FromResult(Ok(client));
            }

            return await Task.FromResult(BadRequest(ModelState.GetErrors()));
        }

        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(ClientViewModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetClient(string id)
        {
            var client = await _mongoDatabase.GetCollection<Client>("clients").Find(x => x.Id == id).SingleOrDefaultAsync();
            if (client == null)
            {
                return await Task.FromResult(NotFound($"El cliente con Id {id} no existe"));
            }

            var model = new ClientViewModel
            {
                Id = client.Id,
                Email = client.Email,
                Name = client.Name,
                LastName = client.LastName,
                VehicleType = client.VehicleType,
                HullSize = client.HullSize,
                EngineType = client.EngineType,
                BoatModel = client.BoatModel,
                RegistrationNumber = client.RegistrationNumber,
                PhoneNumber = client.PhoneNumber,
                AvatarUri = client.AvatarUri,
                DeviceId = client.DeviceId,
                PolicyNumber = client.PolicyNumber,
                InsuranceCompany = client.InsuranceCompany
            };
            return await Task.FromResult(Ok(model));
        }

        [Route("{id}/profilepic")]
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateProfilePicture(string id, [FromBody] AvatarViewModel avatar)
        {
            var client = await _mongoDatabase.GetCollection<Client>("clients").Find(x => x.Id == id).SingleOrDefaultAsync();
            if (client == null)
            {
                return await Task.FromResult(NotFound($"El cliente con Id {id} no existe"));
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

            return await Task.FromResult(Ok(blockBlob.Uri));
        }

        [Route("{id}/registerdevice")]
        [HttpPut]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> AddUpdateDeviceId(string id, [FromBody] DeviceRegistrationViewModel device)
        {
            var client = await _mongoDatabase.GetCollection<Client>("clients").Find(x => x.Id == id).SingleOrDefaultAsync();
            if (client == null)
            {
                return await Task.FromResult(NotFound($"El cliente con Id {id} no existe"));
            }

            var updateDefinitition = new UpdateDefinitionBuilder<Client>().Set(x => x.DeviceId, device.DeviceId);
            await _mongoDatabase.GetCollection<Client>("clients").UpdateOneAsync(x => x.Id == id, updateDefinitition);

            return await Task.FromResult(Ok());
        }
    }
}