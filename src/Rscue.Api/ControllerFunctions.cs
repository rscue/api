namespace Rscue.Api
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ControllerFunctions
    {
        public static string BuildGetImagesUrl(IUrlHelper urlHelper, string store, string bucket) =>
            urlHelper != null && store != null && bucket != null
                ? urlHelper.RouteUrl("GetImages", new { store = store, bucket = bucket })
                : null;
    }
}
