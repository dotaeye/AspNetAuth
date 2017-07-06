
using Microsoft.AspNetCore.Builder;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using AspNetAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace AspNetAuth.Data
{
  public class ApplicationContextSeed
  {
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

    public ApplicationContextSeed(IPasswordHasher<ApplicationUser> passwordHasher)
    {
      _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(IApplicationBuilder applicationBuilder, ILoggerFactory loggerFactory, int? retry = 0)
    {
      int retryForAvaiability = retry.Value;
      try
      {
        var context = (ApplicationDbContext)applicationBuilder
            .ApplicationServices.GetService(typeof(ApplicationDbContext));

        context.Database.Migrate();

        if (!context.Users.Any())
        {
          context.Users.AddRange(
              GetDefaultUser());

          await context.SaveChangesAsync();
        }
      }
      catch (Exception ex)
      {
        if (retryForAvaiability < 10)
        {
          retryForAvaiability++;
          var log = loggerFactory.CreateLogger("catalog seed");
          log.LogError(ex.Message);
          await SeedAsync(applicationBuilder, loggerFactory, retryForAvaiability);
        }
      }
    }

    private ApplicationUser GetDefaultUser()
    {
      var user =
      new ApplicationUser()
      {
        Email = "123@qq.com",
        Id = Guid.NewGuid().ToString(),
        PhoneNumber = "1234567890",
        UserName = "123@qq.com",
        NormalizedEmail = "123@QQ.COM",
        NormalizedUserName = "123@QQ.COM",
        SecurityStamp = Guid.NewGuid().ToString("D"),
        OpenId = "123123"
      };

      user.PasswordHash = _passwordHasher.HashPassword(user, "Pass@word1");

      return user;
    }
  }
}
