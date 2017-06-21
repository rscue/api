﻿using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Rscue.Api.Models;

namespace Rscue.Api.Models
{
    public class ProviderWorker
    {
        [BsonId]
        public string Id { get; set; }

        [Required]
        [BsonElement]
        public string Name { get; set; }

        [Required]
        [BsonElement]
        public string LastName { get; set; }

        [Required]
        [BsonElement]
        public string PhoneNumber { get; set; }

        [Required]
        [BsonElement]
        public string Email { get; set; }

        [BsonElement]
        public string DeviceId { get; set; }

        [BsonElement]
        public string ProviderId { get; set; }

        [BsonElement]
        public ImageBucketKey ProviderWorkerImageBucketKey { get; set; }

        [BsonElement]
        public GeoJson2DGeographicCoordinates LastKnownLocation { get; set; }

        [BsonElement]
        public ProviderWorkerStatus Status { get; set; }
    }
}