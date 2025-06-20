using backend.DTOs.Payment;

namespace backend.DTOs.Subscription
{
    public class CreateSubscriptionWithPaymentDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string UserId { get; set; } = default!;
        public int SubscriptionTierId { get; set; }
        public CreatePaymentDto Payment { get; set; } = default!;
    }
}
