using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
    public class User : IdentityUser
    {


        public string BusinessCoordinatesString { get; set; } = string.Empty;

        public virtual ICollection<Booking> UserBookings { get; set; } = new List<Booking>();

        public virtual ICollection<Vehicle> UserVehicles { get; set; } = new List<Vehicle>();

        public virtual ICollection<Subscription> UserSubscriptions { get; set; } = new List<Subscription>();
    }
}