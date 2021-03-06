﻿using IdentityModel;
using ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }
 
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // register an IHttpContextAccessor so we can access the current HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();

            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = "Cookies";
                opt.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies", (options) => 
            {
                options.AccessDeniedPath = "/Authorization/AccessDenied";
            })
            .AddOpenIdConnect("oidc", opt => 
            {
                opt.SignInScheme = "Cookies";
                opt.Authority = "https://localhost:44379";
                opt.ClientId = "imagegalleryclient";
                opt.ResponseType = "code id_token";
                opt.Scope.Add("openid");
                opt.Scope.Add("profile");
                opt.Scope.Add("address");
                opt.Scope.Add("roles");
                opt.SaveTokens = true;
                opt.ClientSecret = "secret";
                opt.GetClaimsFromUserInfoEndpoint = true;
                opt.ClaimActions.Remove("amr");
                opt.ClaimActions.DeleteClaim("sid");
                opt.ClaimActions.DeleteClaim("idp");
                //opt.ClaimActions.DeleteClaim("address");
                opt.ClaimActions.MapUniqueJsonKey("role", "role");

                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.GivenName,
                    RoleClaimType = JwtClaimTypes.Role
                };
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
