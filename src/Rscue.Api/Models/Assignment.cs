using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Rscue.Api.Models
{
    public class Assignment
    {
        [BsonId]
        public BsonObjectId Id { get; set; }

        [BsonElement]
        public MongoDBRef Client { get; set; }

        [BsonElement]
        public MongoDBRef Provider { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.Document)]
        public DateTimeOffset CreationDateTime { get; set; }

        [BsonElement]
        public GeoJson2DGeographicCoordinates Location { get; set; }

        [BsonElement]
        public AssignmentStatus Status { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.Document)]
        public DateTimeOffset UpdateDateTime { get; set; }
    }
}