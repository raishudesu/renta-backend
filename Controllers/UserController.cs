using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using backend.DTOs.User;

[ApiController]
[Route("api/[controller]")]



public class UserController : ControllerBase
{

    private readonly UserService _userService;
    private readonly VehicleService _vehicleService;
    private readonly BookingService _bookingService;

    public UserController(UserService userService, VehicleService vehicleService, BookingService bookingService)
    {
        _userService = userService;
        _vehicleService = vehicleService;
        _bookingService = bookingService;
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

        var totalVehicles = await _vehicleService.GetTotalVehiclesByUserId(id);
        var totalBookings = await _bookingService.GetTotalBookingsByUserId(id);

        var userStats = new UserStatsDto
        {
            TotalVehicles = totalVehicles,
            TotalBookings = totalBookings
        };

        return Ok(userStats);
    }

    [HttpPatch("{id}/update-business-coordinates")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<ActionResult> UpdateBusinessCoordinates(string id, [FromBody] UpdateBusinessCoordinatesDto data)
    {

        if (id == null) return BadRequest();

        await _userService.UpdateUserBusinessCoordinates(id, data);

        return NoContent();
    }

    // [HttpPut("{id}")]
    // public async Task<IActionResult> Update(string id, User user)
    // {
    //     if (id != user.Id) return BadRequest();

    //     var existingUser = await _userService.GetUserById(id);
    //     if (existingUser == null) return NotFound();

    //     existingUser.UserName = user.UserName;
    //     existingUser.Email = user.Email;

    //     await _userService.UpdateUser(existingUser);

    //     return NoContent();
    // }

}