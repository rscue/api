using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Rscue.Api.Models;

namespace Rscue.Api.Models
{
    public class Worker
    {
        [BsonId]
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

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        [BsonElement]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        [BsonElement]
        public string Email { get; set; }

        [BsonElement]
        public Uri AvatarUri { get; set; }

        [BsonElement]
        public string DeviceId { get; set; }

        [BsonElement]
        public MongoDBRef Provider { get; set; }

        [BsonElement]
        public GeoJson2DGeographicCoordinates Location { get; set; }

        [BsonElement]
        public WorkerStatus Status { get; set; }
    }
}