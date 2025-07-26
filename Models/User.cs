using Microsoft.AspNetCore.Identity;

namespace backend.Models
{

    public enum RoleTypes
    {
        User,
        Admin
    }

    public class User : IdentityUser
    {

        public string BusinessCoordinatesString { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public virtual ICollection<Booking> UserBookings { get; set; } = new List<Booking>();

        public virtual ICollection<Vehicle> UserVehicles { get; set; } = new List<Vehicle>();

        public virtual ICollection<Subscription> UserSubscriptions { get; set; } = new List<Subscription>();
    }
}