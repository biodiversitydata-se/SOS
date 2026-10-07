using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using SOS.Analysis.Api.Controllers;
using SOS.Observations.Api.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using System.Text.Json.Serialization;

if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
{
    Console.Error.WriteLine("Usage: dotnet run --project Tools/SOS.OpenApi -- <output-directory>");
    return 1;
}

var outputDirectory = Path.GetFullPath(args[0]);
Directory.CreateDirectory(outputDirectory);
WriteDocuments(typeof(ObservationsController).Assembly,
    "PublicSosObservationsv1.5", "InternalSosObservationsv1.5",
    "AzurePublicSosObservationsv1.5", "AzureInternalSosObservationsv1.5");
WriteDocuments(typeof(AnalysisController).Assembly,
    "PublicSosAnalysisv1.5", "InternalSosAnalysisv1.5");
return 0;

void WriteDocuments(Assembly assembly, params string[] documentNames)
{
    var services = new ServiceCollection();
    services.AddLogging();
    var environment = new OpenApiEnvironment(assembly.GetName().Name!);
    services.AddSingleton<IWebHostEnvironment>(environment);
    services.AddSingleton<IHostEnvironment>(environment);
    services.AddControllers().AddApplicationPart(assembly)
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    if (assembly == typeof(ObservationsController).Assembly)
        SOS.Observations.Api.Extensions.SwaggerExtensions.SetupSwagger(services);
    else
        SOS.Analysis.Api.Extensions.SwaggerExtensions.SetupSwagger(services);

    using var provider = services.BuildServiceProvider();
    var swagger = provider.GetRequiredService<ISwaggerProvider>();
    foreach (var name in documentNames)
    {
        var path = Path.Combine(outputDirectory, $"{name}.json");
        using var writer = new StreamWriter(path);
        swagger.GetSwagger(name).SerializeAsV3(new OpenApiJsonWriter(writer));
        Console.WriteLine(path);
    }
}

sealed class OpenApiEnvironment(string applicationName) : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = applicationName;
    public string EnvironmentName { get; set; } = "Development";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
}
