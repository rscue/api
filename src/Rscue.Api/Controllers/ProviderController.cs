using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Rscue.Api.Models;

namespace Rscue.Api.Controllers
{
    [Route("provider")]
    public class ProviderController : Controller
    {
        private readonly IMongoDatabase _mongoDatabase;
        public ProviderController(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        [HttpPost]
        public async Task<IActionResult> AddProvider([FromBody] Provider provider)
        {
            if (ModelState.IsValid)
            {
                    await _mongoDatabase.GetCollection<Provider>("providers").InsertOneAsync(provider);

                    var uri = new Uri($"{Request.GetEncodedUrl()}/{provider.Id}");
                    return await Task.FromResult(Created(uri, provider));
            }

            return await Task.FromResult(BadRequest());
        }
    }
}