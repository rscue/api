using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

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

        [Required]
        [BsonElement]
        public Uri AvatarUri { get; set; }

        [BsonElement]
        public ImageBucketKey ProviderImageBucketKey { get; set; }
    }
}