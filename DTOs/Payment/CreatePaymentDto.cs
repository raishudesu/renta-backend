using backend.Models;

namespace backend.DTOs.Payment
{
    public class CreatePaymentDto
    {
        public MediumType MediumType { get; set; }
        public string ProviderName { get; set; } = default!;
        public string ReceiptImageLink { get; set; } = default!;
        public string TransactionId { get; set; } = default!;
    }
}