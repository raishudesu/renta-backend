
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class VehicleController(VehicleService vehicleService) : ControllerBase
{
    private readonly VehicleService _vehicleService = vehicleService;

    [HttpGet]
    public async Task<ActionResult<List<Vehicle>>> GetAll()
    {
        var vehicles = await _vehicleService.GetVehicles();

        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Vehicle>> GetById(Guid id)
    {
        var vehicle = await _vehicleService.GetVehicleById(id);

        return vehicle != null ? Ok(vehicle) : NotFound();
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<Vehicle>>> GetByUserId(string userId)
    {
        var vehicles = await _vehicleService.GetVehiclesByUserId(userId);

        return Ok(vehicles);
    }

    [HttpPost]
    public async Task<ActionResult<Vehicle>> Create(Vehicle vehicle)
    {
        var createdVehicle = await _vehicleService.CreateVehicle(vehicle);

        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Vehicle vehicle)
    {
        if (id != vehicle.Id) return BadRequest();

        await _vehicleService.UpdateVehicle(vehicle);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _vehicleService.DeleteVehicleById(id);

        return NoContent();
    }
}