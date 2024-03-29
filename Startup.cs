﻿using Cash_Flow_Projection.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog.Context;

namespace Cash_Flow_Projection
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add functionality to inject IOptions<T>
            services.AddOptions();

            // Add the Auth0 Settings object so it can be injected
            services.Configure<Auth0Settings>(Configuration.GetSection("Auth0"));

            var auth0Settings = new Auth0Settings { };

            Configuration.GetSection("Auth0").Bind(auth0Settings);

            // Add authentication services

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Home/Login");
                    options.Cookie.SameSite = SameSiteMode.None;
                })
                .AddOpenIdConnect("Auth0", options =>
                {
                    options.Authority = $"https://{auth0Settings.Domain}";

                    // Configure the Auth0 Client ID and Client Secret
                    options.ClientId = auth0Settings.ClientId;
                    options.ClientSecret = auth0Settings.ClientSecret;

                    options.ResponseType = OpenIdConnectResponseType.Code;

                    // Configure the scopes
                    options.Scope.Clear();
                    options.Scope.Add("profile");
                    options.Scope.Add("openid");
                    options.Scope.Add("email");

                    // Set the callback path, so Auth0 will call back to http://localhost:5000/signin-auth0
                    // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
                    options.CallbackPath = new PathString("/signin-auth0");

                    // Configure the Claims Issuer to be Auth0
                    options.ClaimsIssuer = "Auth0";
                });

            services.AddAuthorization();

            services.AddControllersWithViews();

            services.AddRazorPages();

            services.AddServerSideBlazor();

            services.AddProgressiveWebApp();

            services.AddDbContext<Database>(options => options.UseSqlite(Configuration.GetConnectionString("Sqlite")));
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Database db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.None
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                using (LogContext.PushProperty("Username", context.User?.Identity?.Name))
                {
                    await next();
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            Database.Init(db);
        }
    }
}
