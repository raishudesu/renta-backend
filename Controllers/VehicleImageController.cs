using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class VehicleImageController(VehicleImageService vehicleImageService) : ControllerBase
{
    private readonly VehicleImageService _vehicleImageService = vehicleImageService;

    [HttpGet("{vehicleId}")]
    public async Task<ActionResult<List<VehicleImage>>> GetVehicleImagesByVehicleId(Guid vehicleId)
    {
        var images = await _vehicleImageService.GetVehicleImagesByVehicleId(vehicleId);

        return Ok(images);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleImage>> UploadImage(VehicleImage vehicleImage)
    {
        var createdImage = await _vehicleImageService.CreateVehicleImage(vehicleImage);

        return CreatedAtAction(nameof(GetVehicleImagesByVehicleId), new { vehicleId = createdImage.VehicleId }, createdImage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        var image = await _vehicleImageService.GetVehicleImageById(id);

        if (image == null) return NotFound();

        await _vehicleImageService.DeleteVehicleImageById(id);

        return NoContent();
    }
}