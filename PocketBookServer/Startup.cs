using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PocketBookServer.Controllers.API;
using PocketBookServer.Data;
using PocketBookServer.Models;
using PocketBookServer.Services;
using SendGrid;
using System;
using System.IO;
using System.Security.Claims;
using System.Text;

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

                var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
                if (roleManager.FindByNameAsync("Admin").Result == null)
                {
                    var adminRole = new IdentityRole
                    {
                        Name = "Admin"
                    };
                    roleManager.CreateAsync(adminRole).Wait();
                    roleManager.AddClaimAsync(adminRole, new Claim("role", "Admin")).Wait();
                }

                var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                if (userManager.FindByEmailAsync("tony.richards@sja.org.uk").Result == null)
                {
                    var user = new ApplicationUser
                    {
                        Email = Configuration["AdminUser:UserName"],
                        EmailConfirmed = true,
                        RealName = Configuration["AdminUser:RealName"],
                        UserName = Configuration["AdminUser:UserName"],
                        UpdateEmailConsentGiven = true
                    };
                    userManager.CreateAsync(user, Configuration["AdminUser:Password"]).Wait();
                }
                var adminUser = userManager.FindByEmailAsync("tony.richards@sja.org.uk").Result;
                if (!userManager.IsInRoleAsync(adminUser, "Admin").Result)
                {
                    userManager.AddToRoleAsync(adminUser, "Admin").Wait();
                }
            }

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var manager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

                if (!manager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                {
                    var role = new IdentityRole()
                    {
                        Name = "Admin"
                    };
                    manager.CreateAsync(role).GetAwaiter().GetResult();
                    manager.AddClaimAsync(role, new Claim("role", "Admin")).GetAwaiter().GetResult();
                }
            }

            app.UseRouting();
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
            app.UseAuthentication();
            app.UseAuthorization();

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
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDataContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddApplicationInsightsTelemetry();

            services.AddDataProtection()
               .SetApplicationName($"pocketbook-{_env.EnvironmentName}")
               .PersistKeysToFileSystem(new DirectoryInfo($@"{_env.ContentRootPath}\..\local_keys"));

            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
                {
                    o.User.RequireUniqueEmail = true;
                    o.SignIn.RequireConfirmedEmail = true;
                    o.Password.RequireDigit = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequiredLength = 1;
                })
                .AddEntityFrameworkStores<ApplicationDataContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    var key = Encoding.ASCII.GetBytes(Configuration["TokenSecret"]);

                    if (_env.IsDevelopment())
                        x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidAudience = Configuration["TokenAudience"],
                        ValidIssuer = Configuration["TokenIssuer"]
                    };
                });

            services.AddHsts(o =>
            {
                o.Preload = true;
                o.IncludeSubDomains = true;
                o.MaxAge = TimeSpan.FromSeconds(63072000);
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PocketBook Server", Version = "v1" });
            });

            services.AddControllers();

            var emailOptions = Configuration.Get<EmailSenderOptions>();

            services.AddTransient<ISendGridClient>(sp => new SendGridClient(emailOptions.SendGridKey));
            services.AddTransient<ITokenGenerator, TokenGenerator>();
            services.AddSingleton<IEmailSender, EmailSender>();
            services.Configure<TokenGeneratorOptions>(Configuration);
            services.Configure<EmailSenderOptions>(Configuration);
            services.Configure<UserControllerOptions>(Configuration);
        }
    }
}