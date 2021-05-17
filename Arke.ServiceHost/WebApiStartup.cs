using System.Reflection;
using Arke.DependencyInjection;
using Arke.ManagementApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector.Integration.AspNetCore.Mvc;

namespace Arke.ServiceHost
{
    public class WebApiStartup
    {
        public WebApiStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = typeof(StepsController).GetTypeInfo().Assembly;
            var part = new AssemblyPart(assembly);
            
            services.AddMvc((options =>
            {
                options.EnableEndpointRouting = false;
            })).ConfigureApplicationPartManager(apm =>
            {
                apm.ApplicationParts.Add(part);
                
            });
            
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Arke API"));
            app.UseRouting();
            app.UseMvc();
        }

    }
}