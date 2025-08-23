using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using backend.DTOs.User;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]



public class UserController : ControllerBase
{

    private readonly UserService _userService;
    private readonly VehicleService _vehicleService;
    private readonly BookingService _bookingService;

    private readonly UserManager<User> _userManager;

    private readonly SignInManager<User> _signInManager;

    public UserController(UserService userService, VehicleService vehicleService, BookingService bookingService, UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userService = userService;
        _vehicleService = vehicleService;
        _bookingService = bookingService;
        _userManager = userManager;
        _signInManager = signInManager;

    }

    [HttpGet]
    [Authorize(Roles = nameof(RoleTypes.Admin))]
    public async Task<ActionResult<List<User>>> GetAll()
    {
        var users = await _userService.GetUsers();

        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<ActionResult<User>> GetById(string id)
    {
        var user = await _userService.GetUserById(id);
        return user != null ? Ok(user) : NotFound();
    }

    [HttpGet("{id}/stats")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<ActionResult<UserStatsDto>> GetUserStats(string id)
    {
        var stats = await _userService.GetUserStats(id);

        return Ok(stats);
    }

    [HttpPatch("{id}/update-business-coordinates")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<ActionResult> UpdateBusinessCoordinates(string id, [FromBody] UpdateBusinessCoordinatesDto data)
    {

        if (id == null) return BadRequest();

        await _userService.UpdateUserBusinessCoordinates(id, data);

        return NoContent();
    }

    [HttpPatch("{id}/update-password")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<ActionResult> UpdatePassword(string id, [FromBody] UpdatePasswordDto data)
    {
        // if (id == null) return BadRequest();

        if (data.CurrentPassword == data.NewPassword)
        {
            return BadRequest("New password must be different from the current password.");
        }

        var userIdFromClaims = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userIdFromClaims == null || userIdFromClaims != id)
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var passwordComparisonResult = await _signInManager.CheckPasswordSignInAsync(user, data.CurrentPassword, lockoutOnFailure: false);

        if (!passwordComparisonResult.Succeeded)
        {
            return BadRequest(new { message = "Current password is incorrect." });
        }

        await _userService.UpdateUserPassword(id, data.NewPassword);

        return NoContent();
    }
}