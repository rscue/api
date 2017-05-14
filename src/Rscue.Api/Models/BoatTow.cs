using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Rscue.Api.Models
{
    public class BoatTow
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public BsonObjectId Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [BsonElement]
        public string BoatModel { get; set; }

        [Required]
        [BsonElement]
        public string EngineType { get; set; }

        [BsonElement]
        public string RegistrationNumber { get; set; }

        [BsonElement]
        public string ProviderId { get; set; }

        [BsonElement]
        public double? FuelCostPerKm { get; set; }
    }
}