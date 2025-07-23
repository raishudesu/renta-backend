
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
}