using System.Security.Claims;
using ExternalLoginsApp.Dtos;
using ExternalLoginsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExternalLoginsApp.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    } 

    [HttpGet("userinfo")]
    [Authorize]
    public async Task<IActionResult> UserInfo()
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        return Ok(user?.Email);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto newUser)
    {
        var user = new User
        {
            Age = newUser.Age,
            UserName = newUser.Username,
        }; 

        var result = await _userManager.CreateAsync(user, newUser.Password);
        if(result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(result.Errors);
    }
    [HttpGet("loginproviders")]
    public async Task<IActionResult> GetProviders()
    {
        var providers = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        List<string?> p = providers.Select(p => p.DisplayName).ToList();
        return Ok(p);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]UserLogin userLogin)
    {
        return Ok();
    } 

    [HttpGet("externallogin/{returnUrl}")]
    public async Task<IActionResult> ExternalLogin([FromQuery(Name = "provider")]string providerName, [FromRoute]string returnUrl)
    {
        var providers = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if(!providers.Any(p => p.DisplayName == providerName))
        {
            return BadRequest();
        } 

        var redirectUr = Url.Action("ExternalLoginCallback", "Auth", new { ReturnUrl = returnUrl});
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(providerName, redirectUr);
        return new ChallengeResult(providerName, properties);
    }

    [HttpGet("ExternalLoginCallback/{returnUrl}/{remoteError}")]
    public async Task<IActionResult> ExternalLoginCallback([FromRoute]string? returnUrl, [FromRoute]string? remoteError)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if(info == null)
        {
            return BadRequest();
        } 

        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
        if(signInResult.Succeeded)
        {
            return Ok(new { status="signed in successfully" });
        }
        else
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if(email is not null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if(user is null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email
                    };

                    var result = await _userManager.CreateAsync(user);
                    if(!result.Succeeded)
                    {
                        return BadRequest();
                    }
                }

                await _userManager.AddLoginAsync(user!, info);
                await _signInManager.SignInAsync(user, false);
                return Ok(new { message = "Succesfully created" });
            }
        }
        return Ok(info.ProviderKey);
    }

}