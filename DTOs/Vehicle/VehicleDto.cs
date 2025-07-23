using backend.Models;


namespace backend.DTOs.Vehicle
{
    public class VehicleDto
    {
        public required string ModelName { get; init; }

        public required VehicleType Type { get; init; } = VehicleType.Car;
        public required string Color { get; init; }
        public required string PlateNumber { get; init; }
        public string Description { get; init; } = string.Empty;
        public required string OwnerId { get; init; }
    }
}