using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class VehicleImage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public required string ImageLink { get; set; } = string.Empty;

        [Required]
        public required Guid VehicleId { get; set; }


        [ForeignKey(nameof(VehicleId))]
        [JsonIgnore]
        public Vehicle Vehicle { get; set; } = null!;
    }
}