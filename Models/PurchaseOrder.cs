using System.ComponentModel.DataAnnotations;

namespace BillBookApiConsuming.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int? PurchaseOrderId { get; set; } = 0;

        public int? BusinessId { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? PartyName { get; set; }
        public double? Amount { get; set; }
        public DateTime? ValidTill { get; set; }
        public string? Status { get; set; }

    }
}
