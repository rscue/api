namespace Rscue.Api.Models
{
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ImageBucket
    {
        [BsonId]
        [BsonElement]
        public ImageBucketKey StoreBucket { get; set; }

        [BsonElement]
        public List<string> ImageList { get; set; }
    }
}
