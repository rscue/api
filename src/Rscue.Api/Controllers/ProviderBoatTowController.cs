namespace Rscue.Api.Controllers
{
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Rscue.Api.BindingModels;
    using Rscue.Api.Models;
    using Rscue.Api.Plumbing;
    using Rscue.Api.Services;
    using Rscue.Api.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Authorize]
    [Route("provider/{providerId}/boattow")]
    public class ProviderBoatTowController : Controller
    {
        private readonly IProviderBoatTowRepository _providerBoatTowRepository;

        public ProviderBoatTowController(IProviderBoatTowRepository providerBoatTowRepository)
        {
            _providerBoatTowRepository = providerBoatTowRepository ?? throw new ArgumentNullException(nameof(providerBoatTowRepository));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProviderBoatTowViewModel), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> NewProviderBoatTow(string providerId, [FromBody] ProviderBoatTowBindingModel providerBoatTowBindingModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var providerBoatTow = new ProviderBoatTow
            {
                BoatModel = providerBoatTowBindingModel.BoatModel,
                EngineType = providerBoatTowBindingModel.EngineType,
                Name = providerBoatTowBindingModel.Name,
                RegistrationNumber = providerBoatTowBindingModel.RegistrationNumber,
                FuelCostPerKm = providerBoatTowBindingModel.FuelCostPerKm
            };

            var (providerBoatTowResult, outcomeAction, error) = await _providerBoatTowRepository.NewAsync(providerId, providerBoatTow);

            return this.FromRepositoryOutcome(outcomeAction, error, MapToProviderBoatTowViewModel(providerBoatTowResult), Url.BuildGetProviderBoatTowUrl(providerId, providerBoatTowResult.Id));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProviderBoatTowViewModel), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateProviderBoatTow(string providerId, string id, [FromBody] ProviderBoatTowBindingModel boatTow)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var providerBoatTow = new ProviderBoatTow
            {
                Id = id,
                BoatModel = boatTow.BoatModel,
                EngineType = boatTow.EngineType,
                Name = boatTow.Name,
                RegistrationNumber = boatTow.RegistrationNumber,
                FuelCostPerKm = boatTow.FuelCostPerKm
            };

            var (providerBoatTowResult, outcomeAction, error) = await _providerBoatTowRepository.UpdateAsync(providerId, providerBoatTow);

            return this.FromRepositoryOutcome(outcomeAction, error, MapToProviderBoatTowViewModel(providerBoatTowResult));
        }

        [HttpGet("{id}", Name = Constants.Routes.GET_PROVIDER_BOATTOW)]
        [ProducesResponseType(typeof(ProviderBoatTowViewModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetProviderBoatTow(string providerId, string id)
        {
            var (providerBoatTowResult, outcomeAction, error) = 
                await _providerBoatTowRepository.GetByIdAsync(providerId, id);

            return this.FromRepositoryOutcome(outcomeAction, error, providerBoatTowResult);
        }

        [HttpGet(Name = Constants.Routes.GET_PROVIDER_BOATTOWS)]
        [ProducesResponseType(typeof(IEnumerable<ProviderBoatTowViewModel>), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetProviderBoatTows(string providerId)
        {
            var (providerBoatTowResults, outcomeAction, error) =
                await _providerBoatTowRepository.GetAllAsync(providerId);

            var results = providerBoatTowResults != null 
                ? providerBoatTowResults.Select(MapToProviderBoatTowViewModel) 
                : null;

            return this.FromRepositoryOutcome(outcomeAction, error, results);
        }

        private static ProviderBoatTowViewModel MapToProviderBoatTowViewModel(ProviderBoatTow providerBoatTow) =>
            providerBoatTow != null
                ? new ProviderBoatTowViewModel
                {
                    Id = providerBoatTow.Id,
                    RegistrationNumber = providerBoatTow.RegistrationNumber,
                    Name = providerBoatTow.Name,
                    BoatModel = providerBoatTow.BoatModel,
                    EngineType = providerBoatTow.EngineType,
                    FuelCostPerKm = providerBoatTow.FuelCostPerKm
                }
                : null;
    }
}