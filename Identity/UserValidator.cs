using IdentityServer4.Validation;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AspNetAuth.Models;
using System.Security.Claims;
using System.Collections.Generic;

namespace AspNetAuth.Identity
{
  public class UserValidator : IResourceOwnerPasswordValidator
  {
    private readonly ILogger _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    public UserValidator(
      UserManager<ApplicationUser> userManager,
      ILoggerFactory loggerFactory)
    {
      _userManager = userManager;
      _logger = loggerFactory.CreateLogger<UserValidator>();
    }
    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
      var user = await _userManager.FindByEmailAsync(context.UserName);
      if (await _userManager.CheckPasswordAsync(user, context.Password))
      {
        var claims = new List<Claim>();
        claims.Add(new Claim("openId", user.OpenId));
        _logger.LogCritical("claims", claims);
        context.Result = new GrantValidationResult(subject: user.Id, authenticationMethod: "custom", claims: claims);
      }
      else
      {
        //验证失败
        context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid custom credential");
      }
    }
  }
}
