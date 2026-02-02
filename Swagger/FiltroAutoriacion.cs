using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BibliotecaAPI.Swagger
{
    public class FiltroAutoriacion : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize = context.ApiDescription.ActionDescriptor.EndpointMetadata
                            .OfType<AuthorizeAttribute>().Any();

            var hasAllowAnonymous = context.ApiDescription.ActionDescriptor.EndpointMetadata
                            .OfType<AllowAnonymousAttribute>().Any();

            if (!hasAuthorize || hasAllowAnonymous)
                return;

            operation.Security = new List<OpenApiSecurityRequirement>
    {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };

        }
    }
}
