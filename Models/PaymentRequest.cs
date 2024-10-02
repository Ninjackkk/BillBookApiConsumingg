namespace BillBookApiConsuming.Models
{
    public class PaymentRequest
    {
        public int PurchaseOrderId { get; set; }
        public string PaymentMethod { get; set; }
    }
}
