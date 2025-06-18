using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    // Subscription model to represent a rental business owner subscription

    public class Subscription
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required DateTime StartTime { get; set; }
        [Required]
        public required DateTime EndTime { get; set; }

        [Required]
        public required string UserId { get; set; } = null!;

        [Required]
        public required Guid PaymentId { get; set; }

        [Required]
        public required int SubscriptionTierId { get; set; }


        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(PaymentId))]
        [JsonIgnore]
        public Payment PaymentDetails { get; set; } = null!;

        [ForeignKey(nameof(SubscriptionTierId))]
        [JsonIgnore]
        public SubscriptionTier SubscriptionTier { get; set; } = null!;


    }
}