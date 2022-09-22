﻿using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using SOS.Lib.ActionFilters;
using SOS.Lib.ApplicationInsights;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Configuration.Statistics;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.JsonConverters;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Swagger;
using SOS.Statistics.Api.Repositories;
using SOS.Statistics.Api.Repositories.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace SOS.Statistics.Api
{
    /// <summary>
    ///     Program class
    /// </summary>
    public class Startup
    {
        private const string InternalApiName = "InternalSosStatistics";
        private const string PublicApiName = "PublicSosStatistics";
        private const string InternalApiPrefix = "Internal";

        private bool _isDevelopment;

        /// <summary>
        ///     Start up
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            var environment = env.EnvironmentName.ToLower();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables();

            _isDevelopment = environment.Equals("local");
            if (_isDevelopment)
            {
                // If Development mode, add secrets stored on developer machine 
                // (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
                // In production you should store the secret values as environment variables.
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
            services.AddMemoryCache();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new GeoShapeConverter());
                    options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

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
                .AddJwtBearer(options =>
                {
                    options.Audience = identityServerConfiguration.Audience;
                    options.Authority = identityServerConfiguration.Authority;
                    options.RequireHttpsMetadata = identityServerConfiguration.RequireHttpsMetadata;
                    options.TokenValidationParameters.RoleClaimType = "rname";
                });

            // Add application insights.
            services.AddApplicationInsightsTelemetry(Configuration);
            // Application insights custom
            services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
            services.AddSingleton(Configuration.GetSection("ApplicationInsights").Get<ApplicationInsights>());
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();

            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 5);
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
                    swagger.UseInlineDefinitionsForEnums();
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    {                        
                        swagger.SwaggerDoc(
                            $"{InternalApiName}{description.GroupName}",
                            new OpenApiInfo()
                            {
                                Title = $"SOS Statistics API (Internal) {description.GroupName.ToUpperInvariant()}",
                                Version = description.ApiVersion.ToString(),
                                Description = "Species Observation System (SOS) - Statistics API. Internal API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                            });

                        swagger.SwaggerDoc(
                            $"{PublicApiName}{description.GroupName}",
                            new OpenApiInfo()
                            {
                                Title = $"SOS Statistics API (Public) {description.GroupName.ToUpperInvariant()}",
                                Version = description.ApiVersion.ToString(),
                                Description = "Species Observation System (SOS) - Statistics API. Public API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                            });

                        swagger.CustomOperationIds(apiDesc =>
                        {
                            apiDesc.TryGetMethodInfo(out MethodInfo methodInfo);
                            string controller = apiDesc.ActionDescriptor.RouteValues["controller"];
                            string methodName = methodInfo.Name;
                            return $"{controller}_{methodName}".Replace("Async", "", StringComparison.InvariantCultureIgnoreCase);
                        });
                    }

                    // add a custom operation filters
                    swagger.OperationFilter<SwaggerDefaultValues>();
                    swagger.OperationFilter<SwaggerAddOptionalHeaderParameters>();

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
                    // Post-modify Operation descriptions once they've been generated by wiring up one or more
                    // Operation filters.
                    swagger.OperationFilter<ApiManagementDocumentationFilter>();

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

            var statisticsConfiguration = Configuration.GetSection("StatisticsConfiguration")
                .Get<StatisticsConfiguration>();

            // Response compression
            if (statisticsConfiguration.EnableResponseCompression)
            {
                services.AddResponseCompression(o => o.EnableForHttps = true);
                services.Configure<BrotliCompressionProviderOptions>(options =>
                {
                    options.Level = statisticsConfiguration.ResponseCompressionLevel;
                });
                services.Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = statisticsConfiguration.ResponseCompressionLevel;
                });
            }

            //setup the elastic search configuration
            var elasticConfiguration = Configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
            services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(elasticConfiguration));

            // Processed Mongo Db
            var processedDbConfiguration = Configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            services.AddScoped<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));

            // Add configuration
            services.AddSingleton(elasticConfiguration);
            services.AddSingleton(statisticsConfiguration);
            services.AddSingleton(Configuration.GetSection("UserServiceConfiguration").Get<UserServiceConfiguration>());

            // Add security
            services.AddScoped<IAuthorizationProvider, CurrentUserAuthorization>();

            // Add Caches
            services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();

            // Add managers
            services.AddScoped<IFilterManager, FilterManager>();
            services.AddSingleton<ITaxonManager, TaxonManager>();

            // Add repositories
            services.AddScoped<IProcessedObservationRepository, ProcessedObservationRepository>();
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="apiVersionDescriptionProvider"></param>
        /// <param name="configuration"></param>
        /// <param name="applicationInsightsConfiguration"></param>
        /// <param name="observationApiConfiguration"></param>
        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env, 
            IApiVersionDescriptionProvider apiVersionDescriptionProvider, 
            TelemetryConfiguration configuration, 
            ApplicationInsights applicationInsightsConfiguration, 
            StatisticsConfiguration statisticsConfiguration)
        {
            if (statisticsConfiguration.EnableResponseCompression)
            {
                app.UseResponseCompression();
            }
            
            if (_isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
#if DEBUG
            configuration.DisableTelemetry = true;
#endif
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{InternalApiName}{description.GroupName}/swagger.json",
                        $"SOS Statistics API (Internal) {description.GroupName.ToUpperInvariant()}");

                    options.SwaggerEndpoint(
                        $"/swagger/{PublicApiName}{description.GroupName}/swagger.json",
                        $"SOS Statistics API (Public) {description.GroupName.ToUpperInvariant()}");

                    options.DisplayOperationId();                    
                    options.DocExpansion(DocExpansion.None);
                }
            });
         
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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
}