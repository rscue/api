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
            _assignmentRepository = assignmentRepository;
            _notificationServices = notificationServices;
            _imageStore = imageStore;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AssignmentViewModel), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> AddAssignment([FromBody] AssignmentViewModel assignmentViewModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.GetErrors());

            var location = (string)null;
            var assignment = new Assignment
            {
                ProviderId = assignmentViewModel.ProviderId,
                ClientId = assignmentViewModel.ClientId,
                CreationDateTime = assignmentViewModel.CreationDateTime,
                Location = new GeoJson2DGeographicCoordinates(assignmentViewModel.Longitude, assignmentViewModel.Latitude),
                Status = assignmentViewModel.Status,
                EstimatedTimeOfArrival = assignmentViewModel.EstimatedTimeOfArrival,
                WorkerId = assignmentViewModel.WorkerId.IfNotNullOrEmpty()
            };

            var (newAssignment, outcome, message) = await _assignmentRepository.NewAssignmentAsync(assignment);

            if (outcome == RepositoryOutcome.Created)
            {
                location = $"{Request.GetEncodedUrl()}/{newAssignment.Id.ToString()}";
                await _notificationServices.NotifyAssignmentWorkerAsync(newAssignment);
            }

            return this.FromRepositoryOutcome(outcome, message, newAssignment, location);
        }

        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(IEnumerable<ErrorViewModel>), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateAssignment(string id, [FromBody] AssignmentViewModel assignmentViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.GetErrors());
            }

            var (assignment, outcome, message) = await _assignmentRepository.GetAssignmentByIdAsync(assignmentViewModel.Id);
            if (outcome != RepositoryOutcome.RetrieveSuccess)
            {
                return this.FromRepositoryOutcome(outcome, message);
            }

            (_, outcome, message) = await _assignmentRepository.SaveAssignmentAsync(assignment);
            var result = this.FromRepositoryOutcome(outcome, message);
            if (outcome == RepositoryOutcome.Updated)
            {
                await _notificationServices.NotifyAssignmentWorkerAsync(assignment);
            }

            return result;
        }

        [HttpGet]
        [Route("search")]
        [ProducesResponseType(typeof(IEnumerable<AssignmentSearchResponseViewModel>), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> SearchAssignments([FromQuery] AssignmentSearchViewModel search)
        {
            var (assignments, outcome, message) = 
                await _assignmentRepository
                    .SearchAssignmentAsync(search.StartDateTime, search.EndDateTime, search.Statuses,
                                           populateClient: true,
                                           populateWorker: true);

            var assignmentResponses =
                outcome == RepositoryOutcome.RetrieveSuccess
                    ? assignments.Select(
                        _ => new AssignmentResponseViewModel
                        {
                            Id =_.Id,
                            WorkerName = _.Worker.Name + " " + _.Worker.LastName,
                            ClientName = _.Client.Name + " " + _.Client.LastName,
                            CreationDateTime = _.CreationDateTime,
                            Status = _.Status,
                            EstimatedTimeOfArrival = _.EstimatedTimeOfArrival
                        }).ToList()
                    : null;

            return this.FromRepositoryOutcome(outcome, message, assignmentResponses);
        }

        [HttpGet]
        [Route("{id}")]
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
                outcome == RepositoryOutcome.RetrieveSuccess
                    ? new AssignmentResponseViewModel
                    {
                        Id = assignment.Id,
                        Status = assignment.Status,
                        CreationDateTime = assignment.CreationDateTime,
                        ClientName = assignment.Client.Name + " " + assignment.Client.LastName,
                        WorkerName = assignment.Worker.Name + " " + assignment.Worker.LastName,
                        Latitude = assignment.Location.Latitude,
                        Longitude = assignment.Location.Longitude,
                        ClientAvatarUri = assignment.Client.AvatarUri == null ? "assets/img/nobody.jpg" : assignment.Client.AvatarUri.ToString(),
                        ClientId = assignment.ClientId,
                        ProviderId = assignment.ProviderId,
                        Comments = assignment.Comments,
                        ImageUrls = assignment.ImageUrls,
                        WorkerId = assignment.WorkerId,
                        UpdateDateTime = assignment.UpdateDateTime,
                        EstimatedTimeOfArrival = assignment.EstimatedTimeOfArrival
                    }
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
            if (outcome != RepositoryOutcome.RetrieveSuccess)
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
    }
}