using Microsoft.OpenApi;
using SOS.Lib.Swagger;

namespace SOS.DataStewardship.Api.Extensions;

public static class SwaggerExtensions
{
    extension(WebApplicationBuilder webApplicationBuilder)
    {
        public WebApplicationBuilder SetupSwagger()
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

                    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });

                    //var schemaHelper = new SwashbuckleSchemaHelper();
                    //options.CustomSchemaIds(type => schemaHelper.GetSchemaId(type));
                    //options.CustomSchemaIds(type => type.ToString());
                    //options.CustomSchemaIds(type => type.FullName);                
                    options.DescribeAllParametersInCamelCase();
                    options.EnableAnnotations();
                    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                    options.SchemaFilter<SwaggerIgnoreFilter>();
                });

            webApplicationBuilder.Services.AddEndpointsApiExplorer();

            return webApplicationBuilder;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication PreventSwaggerCaching()
        {
            //  Prevent caching on Swagger UI and swagger.json
            app.Use(async (context, next) =>
            {
                try
                {
                    if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
                        context.Response.Headers["Pragma"] = "no-cache";
                        context.Response.Headers["Expires"] = "0";
                    }

                    await next();
                }
                catch (TaskCanceledException) { }
                catch (Exception)
                {
                    throw;
                }
            });

            return app;
        }
    }
}
