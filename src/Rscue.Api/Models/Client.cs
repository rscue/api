using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Rscue.Api.Models
{
    public class Client
    {
        [BsonId]
        public BsonObjectId Id { get; set; }

        [BsonElement]
        public string Name { get; set; }

        [BsonElement]
        public string LastName { get; set; }

        [BsonElement]
        public string Email { get; set; }

        [BsonElement]
        public string PhoneNumber { get; set; }

        [BsonElement]
        public VehicleType VehicleType { get; set; }

        [BsonElement]
        public HullSizeType HullSize { get; set; }

        [BsonElement]
        public string BoatModel { get; set; }

        [BsonElement]
        public string EngineType { get; set; }

        [BsonElement]
        public string RegistrationNumber { get; set; }
    }
}