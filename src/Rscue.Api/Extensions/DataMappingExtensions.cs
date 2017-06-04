namespace Rscue.Api.Extensions
{
    using MongoDB.Driver.GeoJsonObjectModel;
    using Rscue.Api.Models;

    public static class DataMappingExtensions
    {
        public static GeoJson2DGeographicCoordinates ToGeoJson2DGeographicCoordinates(this GeoLocation geoLocation) =>
            geoLocation != null
                ? new GeoJson2DGeographicCoordinates(geoLocation.Longitude, geoLocation.Latitude)
                : null;

        public static GeoLocation ToGeoLocation(this GeoJson2DGeographicCoordinates geoJson2DGeographicCoordinates) =>
            geoJson2DGeographicCoordinates != null
                ? new GeoLocation { Latitude = geoJson2DGeographicCoordinates.Latitude, Longitude = geoJson2DGeographicCoordinates.Longitude }
                : null;
    }
}
