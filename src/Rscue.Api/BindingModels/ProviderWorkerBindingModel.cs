namespace Rscue.Api.BindingModels
{
    using Rscue.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    public class ProviderWorkerBindingModel
    {
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

        [MinLength(3)]
        [MaxLength(255)]
        public string Password { get; set; }

        public string DeviceId { get; set; }

        public GeoLocation LastKnownLocation { get; set; }

        public ProviderWorkerStatus Status { get; set; }
    }
}
