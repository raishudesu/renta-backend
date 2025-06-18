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
        public required Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required DateTime StartTime { get; set; }
        [Required]
        public required DateTime EndTime { get; set; }

        [Required]
        public required string BookerName { get; set; } = string.Empty;

        // VALIDATE THIS IN FRONTEND
        // EITHER OF EMAIL OR PHONE SHOULD BE AVAILABLE, BOTH SHOULDN'T BE UNAVAILABLE
        public string BookerEmail { get; set; } = string.Empty;

        public string BookerPhone { get; set; } = string.Empty;

        [Required]
        public required string UserId { get; set; } = null!;
        [Required]
        public required Guid VehicleId { get; set; }

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