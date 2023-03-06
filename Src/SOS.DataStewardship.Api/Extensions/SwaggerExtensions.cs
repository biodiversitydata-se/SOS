using SOS.DataStewardship.Api.Application.Helpers;

namespace SOS.DataStewardship.Api.Extensions;

public static class SwaggerExtensions
{
    public static WebApplicationBuilder SetupSwagger(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.AddSwaggerGen(
            options =>
            {
                options.AddSecurityDefinition("Bearer", //Name the security scheme
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Description = "JWT Authorization header using the Bearer scheme.",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http, //We set the scheme type to http since we're using bearer authentication
                        Scheme = "bearer" //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
                    });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement{
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

                var schemaHelper = new SwashbuckleSchemaHelper();
                options.CustomSchemaIds(type => schemaHelper.GetSchemaId(type));
                //options.CustomSchemaIds(type => type.ToString());
                //options.CustomSchemaIds(type => type.FullName);
                options.DescribeAllParametersInCamelCase();
                options.EnableAnnotations();
            });

        webApplicationBuilder.Services.AddEndpointsApiExplorer();
        return webApplicationBuilder;
    }
}
