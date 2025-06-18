using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }

    public class Booking
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public string BookerName { get; set; } = string.Empty;

        // VALIDATE THIS IN FRONTEND
        // EITHER OF EMAIL OR PHONE SHOULD BE AVAILABLE, BOTH SHOULDN'T BE UNAVAILABLE
        public string BookerEmail { get; set; } = string.Empty;

        public string BookerPhone { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        public Guid VehicleId { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        // Navigation properties
        public User User { get; set; } = null!;
        [ForeignKey(nameof(VehicleId))]
        [JsonIgnore]
        public Vehicle Vehicle { get; set; } = null!;
    }
}