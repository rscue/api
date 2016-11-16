using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Rscue.Api.Models
{
    public class Fleet
    {
        [BsonId]
        public BsonObjectId Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(255)]
        public string Name { get; set; }

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
    }
}