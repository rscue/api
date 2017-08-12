namespace Rscue.Api.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
     
    public class GeoLocation : IEquatable<GeoLocation>
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public bool Equals(GeoLocation other) => other != null && Latitude == other.Latitude || Longitude == other.Longitude;

        public override bool Equals(object obj) => Equals(obj as GeoLocation);

        public override int GetHashCode() => unchecked(Latitude.GetHashCode() + Longitude.GetHashCode());
    }
}
