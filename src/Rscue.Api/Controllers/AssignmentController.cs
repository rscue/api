namespace Rscue.Api.Controllers
{
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
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
    [Route("assignment")]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IImageBucketRepository _imageBucketRepository;
        private readonly INotificationServices _notificationServices;

        public AssignmentController(IAssignmentRepository assignmentRepository, IImageBucketRepository imageBucketRepository, INotificationServices notificationServices)
        {
            _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
            _imageBucketRepository = imageBucketRepository ?? throw new ArgumentNullException(nameof(imageBucketRepository));
            _notificationServices = notificationServices ?? throw new ArgumentNullException(nameof(notificationServices));
        }

        [HttpPost]
        [ProducesResponseType(typeof(AssignmentViewModel), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> NewAssignment([FromBody] NewAssignmentBindingModel assignmentBindingModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var (imageBucket, outcomeAction, error) = 
                await _imageBucketRepository.NewImageBucket(
                    new ImageBucket
                    {
                        StoreBucket = new ImageBucketKey { Store = Constants.ASSIGNMENT_IMAGES_STORE },
                        ImageList = new List<string>()
                    });

            if (outcomeAction != RepositoryOutcomeAction.OkCreated)
            {
                return this.StatusCode(500, error);
            }

            var assignment = new Assignment
            {
                ProviderId = assignmentBindingModel.ProviderId,
                ClientId = assignmentBindingModel.ClientId,
                CreationDateTime = assignmentBindingModel.CreationDateTime,
                InitialLocation = assignmentBindingModel.InitialLocation.ToGeoJson2DGeographicCoordinates(),
                Status = assignmentBindingModel.Status,
                StatusReason = assignmentBindingModel.StatusReason,
                EstimatedTimeOfArrival = assignmentBindingModel.EstimatedTimeOfArrival,
                WorkerId = assignmentBindingModel.WorkerId.IfNotNullOrEmpty(),
                BoatTowId = assignmentBindingModel.BoatTowId.IfNotNullOrEmpty(),
                ImageBucketKey = imageBucket.StoreBucket
            };

            (assignment, outcomeAction, error) = await _assignmentRepository.NewAssignmentAsync(assignment);

            if (outcomeAction == RepositoryOutcomeAction.OkCreated)
            {
                await _notificationServices.NotifyAssignmentWorkerAsync(assignment);
            }

            return this.FromRepositoryOutcome(outcomeAction, error, MapToAssignmentResponseViewModel(assignment), Url.BuildGetAssignmentUrl(assignment?.Id));
        }

        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateAssignment(string id, [FromBody] UpdateAssignmentBindingModel assignmentBindingModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var (assignment, outcomeAction, message) = await _assignmentRepository.GetAssignmentByIdAsync(id);
            if (outcomeAction != RepositoryOutcomeAction.OkNone)
            {
                return this.FromRepositoryOutcome(outcomeAction, message);
            }

            assignment.ClientId = assignmentBindingModel.ClientId;
            assignment.WorkerId = assignmentBindingModel.WorkerId;
            assignment.ProviderId = assignmentBindingModel.ProviderId;
            assignment.Status = assignmentBindingModel.Status;
            assignment.StatusReason = assignmentBindingModel.StatusReason;
            assignment.InitialLocation = assignmentBindingModel.InitialLocation.ToGeoJson2DGeographicCoordinates();
            assignment.ServiceLocation = assignmentBindingModel.ServiceLocation.ToGeoJson2DGeographicCoordinates();
            assignment.EstimatedTimeOfArrival = assignmentBindingModel.EstimatedTimeOfArrival;
            assignment.UpdateDateTime = assignmentBindingModel.UpdateDateTime;
            assignment.Comments = assignmentBindingModel.Comments;

            (assignment, outcomeAction, message) = await _assignmentRepository.UpdateAssignmentAsync(assignment);
            var result = this.FromRepositoryOutcome(outcomeAction, message, assignment);
            if (outcomeAction == RepositoryOutcomeAction.OkUpdated)
            {
                await _notificationServices.NotifyAssignmentWorkerAsync(assignment);
            }

            return result;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AssignmentViewModel>), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> SearchAssignments([FromQuery] SearchAssignmentBindingModel search)
        {
            var (assignments, outcome, message) = 
                await _assignmentRepository
                    .SearchAssignmentAsync(search.StartDateTime, search.EndDateTime, search.Statuses,
                                           populateClient: true,
                                           populateWorker: true);

            var assignmentResponses =
                outcome == RepositoryOutcomeAction.OkNone
                    ? assignments.Select(MapToAssignmentResponseViewModel).ToList()
                    : null;

            return this.FromRepositoryOutcome(outcome, message, assignmentResponses);
        }

        [HttpGet]
        [Route("{id}", Name = Constants.Routes.GET_ASSIGNMENT)]
        [ProducesResponseType(typeof(AssignmentViewModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetAssignment(string id)
        {
            var (assignment, outcome, message) = 
                await _assignmentRepository
                    .GetAssignmentByIdAsync(id, 
                                            populateClient: true, 
                                            populateWorker: true);
            var assignmentResult =
                outcome == RepositoryOutcomeAction.OkNone
                    ? MapToAssignmentResponseViewModel(assignment)
                    : null;

            return this.FromRepositoryOutcome(outcome, message, assignment);
        }

        private AssignmentViewModel MapToAssignmentResponseViewModel(Assignment assignment) =>
            assignment != null 
                ? new AssignmentViewModel
                {
                    Id = assignment.Id,
                    Status = assignment.Status,
                    StatusReason = assignment.StatusReason,
                    CreationDateTime = assignment.CreationDateTime,
                    ClientName = assignment.Client?.Name + " " + assignment.Client?.LastName,
                    WorkerName = assignment.Worker?.Name + " " + assignment.Worker?.LastName,
                    InitialLocation = assignment.InitialLocation.ToGeoLocation(),
                    ServiceLocation = assignment.ServiceLocation.ToGeoLocation(),
                    ClientAvatarUri = assignment.Client?.AvatarUri == null ? "assets/img/nobody.jpg" : assignment.Client?.AvatarUri?.ToString(),
                    ClientId = assignment.ClientId,
                    ProviderId = assignment.ProviderId,
                    Comments = assignment.Comments,
                    ImagesUrl = Url.BuildGetImagesUrl(assignment.ImageBucketKey?.Store, assignment.ImageBucketKey?.Bucket),
                    WorkerId = assignment.WorkerId,
                    BoatTowId = assignment.BoatTowId,
                    UpdateDateTime = assignment.UpdateDateTime,
                    EstimatedTimeOfArrival = assignment.EstimatedTimeOfArrival
                }
                : null;
    }
}