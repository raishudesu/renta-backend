using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    // Subscription model to represent a rental business owner subscription

    public class Subscription
    {
        [Key]
        // [JsonIgnore]
        public Guid Id { get; set; } = Guid.NewGuid();
        // [Required]
        // DURATION WILL BE ASSIGNED/UPDATED ONCE THE SUBSCRIPTION IS VERIFIED
        [JsonIgnore]
        public DateTime? StartTime { get; set; }
        // [Required]
        [JsonIgnore]
        public DateTime? EndTime { get; set; }

        [Required]
        public required string UserId { get; set; } = null!;

        // [Required]
        public Guid? PaymentId { get; set; }

        [Required]
        public required int SubscriptionTierId { get; set; }

        public bool IsVerified { get; set; } = false;


        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(PaymentId))]
        // [JsonIgnore]
        public Payment? PaymentDetails { get; set; }

        [ForeignKey(nameof(SubscriptionTierId))]
        // [JsonIgnore]
        public SubscriptionTier SubscriptionTier { get; set; } = null!;


    }
}