﻿namespace SOS.UserStatistics.Api.Extensions;

public static class ModuleExtensions
{
    static readonly List<Modules.Interfaces.IModule> registeredModules = new();

    public static WebApplicationBuilder RegisterModules(this WebApplicationBuilder webApplicationBuilder)
    {
        var modules = DiscoverModulesInAssembly();
        foreach (var module in modules)
        {
            registeredModules.Add(module);
        }

        return webApplicationBuilder;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        foreach (var module in registeredModules)
        {
            module.MapEndpoints(app);
        }

        return app;
    }

    private static IEnumerable<Modules.Interfaces.IModule> DiscoverModulesInAssembly()
    {
        return typeof(Modules.Interfaces.IModule).Assembly.GetTypes()
            .Where(t => t.IsClass && t.IsAssignableTo(typeof(Modules.Interfaces.IModule)))
            .Select(Activator.CreateInstance)
            .Cast<Modules.Interfaces.IModule>();
    }
}
