using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;

namespace SOS.Administration.Api.Extensions;

public static class SwaggerExtensions
{    
    public static IServiceCollection SetupSwagger(this IServiceCollection services)
    {       
        services.AddSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "SOS.Administration.Api",
                        Version = "v1",
                        Description = "An API to handle various processing jobs" //,
                        //TermsOfService = "None"
                    });
                // Set the comments path for the Swagger JSON and UI.
                var currentAssembly = Assembly.GetExecutingAssembly();
                var xmlDocs = currentAssembly.GetReferencedAssemblies()
                    .Union(new AssemblyName[] { currentAssembly.GetName() })
                    .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
                    .Where(f => File.Exists(f)).ToArray();

                Array.ForEach(xmlDocs, (d) =>
                {
                    options.IncludeXmlComments(d);
                });
            });

        return services;
    }

    public static WebApplication ApplyUseSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ObservationProcessingJobs API, version 1");
        });

        return app;
    }
}