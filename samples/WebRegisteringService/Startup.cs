using System;
using Consul.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebRegisteringService
{
  public class Startup
  {
    private readonly IHostingEnvironment _environment;
    private readonly IConfiguration _configuration;

    public Startup(IHostingEnvironment environment, IConfiguration configuration)
    {
      _environment = environment;
      _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddHealthChecks();
      
      var consulOptions = new ConsulOptions();
      _configuration.GetSection("Consul").Bind(consulOptions);
      services.Configure<ConsulOptions>(c =>
      {
        c.Address = consulOptions.Address;
        c.ServiceId = consulOptions.ServiceId;
        c.ServiceName = consulOptions.ServiceName;
      });

      services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(c =>
      {
        c.Address = new Uri(consulOptions.Address);
      }));
      
      services
        .AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

      services.AddHostedService<ConsulHostedService>();
    }

    public void Configure(IApplicationBuilder app)
    {
      if (_environment.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      
      app.UseHealthChecks("/health");
      app.UseMvcWithDefaultRoute();
      //app.UseEndpoints(builder =>
      //{
      //  builder.MapDefaultControllerRoute();
      //});
    }
  }
}