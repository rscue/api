namespace Rscue.Api.Models
{
    using System.ComponentModel.DataAnnotations;

    public class GeoLocation
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
}
