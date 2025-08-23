
namespace backend.DTOs.User
{
    public class UserRegistrationDto
    {
        public required string Email { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        // public required string PhoneNumber { get; init; }
        public required string Password { get; init; }
    }

    public class UpdateBusinessCoordinatesDto
    {
        // public required string Id { get; init; }
        public required string BusinessCoordinates { get; init; }
    }

    public class UserStatsDto
    {
        public int TotalVehicles { get; set; }
        public int TotalCompletedBookings { get; set; }
        public int TotalActiveBookings { get; set; }
    }

    public class UpdatePasswordDto
    {
        public required string CurrentPassword { get; init; }
        public required string NewPassword { get; init; }
    }
}