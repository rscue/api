﻿using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using Rscue.Api.Models;

namespace Rscue.Api.Models
{
    public class Client
    {
        [BsonId]
        [Required]
        public string Id { get; set; }

        [Required]
        [BsonElement]
        public string Name { get; set; }

        [Required]
        [BsonElement]
        public string LastName { get; set; }

        [EmailAddress]
        [BsonElement]
        public string Email { get; set; }

        [Required]
        [BsonElement]
        public string PhoneNumber { get; set; }

        [Required]
        [BsonElement]
        public VehicleType VehicleType { get; set; }

        [Required]
        [BsonElement]
        public HullSizeType HullSize { get; set; }

        [Required]
        [BsonElement]
        public string BoatModel { get; set; }

        [Required]
        [BsonElement]
        public string EngineType { get; set; }

        [BsonElement]
        public string RegistrationNumber { get; set; }

        [BsonElement]
        public Uri AvatarUri { get; set; }

        [BsonElement]
        public string DeviceId { get; set; }

        [BsonElement]
        public string InsuranceCompany { get; set; }

        [BsonElement]
        public string PolicyNumber { get; set; }
    }
}