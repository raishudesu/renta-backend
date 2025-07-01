using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace backend.Models
{


    public class Payment
    {
        [Key]
        // [JsonIgnore]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public required int MediumTypeId { get; set; }

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

        [ForeignKey(nameof(MediumTypeId))]
        [JsonIgnore]
        public MediumType MediumType { get; set; } = null!;
    }
}