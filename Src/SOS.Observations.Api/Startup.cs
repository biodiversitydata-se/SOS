using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Elasticsearch.Net;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Nest;
using NLog.Web;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.JsonConverters;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Services;
using SOS.Observations.Api.Services.Interfaces;
using SOS.Observations.Api.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using IProcessedObservationRepository = SOS.Observations.Api.Repositories.Interfaces.IProcessedObservationRepository;
using ProcessedObservationRepository = SOS.Observations.Api.Repositories.ProcessedObservationRepository;

namespace SOS.Observations.Api
{
    /// <summary>
    ///     Program class
    /// </summary>
    public class Startup
    {
        private const string InternalApiName = "InternalSosObservations";
        private const string PublicApiName = "PublicSosObservations";
        private const string InternalApiPrefix = "Internal";
        private const string PublicApiPrefix = "Public";
        private readonly string _environment;

        /// <summary>
        ///     Start up
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            _environment = env.EnvironmentName.ToLower();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{_environment}.json", true)
                .AddEnvironmentVariables();

            if (_environment.Equals("local"))
            {
                //Add secrets stored on developer machine (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        /// <summary>
        ///     Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new GeoShapeConverter()); });

            // MongoDB conventions.
            ConventionRegistry.Register(
                "MongoDB Solution Conventions",
                new ConventionPack
                {
                    new IgnoreExtraElementsConvention(true),
                    new IgnoreIfNullConvention(true)
                },
                t => true);

            // Identity service configuration
            var identityServerConfiguration = Configuration.GetSection("IdentityServer").Get<IdentityServerConfiguration>();

            // Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.Audience = identityServerConfiguration.Audience;
                    options.Authority = identityServerConfiguration.Authority;
                    options.RequireHttpsMetadata = identityServerConfiguration.RequireHttpsMetadata;
                    options.TokenValidationParameters.RoleClaimType = "rname";
                });

            // Add Mvc Core services
            services.AddMvcCore(option => { option.EnableEndpointRouting = false; })
                .AddApiExplorer()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // Add application insights.
            services.AddApplicationInsightsTelemetry(Configuration);
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.ReportApiVersions = true;
                o.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
            });

            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });

            var apiVersionDescriptionProvider =
                services.BuildServiceProvider().GetService<IApiVersionDescriptionProvider>();
            services.AddSwaggerGen(
                swagger =>
                {
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    {
                        swagger.SwaggerDoc(
                            $"{InternalApiName}{description.GroupName}",
                            new OpenApiInfo()
                            {
                                Title = $"SOS Observations API (Internal) {description.GroupName.ToUpperInvariant()}",
                                Version = description.ApiVersion.ToString(),
                                Description = "Species Observation System (SOS) - Observations API. Internal API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                            });

                        swagger.SwaggerDoc(
                            $"{PublicApiName}{description.GroupName}",
                            new OpenApiInfo()
                            {
                                Title = $"SOS Observations API (Public) {description.GroupName.ToUpperInvariant()}",
                                Version = description.ApiVersion.ToString(),
                                Description = "Species Observation System (SOS) - Observations API. Public API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                            });
                    }

                    // add a custom operation filter which sets default values
                    swagger.OperationFilter<SwaggerDefaultValues>();

                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var xmlDocs = currentAssembly.GetReferencedAssemblies()
                        .Union(new AssemblyName[] { currentAssembly.GetName() })
                        .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
                        .Where(f => File.Exists(f)).ToArray();

                    Array.ForEach(xmlDocs, (d) =>
                    {
                        swagger.IncludeXmlComments(d);
                    });

                    swagger.SchemaFilter<SwaggerIgnoreFilter>();

                    swagger.DocInclusionPredicate((documentName, apiDescription) =>
                    {
                        var apiVersions = GetApiVersions(apiDescription);
                        bool isEndpointInternalApi = apiDescription.ActionDescriptor.EndpointMetadata.Any(x => x.GetType() == typeof(InternalApiAttribute));
                        if (isEndpointInternalApi && !documentName.StartsWith(InternalApiPrefix)) return false;
                        return apiVersions.Any(v =>
                                   $"{InternalApiName}v{v}" == documentName) ||
                               apiVersions.Any(v =>
                                   $"{PublicApiName}v{v}" == documentName);
                    });

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

            var observationApiConfiguration = Configuration.GetSection("ObservationApiConfiguration")
                .Get<ObservationApiConfiguration>();

            // Hangfire
            var mongoConfiguration = observationApiConfiguration.HangfireDbConfiguration;

            services.AddHangfire(configuration =>
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseMongoStorage(new MongoClient(mongoConfiguration.GetMongoDbSettings()),
                        mongoConfiguration.DatabaseName,
                        new MongoStorageOptions
                        {
                            MigrationOptions = new MongoMigrationOptions
                            {
                                MigrationStrategy = new MigrateMongoMigrationStrategy(),
                                BackupStrategy = new CollectionMongoBackupStrategy()
                            },
                            Prefix = "hangfire",
                            CheckConnection = true
                        })
            );

            //setup the elastic search configuration
            var elasticConfiguration = observationApiConfiguration.SearchDbConfiguration;
            var uris = elasticConfiguration.Hosts.Select(u => new Uri(u));
            services.AddSingleton<IElasticClient>(
                new ElasticClient(new ConnectionSettings(new StaticConnectionPool(uris))
                        .BasicAuthentication(elasticConfiguration.UserName, elasticConfiguration.Password)
                //.DisableDirectStreaming().EnableDebugMode().PrettyJson() // Uncomment this line when debugging ES-query. Req and Resp is in result.DebugInformation in ProcessedObservationRepository.cs.
                ));

            // Processed Mongo Db
            var processedDbConfiguration = observationApiConfiguration.ProcessDbConfiguration;
            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize);
            services.AddSingleton<IProcessClient>(processClient);

            var blobStorageConfiguration = Configuration.GetSection("BlobStorageConfiguration")
                .Get<BlobStorageConfiguration>();

            // Add configuration
            services.AddSingleton(observationApiConfiguration);
            services.AddSingleton(blobStorageConfiguration);
            services.AddSingleton(elasticConfiguration);
            services.AddSingleton(observationApiConfiguration.UserServiceConfiguration);

            // Add managers
            services.AddSingleton<IAreaManager, AreaManager>();
            services.AddSingleton<IDataProviderManager, DataProviderManager>();
            services.AddSingleton<IBlobStorageManager, BlobStorageManager>();
            services.AddSingleton<IFieldMappingManager, FieldMappingManager>();
            services.AddScoped<IObservationManager, ObservationManager>();
            services.AddScoped<IProcessInfoManager, ProcessInfoManager>();
            services.AddScoped<ITaxonManager, TaxonManager>();
            services.AddSingleton<IFilterManager, FilterManager>();

            // Add repositories
            services.AddSingleton<IAreaRepository, AreaRepository>();
            services.AddSingleton<IDataProviderRepository, DataProviderRepository>();
            services.AddScoped<IProcessedObservationRepository, ProcessedObservationRepository>();
            services.AddScoped<IProcessInfoRepository, ProcessInfoRepository>();
            services.AddScoped<ITaxonRepository, TaxonRepository>();
            services.AddSingleton<IFieldMappingRepository, FieldMappingRepository>();

            // Add services
            services.AddSingleton<IBlobStorageService, BlobStorageService>();
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddSingleton<IUserService, UserService>();
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="apiVersionDescriptionProvider"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            NLogBuilder.ConfigureNLog($"nlog.{env.EnvironmentName}.config");
            if (new[] {"dev", "local"}.Contains(env.EnvironmentName.ToLower()))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options => {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{InternalApiName}{description.GroupName}/swagger.json",
                        $"SOS Observations API (Internal) {description.GroupName.ToUpperInvariant()}");

                    options.SwaggerEndpoint(
                        $"/swagger/{PublicApiName}{description.GroupName}/swagger.json",
                        $"SOS Observations API (Public) {description.GroupName.ToUpperInvariant()}");

                    options.DocExpansion(DocExpansion.None);
                }
            });


            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] {new AllowAllConnectionsFilter()},
                IgnoreAntiforgeryToken = true
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private static IReadOnlyList<ApiVersion> GetApiVersions(ApiDescription apiDescription)
        {
            var actionApiVersionModel = apiDescription.ActionDescriptor
                .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

            var apiVersions = actionApiVersionModel.DeclaredApiVersions.Any()
                ? actionApiVersionModel.DeclaredApiVersions
                : actionApiVersionModel.ImplementedApiVersions;
            return apiVersions;
        }
    }

    /// <summary>
    /// </summary>
    public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Authorize(DashboardContext context)
        {
            // Allow outside. You need an authentication scenario for this part.
            // DON'T GO PRODUCTION WITH THIS LINES.
            return true;
        }
    }
}