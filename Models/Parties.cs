using System.ComponentModel.DataAnnotations;

namespace BillBookApiConsuming.Models
{
    public class Parties
    {
        [Key]
        public int PartyId { get; set; }
        public string PartyName { get; set; }
        public string Email { get; set; }
        public double? PhoneNumber { get; set; }
        public double? OpeningBalance { get; set; }
        public string? GSTINNumber { get; set; }
        public string? PanCardNumber { get; set; }
        public string? PartyType { get; set; }
        public string? PartyCategory { get; set; }
        public string? BillingAddress { get; set; }
        public string? ShippingAddress { get; set; }
        public double? CreditPeriod { get; set; }
        public double? CreditLimit { get; set; }
    }
}
