namespace SOS.UserStatistics.Api.Extensions;

internal static class SwaggerExtensions
{
    internal static WebApplicationBuilder SetupSwagger(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.AddSwaggerGen(
            swagger =>
            {
                swagger.AddSecurityDefinition("Bearer", //Name the security scheme
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Description = "JWT Authorization header using the Bearer scheme.",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http, //We set the scheme type to http since we're using bearer authentication
                        Scheme = "bearer" //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
                    });

                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecurityScheme{
                            Scheme = "bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                            Reference = new OpenApiReference{
                                Id = "Bearer", //The name of the previously defined security scheme.
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });
        return webApplicationBuilder;
    }
}
