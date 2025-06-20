using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace backend.Models
{

    public enum MediumType
    {
        Ewallet,
        Bank,

        Cash
    }

    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public required MediumType MediumType { get; set; } = MediumType.Cash;

        // VALIDATE THIS IN FRONTEND
        // IF MEDIUM TYPE IS NOT CASH, PROVIDER NAME SHOULD BE REQUIRED
        public string ProviderName { get; set; } = string.Empty;

        [Required]
        [Precision(18, 2)]
        public required decimal Amount { get; set; }

        // VALIDATE THIS IN FRONTEND
        // Either Receipt or Transaction should be present, but both shouldn't be unavailable
        public string ReceiptImageLink { get; set; } = string.Empty;

        public string TransactionId { get; set; } = string.Empty;
    }
}