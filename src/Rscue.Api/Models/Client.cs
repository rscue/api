using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using Rscue.Api.Models;

namespace Rscue.Api.Models
{
    public class Client
    {
        [BsonId]
        [Required]
        public string Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        [BsonElement]
        public string Name { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        [BsonElement]
        public string LastName { get; set; }

        [EmailAddress]
        [MinLength(3)]
        [MaxLength(255)]
        [BsonElement]
        public string Email { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        [BsonElement]
        public string PhoneNumber { get; set; }

        [Required]
        [BsonElement]
        public VehicleType VehicleType { get; set; }

        [Required]
        [BsonElement]
        public HullSizeType HullSize { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        [BsonElement]
        public string BoatModel { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        [BsonElement]
        public string EngineType { get; set; }

        [MinLength(3)]
        [MaxLength(255)]
        [BsonElement]
        public string RegistrationNumber { get; set; }

        [BsonElement]
        public Uri AvatarUri { get; set; }

        [BsonElement]
        public string DeviceId { get; set; }
    }
}