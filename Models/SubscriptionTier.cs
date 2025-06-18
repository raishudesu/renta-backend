using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class SubscriptionTier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TierName { get; set; } = string.Empty;

        [Required]
        public float Price { get; set; } = 0;
    }
}