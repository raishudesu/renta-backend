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

        public string Description { get; set; } = string.Empty;

        [Required]
        public required string OwnerId { get; set; } = null!;

        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for the owner
        [ForeignKey(nameof(OwnerId))]
        [JsonIgnore]
        public User Owner { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<Booking> VehicleBookingRecords { get; set; } = new List<Booking>();

        [JsonIgnore]
        public virtual ICollection<VehicleImage> VehicleImages { get; set; } = new List<VehicleImage>();
    }


    public class VehicleParameters
    {
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;


        public VehicleType? Type { get; set; }

        public string? ModelName { get; set; }

        private int _pageSize = 10;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        // For nearest search
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? MaxDistanceKm { get; set; } // optional filter
    }

    public class Coordinates
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}