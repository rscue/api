using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Rscue.Api.Models
{
    public class Assignment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement]
        public string ClientId { get; set; }

        [BsonElement]
        public string ProviderId { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.Document)]
        public DateTimeOffset CreationDateTime { get; set; }

        [BsonElement]
        public GeoJson2DGeographicCoordinates Location { get; set; }

        [BsonElement]
        public AssignmentStatus Status { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.Document)]
        public DateTimeOffset? UpdateDateTime { get; set; }

        [BsonElement]
        public string WorkerId { get; set; }

        [BsonElement]
        public string Comments { get; set; }

        [BsonElement]
        public IList<string> ImageUrls { get; set; }

        [BsonElement]
        public TimeSpan? EstimatedTimeOfArrival { get; set; }
    }
}