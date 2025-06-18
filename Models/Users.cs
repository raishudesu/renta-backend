using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class User : IdentityUser
    {


        public virtual ICollection<Booking> UserBookings { get; set; } = new List<Booking>();

        public virtual ICollection<Vehicle> UserVehicles { get; set; } = new List<Vehicle>();

        public virtual ICollection<Subscription> UserSubscriptions { get; set; } = new List<Subscription>();
    }
}