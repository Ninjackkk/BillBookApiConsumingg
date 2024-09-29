namespace BillBookApiConsuming.Models
{
    public class InvoiceDetails
    {
        public PurchaseOrder PurchaseOrder { get; set; }
        public List<MyStock> MyStocks { get; set; } = new List<MyStock>();
    }
}
