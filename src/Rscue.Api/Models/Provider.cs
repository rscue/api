using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace Rscue.Api.Models
{
    public class Provider
    {
        [BsonId]
        [Required]
        public string Id { get; set; }

        [Required]
        [BsonElement]
        public string Name { get; set; }

        [Required]
        [BsonElement]
        public string Address { get; set; }

        [Required]
        [BsonElement]
        public string ZipCode { get; set; }

        [Required]
        [BsonElement]
        public string City { get; set; }

        [Required]
        [BsonElement]
        public string State { get; set; }
        
        [Required]
        [EmailAddress]
        [BsonElement]
        public string Email { get; set; }

        [BsonElement]
        public ImageBucketKey ProviderImageBucketKey { get; set; }

        [BsonExtraElements]
        public IDictionary<string, object> ExtraElements { get; set; }
    }
}