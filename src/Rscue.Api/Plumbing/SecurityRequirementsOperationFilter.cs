using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Rscue.Api.Plumbing
{
    public class SecurityRequirementsOperationFilter : IOperationFilter, IDocumentFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            // Policy names map to scopes
            var controllerScopes = context.ApiDescription.ControllerAttributes()
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy);

            var actionScopes = context.ApiDescription.ActionAttributes()
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy);

            var requiredScopes = controllerScopes.Union(actionScopes).Distinct();

            if (requiredScopes.Any())
            {
                operation.Responses.Add("401", new Response { Description = "Unauthorized" });
                operation.Responses.Add("403", new Response { Description = "Forbidden" });

                operation.Parameters.Add(new NonBodyParameter()
                {
                    Name = "Authorization",
                    In = "header",
                    Description = "JWT security token obtained from Identity Server.",
                    Required = true,
                    Type = "string"
                });

                operation.Security = new List<IDictionary<string, IEnumerable<string>>>
                {
                    new Dictionary<string, IEnumerable<string>>
                    {
                        {"oauth2", requiredScopes}
                    }
                };
            }
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            IList<IDictionary<string, IEnumerable<string>>> security = swaggerDoc.SecurityDefinitions
                .Select(securityDefinition => new Dictionary<string, IEnumerable<string>>
        {
            {securityDefinition.Key, new string[] {"yourapi"}}
        }).Cast<IDictionary<string, IEnumerable<string>>>().ToList();
            swaggerDoc.Security = security;
        }
    }
}