using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("[controller]")]



public class UserController : ControllerBase
{
    public enum RoleTypes
    {
        User,
        Admin
    }

    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = nameof(RoleTypes.Admin))]
    public async Task<ActionResult<List<User>>> GetAll()
    {
        var users = await _userService.GetUsers();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(string id)
    {
        var user = await _userService.GetUserById(id);
        return user != null ? Ok(user) : NotFound();
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