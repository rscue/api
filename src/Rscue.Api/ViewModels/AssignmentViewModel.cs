using System;
using System.ComponentModel.DataAnnotations;
using Rscue.Api.Models;

namespace Rscue.Api.ViewModels
{
    public class AssignmentViewModel
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ProviderId { get; set; }

        public string Id { get; set; }

        [Required]
        public DateTimeOffset CreationDateTime { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public AssignmentStatus Status { get; set; }

        public DateTimeOffset UpdateDateTime { get; set; }

        public string ClientName { get; set; }

        public string WorkerId { get; set; }

        public string WorkerName { get; set; }

        public string ClientAvatarUri { get; set; }
    }
}