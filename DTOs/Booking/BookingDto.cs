using backend.DTOs.VehicleDto;
using backend.Models;

namespace backend.DTOs.BookingDto
{
    public class BookingDto
    {
        public required DateTime StartTime { get; init; }
        public required DateTime EndTime { get; init; }

        public required string BookerName { get; init; }

        public required string BookerEmail { get; init; }

        public required string BookerPhone { get; init; }

        public required string UserId { get; init; }
        public required Guid VehicleId { get; init; }

    }

    public class BookingWithVehicleDto : Booking
    {
        public required VehicleDetailDto VehicleDetails { get; init; }
    }

}