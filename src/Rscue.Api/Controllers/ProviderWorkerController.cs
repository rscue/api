namespace Rscue.Api.Controllers
{
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Driver.Linq;
    using Rscue.Api.Models;
    using Rscue.Api.Plumbing;
    using Rscue.Api.ViewModels;
    using Rscue.Api.Services;
    using Rscue.Api.BindingModels;
    using Microsoft.AspNetCore.JsonPatch;
    using Auth0.Core.Exceptions;
    using Microsoft.AspNetCore.JsonPatch.Operations;

    [Authorize]
    [Route("provider/{providerId}/worker")]
    public class ProviderWorkerController : Controller
    {
        private readonly IImageBucketRepository _imageBucketRepository;
        private readonly IProviderWorkerRepository _providerWorkerRepository;
        private readonly IUserService _userService;

        public ProviderWorkerController(IImageBucketRepository imageBucketRepository, IProviderWorkerRepository providerWorkerRepository, IUserService userService)
        {
            _imageBucketRepository = imageBucketRepository ?? throw new ArgumentNullException(nameof(imageBucketRepository));
            _providerWorkerRepository = providerWorkerRepository ?? throw new ArgumentNullException(nameof(providerWorkerRepository)); 
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProviderWorkerViewModel), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> NewWorker(string providerId, [FromBody] ProviderWorkerBindingModel bindingModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var (imageBucket, outcomeAction, error) =
                await _imageBucketRepository.NewImageBucket(
                    new ImageBucket
                    {
                        StoreBucket = new ImageBucketKey { Store = Constants.WORKER_IMAGES_STORE },
                        ImageList = new List<string>()
                    });

            if (outcomeAction != RepositoryOutcomeAction.OkCreated)
            {
                return this.StatusCode(500, error);
            }

            var providerWorker = new ProviderWorker
            {
                Name = bindingModel.Name,
                LastName = bindingModel.LastName,
                Email = bindingModel.Email,
                PhoneNumber = bindingModel.PhoneNumber,
                ProviderWorkerImageBucketKey = imageBucket.StoreBucket
            };

            (providerWorker, outcomeAction, error) = await _providerWorkerRepository.NewAsync(providerId, providerWorker);

            try
            {
                var userRegistration = await _userService.RegisterUserAsync(
                    new UserRegistration
                    {
                        ProviderId = providerId,
                        WorkerId = providerWorker.Id,
                        FirstName = bindingModel.Name,
                        LastName = bindingModel.LastName,
                        Email = bindingModel.Email,
                        Password = bindingModel.Password,
                    });
            }
            catch (ApiException ex)
            {
                return BadRequest(ex.ApiError);
            }

            var location = this.Url.BuildGetProviderWorkerUrl(providerId, providerWorker?.Id);

            return this.FromRepositoryOutcome(outcomeAction, error, MapToProviderWorkerViewModel(providerWorker), location);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateWorker(string providerId, string id, [FromBody] ProviderWorkerBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var (result, outcomeAction, error) = 
                await _providerWorkerRepository.PatchAllButProviderWorkerImageStoreAsync(
                    providerId, 
                    new ProviderWorker
                    {
                        Id = id,
                        Email = model.Email,
                        Name = model.Name,
                        LastName = model.LastName,
                        Status = model.Status,
                        PhoneNumber = model.PhoneNumber,
                        DeviceId = model.DeviceId,
                        LastKnownLocation = model.LastKnownLocation
                    });

            return this.FromRepositoryOutcome(outcomeAction, error, MapToProviderWorkerViewModel(result));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchWorker(string providerId, string id, [FromBody]JsonPatchDocument<ProviderWorkerBindingModel> patchBindingModel)
        {
            if (patchBindingModel.Operations.Count == 0) return BadRequest("Must indicate operations to perform");

            var patchModel = new JsonPatchDocument<ProviderWorker>();
            for (int i = 0; i < patchBindingModel.Operations.Count; i++)
            {
                var operation = patchBindingModel.Operations[i];
                patchModel.Operations.Add(
                    new Operation<ProviderWorker>(operation.op, operation.path, operation.from, operation.value.ToModelType()));
            }

            var (result, outcomeAction, error) = await _providerWorkerRepository.PatchAsync(providerId, id, patchModel);

            return this.FromRepositoryOutcome(outcomeAction, error, MapToProviderWorkerViewModel(result));            
        }

        [HttpGet("{id}", Name = Constants.Routes.GET_PROVIDER_WORKER)]
        [ProducesResponseType(typeof(ProviderWorkerViewModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetWorker(string providerId, string id)
        {
            var (result, outcomeAction, error) = await _providerWorkerRepository.GetByIdAsync(providerId, id);
            
            return this.FromRepositoryOutcome(outcomeAction, error, MapToProviderWorkerViewModel(result));
        }

        [HttpGet(Name = Constants.Routes.GET_PROVIDER_WORKERS)]
        [ProducesResponseType(typeof(IEnumerable<ProviderWorkerViewModel>), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetWorkers(string providerId, [FromQuery] IEnumerable<ProviderWorkerStatus> status)
        {
            var (result, outcomeAction, error) = await _providerWorkerRepository.GetAllAsync(providerId);

            var resultVM = result
                .Where(_ => status.Contains(_.Status))
                .Select(MapToProviderWorkerViewModel);

            return this.FromRepositoryOutcome(outcomeAction, error, resultVM);
        }

        private ProviderWorkerViewModel MapToProviderWorkerViewModel(ProviderWorker providerWorker) =>
            providerWorker != null
                ? new ProviderWorkerViewModel
                {
                    Id = providerWorker.Id,
                    ProviderId = providerWorker.ProviderId,
                    Name = providerWorker.Name,
                    LastName = providerWorker.LastName,
                    Email = providerWorker.Email,
                    LastKnownLocation = providerWorker.LastKnownLocation,
                    PhoneNumber = providerWorker.PhoneNumber,
                    Status = providerWorker.Status,
                    ProfilePictureUrl = Url.BuildGetImageUrl(providerWorker?.ProviderWorkerImageBucketKey?.Store, providerWorker?.ProviderWorkerImageBucketKey?.Bucket, "profilepicture"),
                    DeviceId = providerWorker.DeviceId
                }
                : null;
    }
}