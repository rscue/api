﻿﻿namespace Rscue.Api.Controllers
{
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.WindowsAzure.Storage;
    using MongoDB.Driver;
    using Rscue.Api.BindingModels;
    using Rscue.Api.Models;
    using Rscue.Api.Plumbing;
    using Rscue.Api.Services;
    using Rscue.Api.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Authorize]
    [Route("provider")]
    public class ProviderController : Controller
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IImageBucketRepository _imageBucketRepository;

        public ProviderController(IProviderRepository providerRepository, IImageBucketRepository imageBucketRepository)
        {
            _providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
            _imageBucketRepository = imageBucketRepository ?? throw new ArgumentNullException(nameof(imageBucketRepository));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProviderViewModel), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> NewProvider([FromBody] ProviderBindingModel providerBindingModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var (imageBucket, outcomeAction, error) =
                await _imageBucketRepository.NewImageBucket(
                    new ImageBucket
                    {
                        StoreBucket = new ImageBucketKey { Store = Constants.PROVIDER_IMAGES_STORE },
                        ImageList = new List<string>()
                    });

            if (outcomeAction != RepositoryOutcomeAction.OkCreated)
            {
                return this.StatusCode(500, error);
            }

            var provider = new Provider
            {
                Email = providerBindingModel.Email,
                Name = providerBindingModel.Name,
                State = providerBindingModel.State,
                City = providerBindingModel.City,
                ZipCode = providerBindingModel.ZipCode,
                Address = providerBindingModel.Address,
                ProviderImageBucketKey = imageBucket.StoreBucket,
            };

            (provider, outcomeAction, error) = await _providerRepository.NewAsync(provider);

            return this.FromRepositoryOutcome(
                outcomeAction, error,
                MapToProviderViewModel(provider),
                Url.BuildGetProviderUrl(provider?.Id));
        }

        [Route("{id}")]
        [HttpPut]
        [ProducesResponseType(typeof(ProviderViewModel), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateProvider(string id, [FromBody] ProviderBindingModel providerBindingModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var providerPatch = new Provider
            {
                Id = id,
                Email = providerBindingModel.Email,
                Name = providerBindingModel.Name,
                State = providerBindingModel.State,
                City = providerBindingModel.City,
                ZipCode = providerBindingModel.ZipCode,
                Address = providerBindingModel.Address,
            };

            var (provider, outcomeAction, error) = 
                await _providerRepository.PatchAllButProviderImageStoreAsync(providerPatch);

            return this.FromRepositoryOutcome(outcomeAction, error, MapToProviderViewModel(provider));
        }

        [Route("{id}", Name = Constants.Routes.GET_PROVIDER)]
        [HttpGet]
        [ProducesResponseType(typeof(ProviderViewModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetProvider(string id)
        {
            var (provider, outcomeAction, error) = 
                await _providerRepository.GetByIdAsync(id);

            return this.FromRepositoryOutcome(outcomeAction, error, MapToProviderViewModel(provider));
        }

        private ProviderViewModel MapToProviderViewModel(Provider provider) =>
            provider != null
                ? new ProviderViewModel
                {
                    Id = provider.Id,
                    Email = provider.Email,
                    Name = provider.Name,
                    State = provider.State,
                    City = provider.City,
                    ZipCode = provider.ZipCode,
                    Address = provider.Address,
                    ProfilePictureUrl = Url.BuildGetImageUrl(provider.ProviderImageBucketKey?.Store, provider.ProviderImageBucketKey.Bucket, "profilepicture"),
                    ProviderBoatTowsUrl = Url.BuildGetProviderBoatTowsUrl(provider.Id)
                }
                : null;
    }
}