
using backend.DTOs.VehicleDto;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Amazon.S3;
using System.Security.Claims;
using Newtonsoft.Json;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]

public class VehicleController(VehicleService vehicleService, VehicleImageService vehicleImageService, IAmazonS3 s3Client, ILogger<VehicleController> logger) : ControllerBase
{
    private readonly VehicleService _vehicleService = vehicleService;
    private readonly VehicleImageService _vehicleImageService = vehicleImageService;

    private readonly IAmazonS3 _s3Client = s3Client;

    private readonly ILogger _logger = logger;

    [HttpGet]
    [EnableRateLimiting("UserAwarePolicy")]
    // [Authorize(Roles = nameof(RoleTypes.Admin))]
    public async Task<ActionResult<List<VehicleWithOwnerDto>>> GetAll([FromQuery] VehicleParameters vehicleParameters)
    {
        var vehicles = await _vehicleService.GetVehicles(vehicleParameters);

        var vehiclesWithOwner = new List<VehicleWithOwnerDto>();

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

            var ownerName = new OwnerNameDto
            {
                FirstName = vehicle.Owner.FirstName,
                LastName = vehicle.Owner.LastName,

            };

            vehiclesWithOwner.Add(new VehicleWithOwnerDto
            {
                Id = vehicle.Id,
                ModelName = vehicle.ModelName,
                Type = vehicle.Type,
                Color = vehicle.Color,
                PlateNumber = vehicle.PlateNumber,
                Description = vehicle.Description,
                OwnerId = vehicle.OwnerId,
                OwnerName = ownerName,
                BusinessCoordinates = vehicle.Owner.BusinessCoordinatesString ?? null, // assuming this is the correct property for business coordinates
                ImagePreSignedUrl = imageUrls.Count > 0 ? imageUrls[0] : null // safely get the first image URL or null if none exist
            });
        }

        var metadata = new
        {
            vehicles.TotalCount,
            vehicles.PageSize,
            vehicles.CurrentPage,
            vehicles.TotalPages,
            vehicles.HasNext,
            vehicles.HasPrevious
        };
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));


        return Ok(vehiclesWithOwner);
    }

    [HttpGet("{id}")]
    [EnableRateLimiting("ConcurrencyPolicy")]
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
            ImagePreSignedUrl = preSignedUrl ?? null
        };

        return Ok(vehicleWithImage);
    }

    [HttpGet("user/{userId}")]
    [EnableRateLimiting("ConcurrencyPolicy")]

    public async Task<ActionResult<List<VehicleWithImageUrlDto>>> GetByUserId(string userId)
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
                ImagePreSignedUrl = imageUrls.Count > 0 ? imageUrls[0] : null // safely get the first image URL or null if none exist

            });
        }

        return Ok(vehiclesWithImages);
    }

    [HttpPost]
    [EnableRateLimiting("ConcurrencyPolicy")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<ActionResult<Vehicle>> Create([FromForm] VehicleDto vehicle)
    {


        var vehicleToCreate = new Vehicle
        {
            Color = vehicle.Color,
            ModelName = vehicle.ModelName,
            Type = vehicle.Type ?? VehicleType.Car, // Default to Car if Type is null
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

    [HttpPatch("{id}")]
    [EnableRateLimiting("ConcurrencyPolicy")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<IActionResult> Update(Guid id, [FromForm] VehicleDto vehicle)
    {
        var existingVehicle = await _vehicleService.GetVehicleById(id);
        if (existingVehicle == null) return NotFound();


        // Get the current user's id from the claims
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || userId != vehicle.OwnerId)
        {
            return Forbid();
        }

        var vehicleImages = await _vehicleImageService.GetVehicleImagesByVehicleId(id);

        if (vehicle.VehicleImageFile != null)
        {
            // Delete existing images from S3
            // there could be a better way such as check first if the current image is the same as the new one
            // and only delete if they are different
            foreach (var image in vehicleImages)
            {
                var deleteRequest = new Amazon.S3.Model.DeleteObjectRequest
                {
                    BucketName = image.S3BucketName,
                    Key = image.S3ObjectKey
                };
                var response = await _s3Client.DeleteObjectAsync(deleteRequest);

                await _vehicleImageService.DeleteVehicleImageById(image.Id);
            }

            using (var memoryStream = new MemoryStream())
            {
                await vehicle.VehicleImageFile.CopyToAsync(memoryStream);
                var uploadRequest = new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = "renta-s3", // Replace with your S3 bucket name
                    Key = $"vehicle-images/{id}/{vehicle.VehicleImageFile.FileName}",
                    InputStream = memoryStream,
                    ContentType = vehicle.VehicleImageFile.ContentType
                };

                await _s3Client.PutObjectAsync(uploadRequest);
            }

            var vehicleImage = new VehicleImage
            {
                VehicleId = id,
                S3BucketName = "renta-s3",
                S3ObjectKey = $"vehicle-images/{id}/{vehicle.VehicleImageFile.FileName}"
            };

            // Save the vehicle image details to the database
            await _vehicleImageService.CreateVehicleImage(vehicleImage);
        }

        // var updatedVehicle = new Vehicle
        // {
        //     Id = id,
        //     ModelName = vehicle.ModelName ?? existingVehicle.ModelName,
        //     Type = vehicle.Type ?? existingVehicle.Type,
        //     Color = vehicle.Color ?? existingVehicle.Color,
        //     PlateNumber = vehicle.PlateNumber ?? existingVehicle.PlateNumber,
        //     Description = vehicle.Description ?? existingVehicle.Description,
        //     OwnerId = existingVehicle.OwnerId
        // };
        // Update the existing tracked entity instead of creating a new one
        existingVehicle.ModelName = vehicle.ModelName ?? existingVehicle.ModelName;
        existingVehicle.Type = vehicle.Type ?? existingVehicle.Type;
        existingVehicle.Color = vehicle.Color ?? existingVehicle.Color;
        existingVehicle.PlateNumber = vehicle.PlateNumber ?? existingVehicle.PlateNumber;
        existingVehicle.Description = vehicle.Description ?? existingVehicle.Description;
        // Don't update OwnerId as it should remain the same

        await _vehicleService.UpdateVehicle(existingVehicle);

        return Ok(existingVehicle);
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("ConcurrencyPolicy")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var vehicle = await _vehicleService.GetVehicleById(id);
        if (vehicle == null) return NotFound();

        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || userId != vehicle.OwnerId)
        {
            return Forbid();
        }

        var vehicleImages = await _vehicleImageService.GetVehicleImagesByVehicleId(id);

        var s3Results = new List<object>();

        foreach (var image in vehicleImages)
        {
            try
            {
                var deleteRequest = new Amazon.S3.Model.DeleteObjectRequest
                {
                    BucketName = image.S3BucketName,
                    Key = image.S3ObjectKey
                };

                var response = await _s3Client.DeleteObjectAsync(deleteRequest);

                s3Results.Add(new
                {
                    ImageId = image.Id,
                    Bucket = image.S3BucketName,
                    Key = image.S3ObjectKey,
                    Success = true,
                    HttpStatusCode = response.HttpStatusCode,
                    ResponseMessage = "S3 deletion successful"
                });
            }
            catch (Exception ex)
            {
                s3Results.Add(new
                {
                    ImageId = image.Id,
                    Bucket = image.S3BucketName,
                    Key = image.S3ObjectKey,
                    Success = false,
                    Error = ex.Message,
                    ExceptionType = ex.GetType().Name,
                    InnerException = ex.InnerException?.Message
                });
            }

            await _vehicleImageService.DeleteVehicleImageById(image.Id);
        }

        await _vehicleService.DeleteVehicleById(id);

        return Ok(new
        {
            Message = "Vehicle deleted",
            S3Results = s3Results
        });
    }
}