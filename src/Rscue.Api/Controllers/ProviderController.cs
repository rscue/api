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
    }
}