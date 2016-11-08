using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using Rscue.Api.Models;

namespace Rscue.Api.ViewModels
{
    public class ClientViewModel
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
        [EmailAddress]
        [MinLength(3)]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        public string PhoneNumber { get; set; }

        [Required]
        public VehicleType VehicleType { get; set; }

        [Required]
        public HullSizeType HullSize { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        public string BoatModel { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        public string EngineType { get; set; }

        [MinLength(3)]
        [MaxLength(255)]
        public string RegistrationNumber { get; set; }
    }
}