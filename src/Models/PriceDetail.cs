using System.ComponentModel.DataAnnotations;

namespace src.Models
{
    public class PriceInfo
    {
        [Key] public int PriceValueId { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        [Required] public string? CatalogEntryCode { get; set; }

        [Required] public string? MarketId { get; set; }

        [Required] public string? CurrencyCode { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime? ValidUntil { get; set; }

        public decimal UnitPrice { get; set; }
    }
}