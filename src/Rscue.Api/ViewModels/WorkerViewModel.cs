using System;
using System.ComponentModel.DataAnnotations;
using Rscue.Api.Controllers;
using Rscue.Api.Models;

namespace Rscue.Api.ViewModels
{
    public class WorkerViewModel
    {
        public string Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        public string LastName { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        public string Email { get; set; }

        public Uri AvatarUri { get; set; }

        public string DeviceId { get; set; }

        [MinLength(3)]
        [MaxLength(255)]
        public string Password { get; set; }

        public LocationViewModel Location { get; set; }

        public ProviderWorkerStatus Status { get; set; }
    }
}