var builder = DistributedApplication.CreateBuilder(args);

// Get environment variables
var env = new
{
    AspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    UseLocalObservationApi = GetBoolEnv("USE_LOCAL_OBSERVATION_API"),
    UseLocalHangfireDb = GetBoolEnv("USE_LOCAL_HANGFIRE"),
    DisableHangfireInit = GetBoolEnv("DISABLE_HANGFIRE_INIT"),
    DisableHealthcheckInit = GetBoolEnv("DISABLE_HEALTHCHECK_INIT"),
    DisableCachedTaxonSumInit = GetBoolEnv("DISABLE_CACHED_TAXON_SUM_INIT")
};

// Configure Hangfire db
var hangfireDb = builder.AddMongoDB("hangfire-mongodb");
    //.WithMongoExpress();

// Configure Observation API
var observationApi = builder.AddProject<Projects.SOS_Observations_Api>("sos-observations-api", configure: static project =>
    {
        project.ExcludeLaunchProfile = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", env.AspNetCoreEnvironment)
    .WithEnvironment("USE_LOCAL_HANGFIRE", env.UseLocalHangfireDb.ToString())
    .WithEnvironment("DISABLE_HANGFIRE_INIT", env.DisableHangfireInit.ToString())
    .WithEnvironment("DISABLE_HEALTHCHECK_INIT", env.DisableHealthcheckInit.ToString())
    .WithEnvironment("DISABLE_CACHED_TAXON_SUM_INIT", env.DisableCachedTaxonSumInit.ToString())
    .WithHttpEndpoint(name: "http", port: 5000)
    .WithUrl("http://localhost:5000/swagger", "Observations API (Swagger)")
    .WithHttpHealthCheck("/healthz")
    .WithReference(hangfireDb)
    .WaitFor(hangfireDb);

// Configure Administration API
var adminApi = builder.AddProject<Projects.SOS_Administration_Api>(name: "sos-administration-api", configure: static project =>
{
    project.ExcludeLaunchProfile = true;
})
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", env.AspNetCoreEnvironment)
    .WithEnvironment("USE_LOCAL_HANGFIRE", env.UseLocalHangfireDb.ToString())
    .WithEnvironment("DISABLE_HANGFIRE_INIT", env.DisableHangfireInit.ToString())
    .WithHttpEndpoint(name: "http", port: 5005)
    .WithUrl("http://localhost:5005/swagger", "Administration API (Swagger)")
    .WithUrl("http://localhost:5005/hangfire/jobs/processing", "Hangfire dashboard")
    .WithHttpHealthCheck("/healthz")
    .WithReference(hangfireDb)
    .WaitFor(hangfireDb);

//// Configure Hangfire JobServer
//builder.AddProject<Projects.SOS_Hangfire_JobServer>("sos-hangfire-jobserver")
//    .WithHttpEndpoint()
//    .WithEnvironment("ASPNETCORE_ENVIRONMENT", env.AspNetCoreEnvironment)
//    .WithEnvironment("USE_LOCAL_HANGFIRE", env.UseLocalHangfireDb.ToString())
//    .WithReference(hangfireDb)
//    .WaitFor(hangfireDb);

//// Configure Admin GUI BFF
//var adminGuiBff = builder.AddProject<Projects.SOS_Administration_Gui>("sos-admin-gui-bff", configure: static project =>
//{
//    project.ExcludeLaunchProfile = true;
//})
//    .WithEnvironment("ASPNETCORE_ENVIRONMENT", env.AspNetCoreEnvironment)
//    .WithEnvironment("USE_LOCAL_OBSERVATION_API", env.UseLocalObservationApi.ToString())
//    .WithHttpEndpoint(name: "http", port: 5050)
//    .WithReference(observationApi)
//    .WithHttpHealthCheck("/healthz")
//    .WaitFor(observationApi)
//    .WithExplicitStart();

//// Configure Admin GUI (Angular)
//var adminGui = builder.AddNpmApp("sos-admin-gui-angular", "../SOS.Administration.Gui.Web")
//    .WithReference(adminGuiBff)
//    .WaitFor(adminGuiBff)
//    .WithHttpEndpoint(name: "angular-ui", env: "PORT")
//    .WithUrlForEndpoint("angular-ui", endpoint =>
//    {
//        endpoint.Url = "http://localhost:4200";
//        endpoint.DisplayText = $"Admin GUI (http://localhost:4200)";
//    })
//    .WithExplicitStart();

// Healthcheck UI
//builder.AddHealthChecksUI("healthchecksui")
//    .WithReference(observationApi)
//    .WaitFor(observationApi)
//    // This will make the HealthChecksUI dashboard available from external networks when deployed.
//    // In a production environment, you should consider adding authentication to the ingress layer
//    // to restrict access to the dashboard.
//    .WithExternalHttpEndpoints();

builder.Build().Run();

bool GetBoolEnv(string name) => Environment.GetEnvironmentVariable(name)?.ToLower() == "true";