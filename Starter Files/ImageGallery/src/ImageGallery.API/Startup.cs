using IdentityServer4.AccessTokenValidation;
using ImageGallery.API.Entities;
using ImageGallery.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageGallery.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
             services.AddMvc();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://localhost:44379/";
                    options.ApiName = "imagegalleryapi";
                });
        
            var connectionString = Configuration["ConnectionStrings:imageGalleryDBConnectionString"];
            services.AddDbContext<GalleryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<IGalleryRepository, GalleryRepository>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory, GalleryContext galleryContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        // ensure generic 500 status code on fault.
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                // Map from Image (entity) to Image, and back
                cfg.CreateMap<Image, Model.Image>().ReverseMap();

                // Map from ImageForCreation to Image
                // Ignore properties that shouldn't be mapped
                cfg.CreateMap<Model.ImageForCreation, Image>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());

                // Map from ImageForUpdate to Image
                // ignore properties that shouldn't be mapped
                cfg.CreateMap<Model.ImageForUpdate, Image>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());
            });

            AutoMapper.Mapper.AssertConfigurationIsValid();            

            app.UseMvc(); 
        }
    }
}
