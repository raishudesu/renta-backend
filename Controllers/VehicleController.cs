
using backend.DTOs.VehicleDto;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Amazon.S3;

[ApiController]
[Route("api/[controller]")]

public class VehicleController(VehicleService vehicleService, VehicleImageService vehicleImageService, IAmazonS3 s3Client) : ControllerBase
{
    private readonly VehicleService _vehicleService = vehicleService;
    private readonly VehicleImageService _vehicleImageService = vehicleImageService;

    private readonly IAmazonS3 _s3Client = s3Client;

    [HttpGet]
    [Authorize(Roles = nameof(RoleTypes.Admin))]
    public async Task<ActionResult<List<VehicleWithImageUrlDto>>> GetAll()
    {
        var vehicles = await _vehicleService.GetVehicles();

        var vehiclesWithImages = new List<VehicleWithImageUrlDto>();

        foreach (var vehicle in vehicles)
        {
            var vehicleImages = await _vehicleImageService.GetVehicleImagesByVehicleId(vehicle.Id);

            var imageUrls = vehicleImages.Select(img =>
                _s3Client.GetPreSignedURL(new Amazon.S3.Model.GetPreSignedUrlRequest
                {
                    BucketName = img.S3BucketName,
                    Key = img.S3ObjectKey,
                    Expires = DateTime.UtcNow.AddHours(1),
                    Protocol = Protocol.HTTPS
                })
            ).ToList();

            vehiclesWithImages.Add(new VehicleWithImageUrlDto
            {
                Id = vehicle.Id,
                ModelName = vehicle.ModelName,
                Type = vehicle.Type,
                Color = vehicle.Color,
                PlateNumber = vehicle.PlateNumber,
                Description = vehicle.Description,
                OwnerId = vehicle.OwnerId,
                ImagePreSignedUrl = imageUrls[0] // temporarily using the first image URL, can be modified to support multiple images
            });
        }

        return Ok(vehiclesWithImages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleWithImageUrlDto>> GetById(Guid id)
    {
        var vehicle = await _vehicleService.GetVehicleById(id);

        if (vehicle == null) return NotFound();

        var vehicleImage = await _vehicleImageService.GetVehicleImagesByVehicleId(id);

        var preSignedUrl = _s3Client.GetPreSignedURL(new Amazon.S3.Model.GetPreSignedUrlRequest
        {
            BucketName = vehicleImage.FirstOrDefault()?.S3BucketName ?? string.Empty,
            Key = vehicleImage.FirstOrDefault()?.S3ObjectKey ?? string.Empty,
            Expires = DateTime.UtcNow.AddHours(1),
            Protocol = Protocol.HTTPS
        }) ?? null;


        var vehicleWithImage = new VehicleWithImageUrlDto // this can be <List<VehicleWithImageUrlDto>> to support multiple images
        {
            Id = vehicle.Id,
            ModelName = vehicle.ModelName,
            Type = vehicle.Type,
            Color = vehicle.Color,
            PlateNumber = vehicle.PlateNumber,
            Description = vehicle.Description,
            OwnerId = vehicle.OwnerId,
            ImagePreSignedUrl = preSignedUrl
        };

        return Ok(vehicleWithImage);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<Vehicle>>> GetByUserId(string userId)
    {
        var vehicles = await _vehicleService.GetVehiclesByUserId(userId);

        var vehiclesWithImages = new List<VehicleWithImageUrlDto>();

        foreach (var vehicle in vehicles)
        {
            var vehicleImages = await _vehicleImageService.GetVehicleImagesByVehicleId(vehicle.Id);

            var imageUrls = vehicleImages.Select(img =>
                _s3Client.GetPreSignedURL(new Amazon.S3.Model.GetPreSignedUrlRequest
                {
                    BucketName = img.S3BucketName,
                    Key = img.S3ObjectKey,
                    Expires = DateTime.UtcNow.AddHours(1),
                    Protocol = Protocol.HTTPS
                })
            ).ToList();

            vehiclesWithImages.Add(new VehicleWithImageUrlDto
            {
                Id = vehicle.Id,
                ModelName = vehicle.ModelName,
                Type = vehicle.Type,
                Color = vehicle.Color,
                PlateNumber = vehicle.PlateNumber,
                Description = vehicle.Description,
                OwnerId = vehicle.OwnerId,
                ImagePreSignedUrl = imageUrls[0] // temporarily using the first image URL, can be modified to support multiple images
            });
        }

        return Ok(vehiclesWithImages);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<ActionResult<Vehicle>> Create([FromForm] VehicleDto vehicle)
    {


        var vehicleToCreate = new Vehicle
        {
            Color = vehicle.Color,
            ModelName = vehicle.ModelName,
            Type = vehicle.Type,
            Description = vehicle.Description,
            PlateNumber = vehicle.PlateNumber,
            OwnerId = vehicle.OwnerId
        };

        var createdVehicle = await _vehicleService.CreateVehicle(vehicleToCreate);


        // Handle image upload if provided
        if (vehicle.VehicleImageFile != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await vehicle.VehicleImageFile.CopyToAsync(memoryStream);
                var uploadRequest = new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = "renta-s3", // Replace with your S3 bucket name
                    Key = $"vehicle-images/{createdVehicle.Id}/{vehicle.VehicleImageFile.FileName}",
                    InputStream = memoryStream,
                    ContentType = vehicle.VehicleImageFile.ContentType
                };

                await _s3Client.PutObjectAsync(uploadRequest);
            }

            var vehicleImage = new VehicleImage
            {
                VehicleId = createdVehicle.Id,
                S3BucketName = "renta-s3",
                S3ObjectKey = $"vehicle-images/{createdVehicle.Id}/{vehicle.VehicleImageFile.FileName}"
            };

            // Save the vehicle image details to the database
            await _vehicleImageService.CreateVehicleImage(vehicleImage);
        }

        return CreatedAtAction(nameof(GetById), new { id = createdVehicle.Id }, vehicleToCreate);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<IActionResult> Update(Guid id, Vehicle vehicle)
    {
        if (id != vehicle.Id) return BadRequest();

        // Get the current user's id from the claims
        var userId = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "id")?.Value;
        if (userId == null || userId != vehicle.OwnerId)
        {
            return Forbid();
        }

        await _vehicleService.UpdateVehicle(vehicle);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var vehicle = await _vehicleService.GetVehicleById(id);
        if (vehicle == null) return NotFound();

        // Get the current user's id from the claims
        var userId = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "id")?.Value;
        if (userId == null || userId != vehicle.OwnerId)
        {
            return Forbid();
        }

        await _vehicleService.DeleteVehicleById(id);

        return NoContent();
    }
}