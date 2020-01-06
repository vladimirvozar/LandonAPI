using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandonAPI.Filters;
using LandonAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSwag.AspNetCore;
using Microsoft.EntityFrameworkCore;
using LandonAPI.Services;
using AutoMapper;
using LandonAPI.Infrastructure;

namespace LandonAPI
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
            services.Configure<HotelInfo>(Configuration.GetSection("Info"));

            services.AddScoped<IRoomService, DefaultRoomService>();

            // Use in-memory database for quick dev and testing
            // TODO: Swap out for a real database in production
            services.AddDbContext<HotelApiDbContext>(options => options.UseInMemoryDatabase("landondb"));

            services
                .AddMvc(options =>
                {
                    options.Filters.Add<JsonExceptionFilter>();
                    options.Filters.Add<RequireHttpsOrCloseAttribute>();
                    options.Filters.Add<LinkRewritingFilter>();
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new MediaTypeApiVersionReader();
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
            });

            // Register the Swagger services
            services.AddSwaggerDocument();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowMyApp", policy => policy.AllowAnyOrigin());
            });

            services.AddAutoMapper(options => options.AddProfile<MappingProfile>(), AppDomain.CurrentDomain.GetAssemblies());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseOpenApi();
                app.UseSwaggerUi3();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("AllowMyApp");
            app.UseMvc();
        }
    }
}
