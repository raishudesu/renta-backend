using backend.Models;


namespace backend.DTOs.VehicleDto
{
    public class VehicleDto
    {
        public required string ModelName { get; init; }

        public required VehicleType? Type { get; init; }

        public IFormFile? VehicleImageFile { get; init; }

        public required string Color { get; init; }
        public required string PlateNumber { get; init; }
        public string Description { get; init; } = string.Empty;
        public required string OwnerId { get; init; }

    }

    public class VehicleWithImageUrlDto : Vehicle
    {
        public required string? ImagePreSignedUrl { get; init; }
    }

    public class OwnerNameDto
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
    }

    public class VehicleWithOwnerNameDto : Vehicle
    {

        public required OwnerNameDto OwnerName { get; init; }
        public required string? ImagePreSignedUrl { get; set; }
    }
}