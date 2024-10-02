using System.ComponentModel.DataAnnotations;

namespace BillBookApiConsuming.Models
{
    public class Inventories
    {
        [Key]
        public int ItemId { get; set; }
        public bool ItemType { get; set; }
        public int CategoryID { get; set; }
        //public int? Quantity { get; set; }
        public string ItemName { get; set; }
        public double SalesPrice { get; set; }
        public string GSTTaxRate { get; set; }
        public string MeasuringUnit { get; set; }
        public double OpeningStock { get; set; }
        public double PurchasePrice { get; set; }
        public string ItemCode { get; set; }
        public string HSNCode { get; set; }
        public string Description { get; set; }
        public string? Image { get; set; }
        public int BusinessId { get; set; }
    }
}
