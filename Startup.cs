using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServer4.Services;
using Swashbuckle.AspNetCore.Swagger;
using AspNetAuth.Models;
using AspNetAuth.Data;
using AspNetAuth.Services;
using AspNetAuth.Imp;
using AspNetAuth.Identity;

namespace AspNetAuth
{
  public class Startup
  {
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
          .AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

      services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

      services.AddIdentity<ApplicationUser, IdentityRole>()
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddDefaultTokenProviders();
      // Add framework services.
      services.AddMvc();
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
      });
      var connectionString = Configuration.GetConnectionString("DefaultConnection");

      services.AddTransient<ILoginService<ApplicationUser>, LoginService>();

      services.AddIdentityServer()
          .AddTemporarySigningCredential()
          .AddInMemoryApiResources(Config.GetApiResources())
          .AddInMemoryIdentityResources(Config.GetIdentityResources())
          .AddInMemoryClients(Config.GetClients())
          .AddAspNetIdentity<ApplicationUser>()
          .AddResourceOwnerValidator<UserValidator>()
          .Services.AddTransient<IProfileService, ProfileService>();
          
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();

      app.UseCors("CorsPolicy");

      app.UseDeveloperExceptionPage();

      app.UseIdentity();
      app.UseIdentityServer();
      // app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
      // {
      //   Authority = "http://localhost:5000",
      //   RequireHttpsMetadata = false,
      //   ApiName = "api1"
      // });
      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
      });

      app.UseMvcWithDefaultRoute();

      //Seed Data
      var hasher = new PasswordHasher<ApplicationUser>();
      new ApplicationContextSeed(hasher).SeedAsync(app, loggerFactory)
      .Wait();
    }
  }
}
