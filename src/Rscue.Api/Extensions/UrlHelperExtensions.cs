using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rscue.Api.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string BuildGetAssignmentUrl(this IUrlHelper urlHelper, string id) =>
            urlHelper != null && id != null
                ? urlHelper.RouteUrl(Constants.Routes.GET_ASSIGNMENT, BuildGetAssignmentRouteValues(id))
                : null;

        public static string BuildGetProviderUrl(this IUrlHelper urlHelper, string id) =>
            urlHelper != null && id != null
                ? urlHelper.RouteUrl(Constants.Routes.GET_PROVIDER, BuildGetProviderRouteValues(id))
                : null;

        public static string BuildGetImageUrl(this IUrlHelper urlHelper, string store, string bucket, string name) =>
            urlHelper != null && store != null && bucket != null
                ? urlHelper.RouteUrl(Constants.Routes.GET_IMAGE, BuildGetImageRouteValues(store, bucket, name))
                : null;

        public static string BuildGetImagesUrl(this IUrlHelper urlHelper, string store, string bucket) =>
            urlHelper != null && store != null && bucket != null
                ? urlHelper.RouteUrl(Constants.Routes.GET_IMAGES, new { store = store, bucket = bucket })
                : null;

        public static string BuildGetProviderBoatTowUrl(this IUrlHelper urlHelper, string providerId, string id) =>
            urlHelper != null && providerId != null &&  id != null
                ? urlHelper.RouteUrl(Constants.Routes.GET_PROVIDER_BOATTOW, BuildGetProviderBoatTowRouteValues(providerId, id))
                : null;

        public static string BuildGetProviderBoatTowsUrl(this IUrlHelper urlHelper, string providerId) =>
            urlHelper != null && providerId != null
                ? urlHelper.RouteUrl(Constants.Routes.GET_PROVIDER_BOATTOWS, BuildGetProviderBoatTowsRouteValues(providerId))
                : null;

        public static string BuildGetProviderProfilePictureUrl(this IUrlHelper urlHelper, string store, string bucket) =>
            BuildGetImageUrl(urlHelper, store, bucket, "profilepicture");

        private static object BuildGetImageRouteValues(string store, string bucket, string name) =>
            store != null && bucket != null && name != null
                ? new { store, bucket, name }
                : null;

        private static object BuildGetAssignmentRouteValues(string id) => id != null ? new { id } : null;

        private static object BuildGetProviderRouteValues(string id) => id != null ? new { id } : null;

        private static object BuildGetProviderBoatTowRouteValues(string providerId, string id) =>
            providerId != null && id != null
                ? new { providerId, id }
                : null;

        private static object BuildGetProviderBoatTowsRouteValues(string providerId) => new { providerId };

    }
}
