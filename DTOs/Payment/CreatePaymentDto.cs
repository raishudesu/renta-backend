using backend.Models;

namespace backend.DTOs.Payment
{
    public class CreatePaymentDto
    {
        public int MediumTypeId { get; set; }
        public string ProviderName { get; set; } = default!;
        public string ReceiptImageLink { get; set; } = default!;
        public string TransactionId { get; set; } = default!;
    }
}