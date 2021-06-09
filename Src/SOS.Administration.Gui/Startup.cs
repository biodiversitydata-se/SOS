using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SOS.Administration.Gui.Services;
using SOS.Lib.Configuration.Shared;
using System.Text;
using System.Text.Json.Serialization;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;

namespace SOS.Administration.Gui
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.AddMvcCore().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            var authConfig = Configuration.GetSection(nameof(AuthenticationConfiguration));
            services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authConfig["Issuer"],
                    ValidAudience = authConfig["Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig["SecretKey"]))
                };
            });
            services.Configure<MongoDbConfiguration>(
                Configuration.GetSection(nameof(MongoDbConfiguration)));

            services.Configure<AuthenticationConfiguration>(authConfig);

            services.Configure<ElasticSearchConfiguration>(
              Configuration.GetSection(nameof(ElasticSearchConfiguration)));

            services.Configure<TestElasticSearchConfiguration>(
                Configuration.GetSection(nameof(TestElasticSearchConfiguration)));

            services.Configure<ApiTestConfiguration>(
              Configuration.GetSection(nameof(ApiTestConfiguration)));

            services.Configure<ApplicationInsightsConfiguration>(Configuration.GetSection(nameof(ApplicationInsightsConfiguration)));
            services.AddTransient<ISearchService, SearchService>();
            services.AddTransient<ITestService, TestService>();

            services.AddSingleton(Configuration.GetSection(nameof(ApplicationInsightsConfiguration)).Get<ApplicationInsightsConfiguration>());
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddScoped<IApplicationInsightsService, ApplicationInsightsService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
