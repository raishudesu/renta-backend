using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace backend.Models
{
    public class SubscriptionTier
    {
        [Key]
        // [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string TierName { get; set; } = string.Empty;

        [Required]
        public required string TierDescription { get; set; } = string.Empty;

        [Required]
        [Precision(18, 2)]
        public required decimal Price { get; set; }
    }
}