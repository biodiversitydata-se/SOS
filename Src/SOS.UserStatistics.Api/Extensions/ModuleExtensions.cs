namespace SOS.UserStatistics.Api.Extensions;

internal static class ModuleExtensions
{
    static readonly List<IModule> registeredModules = new();

    internal static WebApplicationBuilder RegisterModules(this WebApplicationBuilder webApplicationBuilder)
    {
        var modules = DiscoverModulesInAssembly();
        foreach (var module in modules)
        {
            registeredModules.Add(module);
        }

        return webApplicationBuilder;
    }

    internal static WebApplication MapEndpoints(this WebApplication app)
    {
        foreach (var module in registeredModules)
        {
            module.MapEndpoints(app);
        }

        return app;
    }

    private static IEnumerable<IModule> DiscoverModulesInAssembly()
    {
        return typeof(IModule).Assembly.GetTypes()
            .Where(t => t.IsClass && t.IsAssignableTo(typeof(IModule)))
            .Select(Activator.CreateInstance)
            .Cast<IModule>();
    }
}
