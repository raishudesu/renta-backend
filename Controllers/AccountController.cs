using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]/roles")]

public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;


    public AccountController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    [HttpPost("add")]
    [EnableRateLimiting("UserAwarePolicy")]
    [Authorize(Roles = nameof(RoleTypes.Admin))]
    public async Task<IActionResult> AddRole([FromBody] string role)
    {

        var roleExist = await _roleManager.RoleExistsAsync(role);
        if (!roleExist)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest("Failed to create role");
        }
        return BadRequest("Role already exists");
    }

    [HttpPost("add-to-user")]
    [EnableRateLimiting("UserAwarePolicy")]
    [Authorize(Roles = nameof(RoleTypes.Admin))]

    public async Task<IActionResult> AddToUser([FromBody] string role, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user != null)
        {
            // var roles = await _userManager.GetRolesAsync(user);
            // var result = await _userManager.RemoveFromRolesAsync(user, roles);
            // if (result.Succeeded)
            // {
            //     var roleExist = await _roleManager.RoleExistsAsync(role);
            //     if (roleExist)
            //     {
            //         var roleResult = await _userManager.AddToRoleAsync(user, role);
            //         if (roleResult.Succeeded)
            //         {
            //             return Ok();
            //         }
            //         return BadRequest("Failed to add role");
            //     }
            //     return BadRequest("Role does not exist");
            // }
            // return BadRequest("Failed to remove roles");

            var roleExist = await _roleManager.RoleExistsAsync(role);

            if (roleExist)
            {
                var result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    return Ok($"User added as {role} successfully");
                }
            }
            return BadRequest("Role does not exist");
        }
        return BadRequest("User does not exist");
    }

}