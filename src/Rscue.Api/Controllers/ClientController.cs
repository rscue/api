using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Rscue.Api.Models;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Controllers
{
    [Route("client")]
    public class ClientController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;

        public ClientController(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        [HttpPost]
        public async Task<IActionResult> AddClient([FromBody] ClientViewModel client)
        {
            if (ModelState.IsValid)
            {
                var model = new Client
                {
                    VehicleType = client.VehicleType,
                    BoatModel = client.BoatModel,
                    Email = client.Email,
                    EngineType = client.EngineType,
                    HullSize = client.HullSize,
                    LastName = client.LastName,
                    Name = client.Name,
                    PhoneNumber = client.PhoneNumber,
                    RegistrationNumber = client.RegistrationNumber
                };

                await _mongoDatabase.GetCollection<Client>("clients").InsertOneAsync(model);

                var uri = new Uri($"{Request.GetEncodedUrl()}/{model.Id}");
                client.Id = model.Id.ToString();

                return await Task.FromResult(Created(uri, client));
            }

            return await Task.FromResult(BadRequest());
        }
    }
}