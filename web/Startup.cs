﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using web.Filters;
using Microsoft.Extensions.Hosting;

namespace web
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
            string connectionString = dal.RCBenevoleContextFactory.GetConnectionString();

           
            services.AddDbContext<dal.RCBenevoleContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new RequestLogFilter());
                options.Filters.Add(new MaintenanceModeFilter());
                options.EnableEndpointRouting = false; // Use the routing logic of ASP.NET Core 2.1 or earlier:
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt => {
                    opt.Cookie.Name = "rcbene_auth";
                    opt.ExpireTimeSpan = TimeSpan.FromHours(2);
                    opt.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                    opt.LoginPath = "/Home";
                    opt.LogoutPath = "/Account/Logout";
                });

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
			// Pour utilisation d'un sous-répertoire via nginx (rendre configurable pour docker par variable d'environnement)
			var pathBase = Environment.GetEnvironmentVariable("APP_PATH_BASE");

            // Line below avoid errors while saving datetime in PostGreSQL
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            if (!string.IsNullOrEmpty(pathBase))
				app.UsePathBase(pathBase);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            // Pour que l'authentification soit bien gérée depuis un reverse-proxy. A appeler avant app.UseAuthentication():
			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				   ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});

            app.UseRequestLocalization(BuildLocalizationOptions());
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            // Astuce pour appeler une méthode de SeedData
            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            serviceScopeFactory.SeedData();
        }

        private RequestLocalizationOptions BuildLocalizationOptions()
        {
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("fr-FR"),
                new CultureInfo("en-US"),
                new CultureInfo("es-ES"),
                new CultureInfo("de-DE"),
            };
 
            var options = new RequestLocalizationOptions {
                //DefaultRequestCulture = new RequestCulture("fr-FR"),
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
 
            return options;
        }
    }
}
