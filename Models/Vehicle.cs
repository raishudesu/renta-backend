using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public enum VehicleType
    {
        Car,
        Motorcycle
    }

    public class Vehicle
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string ModelName { get; set; } = string.Empty;
        [Required]
        public required VehicleType Type { get; set; } = VehicleType.Car;
        [Required]
        public required string Color { get; set; } = string.Empty;
        [Required]
        public required string PlateNumber { get; set; } = string.Empty;

        [Required]
        public required string OwnerId { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(OwnerId))]
        [JsonIgnore]
        // Navigation property for the owner
        public User Owner { get; set; } = null!;


        public virtual ICollection<Booking> VehicleBookingRecords { get; set; } = new List<Booking>();

        public virtual ICollection<VehicleImage> VehicleImages { get; set; } = new List<VehicleImage>();
    }
}