using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web.Resource;
using Microsoft.OpenApi.Models;
using PocketBookModel;
using System;
using System.IO;

namespace PocketBookServer
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDataContext>();
                context.Database.Migrate();
            }

            app.UseRouter(r =>
            {
                r.MapGet(".well-known/acme-challenge/{id}", async (request, response, routeData) =>
                {
                    var id = routeData.Values["id"] as string;
                    var file = Path.Combine(env.WebRootPath, "..", ".well-known", "acme-challenge", id);
                    await response.SendFileAsync(file);
                });
            });

            app.UseHttpsRedirection();

            var cacheLength = env.IsDevelopment() ? "600" : "604800";

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    var name = ctx.File.Name;

                    if (ctx.File.PhysicalPath.Contains("static") || name.EndsWith("png"))
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cacheLength}");
                    }
                }
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapWhen(x => !x.Request.Path.Value.StartsWith("/api"), builder =>
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "PocketBook v1"); });

                app.UseEndpoints(b =>
                {
                    b.MapFallbackToController("Index", "Home");
                });
            });

            app.UseEndpoints(builder =>
            {
                builder.MapDefaultControllerRoute();
                builder.MapHealthChecks("/health");
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection()
               .SetApplicationName($"pocketbook-{_env.EnvironmentName}")
               .PersistKeysToFileSystem(new DirectoryInfo($@"{_env.ContentRootPath}\..\local_keys"));

            services.AddDbContext<ApplicationDataContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(o =>
                {
                    Configuration.Bind("AzureAd", o);
                });

            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                // This is a Microsoft identity platform web API.
                options.Authority += "/v2.0";

                // The valid audiences are both the Client ID (options.Audience) and api://{ClientID}
                options.TokenValidationParameters.ValidAudiences = new string[]
                {
                    options.Audience, $"api://{options.Audience}"
                };

                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;
            });

            services.AddApplicationInsightsTelemetry();

            services.AddControllers();

            services.AddHsts(o =>
            {
                o.Preload = true;
                o.IncludeSubDomains = true;
                o.MaxAge = TimeSpan.FromSeconds(63072000);
            });

            services.AddHealthChecks();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PocketBook Server", Version = "v1" });
            });
        }
    }
}
