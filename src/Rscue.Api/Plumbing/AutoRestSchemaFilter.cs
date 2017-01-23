using System.Reflection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Rscue.Api.Plumbing
{
    public class AutoRestSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            var typeInfo = context.SystemType.GetTypeInfo();

            if (typeInfo.IsEnum)
            {
                schema.Extensions.Add(
                    "x-ms-enum",
                    new { name = typeInfo.Name, modelAsString = true }
                    );
            };
        }
    }
}