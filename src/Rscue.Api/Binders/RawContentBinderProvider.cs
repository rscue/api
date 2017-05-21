namespace Rscue.Api.ModelBinders
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Rscue.Api.Models;
    using System;

    public class RawContentBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(RawContent))
            {
                // Remember to use BinderTypeModelBinder if we need to use DI services from RawContentBinder
                return new RawContentBinder();
            }

            return null;
        }
    }
}
