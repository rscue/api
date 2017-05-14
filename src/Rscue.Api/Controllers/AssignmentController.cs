using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.Linq;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.Services;
using Rscue.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rscue.Api.Controllers
{
    [Authorize]
    [Route("assignment")]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly INotificationServices _notificationServices;
        private readonly IImageStore _imageStore;

        public AssignmentController(IAssignmentRepository assignmentRepository, INotificationServices notificationServices, IImageStore imageStore)
        {
            _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
            _notificationServices = notificationServices ?? throw new ArgumentNullException(nameof(notificationServices));
            _imageStore = imageStore ?? throw new ArgumentNullException(nameof(imageStore));
        }

        [HttpPost]
        [ProducesResponseType(typeof(AssignmentViewModel), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> NewAssignment([FromBody] AssignmentViewModel assignmentViewModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var assignment = new Assignment
            {
                // Id ignored on pupose
                ProviderId = assignmentViewModel.ProviderId,
                ClientId = assignmentViewModel.ClientId,
                CreationDateTime = assignmentViewModel.CreationDateTime,
                InitialLocation = new GeoJson2DGeographicCoordinates(assignmentViewModel.InitialLocationLongitude, assignmentViewModel.InitialLocationLatitude),
                // ServiceLocation ignored on purpose.
                Status = assignmentViewModel.Status,
                StatusReason = assignmentViewModel.StatusReason,
                EstimatedTimeOfArrival = assignmentViewModel.EstimatedTimeOfArrival,
                WorkerId = assignmentViewModel.WorkerId.IfNotNullOrEmpty(),
                BoatTowId = assignmentViewModel.BoatTowId.IfNotNullOrEmpty()
            };

            var (newAssignment, outcome, message) = await _assignmentRepository.NewAssignmentAsync(assignment);

            if (outcome == RepositoryOutcome.Created)
            {
                await _notificationServices.NotifyAssignmentWorkerAsync(newAssignment);
            }

            return this.FromRepositoryOutcome(outcome, message, newAssignment, nameof(GetAssignment), new { id = newAssignment?.Id });
        }

        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateAssignment(string id, [FromBody] AssignmentViewModel assignmentViewModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var (assignment, outcome, message) = await _assignmentRepository.GetAssignmentByIdAsync(id);
            if (outcome != RepositoryOutcome.Ok)
            {
                return this.FromRepositoryOutcome(outcome, message);
            }

            assignment.ClientId = assignmentViewModel.ClientId;
            assignment.WorkerId = assignmentViewModel.WorkerId;
            assignment.ProviderId = assignmentViewModel.ProviderId;
            assignment.Status = assignmentViewModel.Status;
            assignment.StatusReason = assignmentViewModel.StatusReason;
            assignment.InitialLocation = new GeoJson2DGeographicCoordinates(assignmentViewModel.InitialLocationLongitude, assignmentViewModel.InitialLocationLatitude);
            assignment.ServiceLocation = 
                assignmentViewModel.ServiceLocationLongitude != null && assignmentViewModel.ServiceLocationLatitude != null 
                    ? new GeoJson2DGeographicCoordinates(assignmentViewModel.ServiceLocationLongitude.Value, assignmentViewModel.ServiceLocationLatitude.Value) 
                    : null;
            assignment.EstimatedTimeOfArrival = assignmentViewModel.EstimatedTimeOfArrival;
            assignment.UpdateDateTime = assignmentViewModel.UpdateDateTime;
            assignment.Comments = assignmentViewModel.Comments;

            (assignment, outcome, message) = await _assignmentRepository.UpdateAssignmentAsync(assignment);
            var result = this.FromRepositoryOutcome(outcome, message, assignment);
            if (outcome == RepositoryOutcome.Ok)
            {
                await _notificationServices.NotifyAssignmentWorkerAsync(assignment);
            }

            return result;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AssignmentResponseViewModel>), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> SearchAssignments([FromQuery] AssignmentSearchViewModel search)
        {
            var (assignments, outcome, message) = 
                await _assignmentRepository
                    .SearchAssignmentAsync(search.StartDateTime, search.EndDateTime, search.Statuses,
                                           populateClient: true,
                                           populateWorker: true);

            var assignmentResponses =
                outcome == RepositoryOutcome.Ok
                    ? assignments.Select(MapToAssignmentResponseViewModel).ToList()
                    : null;

            return this.FromRepositoryOutcome(outcome, message, assignmentResponses);
        }

        [HttpGet]
        [Route("{id}", Name = "GetAssignment")]
        [ProducesResponseType(typeof(AssignmentResponseViewModel), 200)]
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
                outcome == RepositoryOutcome.Ok
                    ? MapToAssignmentResponseViewModel(assignment)
                    : null;

            return this.FromRepositoryOutcome(outcome, message, assignment);
        }

        [Route("{id}/incidentpic/{imageName}")]
        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public IActionResult GetIncidentImage(string id, string imageName)
        {
            return new StreamResult(async _ => await _imageStore.DownloadImageAsync(imageName, this.Response.Body));
        }


        [Route("{id}/incidentpic")]
        [HttpPost]
        [ProducesResponseType(typeof(string), 201)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> AddIncidentImage(string id, [FromBody] AvatarViewModel avatar)
        {
            var (_, outcome, message) = await _assignmentRepository.GetAssignmentByIdAsync(id);
            if (outcome != RepositoryOutcome.Ok)
            {
                return this.FromRepositoryOutcome(outcome, message);
            }

            var dataImage = avatar.ImageBase64.Split(',');
            var imageStream = new MemoryStream(Convert.FromBase64String(dataImage[1]));
            var mimeString = dataImage[0].Split(':')[1].Split(';')[0];
            var extension = mimeString.Split('/')[1];
            var imageName = $"{Guid.NewGuid().ToString("N")}.{extension}";
            var location = $"{Request.GetEncodedUrl()}/{imageName}";

            await _imageStore.UploadImageAsync(imageName, imageStream);
            await _assignmentRepository.PatchAssignmentAddImageAsync(id, imageName);
            return this.FromRepositoryOutcome(RepositoryOutcome.Created, null, null, location);
        }

        private static AssignmentResponseViewModel MapToAssignmentResponseViewModel(Assignment assignment) =>
            new AssignmentResponseViewModel
            {
                Id = assignment.Id,
                Status = assignment.Status,
                StatusReason = assignment.StatusReason,
                CreationDateTime = assignment.CreationDateTime,
                ClientName = assignment.Client?.Name + " " + assignment.Client?.LastName,
                WorkerName = assignment.Worker?.Name + " " + assignment.Worker?.LastName,
                InitialLocationLatitude = assignment.InitialLocation?.Latitude ?? 0d,
                InitialLocationLongitude = assignment.InitialLocation?.Longitude ?? 0d,
                ServiceLocationLatitude = assignment.ServiceLocation?.Latitude,
                ServiceLocationLongitude = assignment.ServiceLocation?.Longitude,
                ClientAvatarUri = assignment.Client?.AvatarUri == null ? "assets/img/nobody.jpg" : assignment.Client?.AvatarUri?.ToString(),
                ClientId = assignment.ClientId,
                ProviderId = assignment.ProviderId,
                Comments = assignment.Comments,
                ImageUrls = assignment.ImageUrls,
                WorkerId = assignment.WorkerId,
                BoatTowId = assignment.BoatTowId,
                UpdateDateTime = assignment.UpdateDateTime,
                EstimatedTimeOfArrival = assignment.EstimatedTimeOfArrival
            };
    }
}