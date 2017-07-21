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
        [BsonElement]
        public string Id { get; set; }

        [BsonElement]
        public string ClientId { get; set; }

        [BsonElement]
        public string ProviderId { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.Document)]
        public DateTimeOffset CreationDateTime { get; set; }

        [BsonElement]
        public GeoJson2DGeographicCoordinates InitialLocation { get; set; }

        [BsonElement]
        public GeoJson2DGeographicCoordinates ServiceLocation { get; set; }

        [BsonElement]
        public AssignmentStatus Status { get; set; }

        [BsonElement]
        public AssignmentStatusReason StatusReason { get; set; }

        [BsonElement]
        [BsonRepresentation(BsonType.Document)]
        public DateTimeOffset? UpdateDateTime { get; set; }

        [BsonElement]
        public string WorkerId { get; set; }

        [BsonElement]
        public string BoatTowId { get; set; }

        [BsonElement]
        public string Comments { get; set; }

        [BsonElement]
        public ImageBucketKey ImageBucketKey { get; set; }

        [BsonElement]
        public TimeSpan? EstimatedTimeOfArrival { get; set; }

        [BsonIgnore]
        public ProviderWorker Worker { get; set; }

        [BsonIgnore]
        public Client Client { get; set; }

        [BsonIgnore]
        public Provider Provider { get; set; }

        [BsonIgnore]
        public ProviderBoatTow BoatTow { get; set; }

        [BsonExtraElements]
        public IDictionary<string, object> ExtraElements { get; set; }
    }
}