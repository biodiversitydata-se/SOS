using SOS.DataStewardship.Api.Endpoints;

namespace SOS.DataStewardship.Api.Extensions;

public static class EndpointDefinitionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddEndpointDefinitions(
params Type[] scanMarkers)
        {
            var endpointDefinitions = new List<IEndpointDefinition>();

            foreach (var marker in scanMarkers)
            {
                endpointDefinitions.AddRange(
                    marker.Assembly.ExportedTypes
                        .Where(x => typeof(IEndpointDefinition).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                        .Select(Activator.CreateInstance).Cast<IEndpointDefinition>()
                    );
            }

            services.AddSingleton(endpointDefinitions as IReadOnlyCollection<IEndpointDefinition>);
        }
    }

    extension(WebApplication app)
    {
        public void UseEndpointDefinitions()
        {
            var definitions = app.Services.GetRequiredService<IReadOnlyCollection<IEndpointDefinition>>();

            foreach (var endpointDefinition in definitions)
            {
                endpointDefinition.DefineEndpoint(app);
            }
        }
    }
}