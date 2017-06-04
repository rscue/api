using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rscue.Api.Models
{
    public class ImageBucketKey
    {
        [BsonElement]
        public string Store { get; set; }

        [BsonElement]
        public string Bucket { get; set; }

        
    }
}
