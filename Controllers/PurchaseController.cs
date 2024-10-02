using Azure;
using BillBookApiConsuming.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
//using BillBookApiConsuming.Data;

namespace BillBookApiConsuming.Controllers
{
    public class PurchaseController : Controller
    {
        HttpClient client;                      //Declaring global object of HttpClient class 
        //private readonly ApplicationDbContext db;
        public PurchaseController(/*ApplicationDbContext db*/)
        {

            //this.db = db;

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            client = new HttpClient(clientHandler);        // loading the client handler into client object

        }

        public async Task<IActionResult> Index()
        {
            var categories = await GetAllCategoriesAsync();
            var parties = await GetAllPartiesAsync(); // Fetch parties
            ViewBag.Parties = parties; // Pass parties to the view

            return View(categories);
        }

        private async Task<List<Category>> GetAllCategoriesAsync()
        {
            string url = "https://localhost:44381/api/Purchase/GetAllCategories";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsondata = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Category>>(jsondata);
            }
            return new List<Category>(); // Return an empty list on failure
        }

        [HttpGet]
        public async Task<JsonResult> GetItemsByCategoryId(int categoryId)
        {
            string url = $"https://localhost:44381/api/Purchase/GetItemsByCategoryId/{categoryId}";
            var response = await client.GetAsync(url);
            List<Inventories> itemList = new List<Inventories>();

            if (response.IsSuccessStatusCode)
            {
                var jsondata = await response.Content.ReadAsStringAsync();
                itemList = JsonConvert.DeserializeObject<List<Inventories>>(jsondata);
            }
            return Json(itemList); // Return JSON data
        }

        [HttpPost]
        public async Task<IActionResult> NewPurchase(PurchaseRequest purchaseRequest)
        {
            if (purchaseRequest == null || purchaseRequest.Stocks == null)
            {
                return BadRequest("Invalid purchase request.");
            }

            // Filter stocks to include only those with a quantity greater than zero
            purchaseRequest.Stocks = purchaseRequest.Stocks
                .Where(stock => stock.Quantity > 0)
                .ToList();

            if (!purchaseRequest.Stocks.Any())
            {
                return BadRequest("No items selected for purchase.");
            }

            purchaseRequest.PurchaseOrder.PurchaseDate = DateTime.Now;
            purchaseRequest.PurchaseOrder.Status = "Unpaid";
            purchaseRequest.PurchaseOrder.BusinessId = 1001; // we have to take through session

            // API call to create a new purchase order
            using (var apiClient = new HttpClient())
            {
                var jsonRequest = JsonConvert.SerializeObject(purchaseRequest);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var result = await apiClient.PostAsync("https://localhost:44381/api/Purchase/NewPurchase", content);

                if (result.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Purchase order created successfully.";
                    return RedirectToAction("ListPurchaseOrders"); // Redirect to the ListPurchaseOrders action
                }

                return BadRequest("Error creating purchase order.");
            }
        }

        private async Task<List<Parties>> GetAllPartiesAsync()
        {
            string url = "https://localhost:44381/api/Purchase/GetAllParties";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsondata = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Parties>>(jsondata);
            }
            return new List<Parties>(); // Return an empty list on failure
        }

        //Will try dynamically showing quantity

        //[HttpGet]
        //public async Task<JsonResult> GetPartyStocks(int businessId, int purchaseOrderId)
        //{
        //    string url = $"https://localhost:44381/api/Purchase/GetPartyStocksByBusinessId/{businessId}/{purchaseOrderId}";
        //    var response = await client.GetAsync(url);
        //    List<MyStock> stockList = new List<MyStock>();

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var jsondata = await response.Content.ReadAsStringAsync();
        //        stockList = JsonConvert.DeserializeObject<List<MyStock>>(jsondata);
        //    }
        //    return Json(stockList); // Return JSON data
        //}


        public async Task<IActionResult> ListPurchaseOrders()
        {
            var apiClient = new HttpClient();
            var response = await apiClient.GetAsync("https://localhost:44381/api/Purchase/GetAllPurchaseOrders");

            if (response.IsSuccessStatusCode)
            {
                var jsondata = await response.Content.ReadAsStringAsync();
                var purchaseOrders = JsonConvert.DeserializeObject<List<PurchaseOrder>>(jsondata);
                return View(purchaseOrders);
            }

            return View(new List<PurchaseOrder>());
        }

        public async Task<IActionResult> ViewInvoice(int PurchaseOrderId)
        {
            // Call the API to get purchase order details by ID
            var response = await client.GetAsync($"https://localhost:44381/api/Purchase/GetAllPurchaseOrderById?PID={PurchaseOrderId}");

            if (response.IsSuccessStatusCode)
            {
                var jsondata = await response.Content.ReadAsStringAsync();
                var purchaseRequest = JsonConvert.DeserializeObject<PurchaseRequest>(jsondata);
                return View(purchaseRequest); // Return the view with purchase order and stocks
            }

            return NotFound(); // Handle the case when the purchase order is not found
        }












        public async Task<IActionResult> DownloadInvoice(int purchaseOrderId)
        {
            // Fetch purchase order details from the API
            var response = await client.GetAsync($"https://localhost:44381/api/Purchase/GetAllPurchaseOrderById?PID={purchaseOrderId}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound(); // Handle not found
            }

            var jsondata = await response.Content.ReadAsStringAsync();
            var purchaseRequest = JsonConvert.DeserializeObject<PurchaseRequest>(jsondata);

            using (var stream = new MemoryStream())
            {
                // Create PDF document
                Document document = new Document();
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Add content
                document.Add(new Paragraph($"Invoice for Purchase Order {purchaseRequest.PurchaseOrder.PurchaseOrderId}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20)));
                document.Add(new Paragraph($"Party Name: {purchaseRequest.PurchaseOrder.PartyName}"));
                document.Add(new Paragraph($"Purchase Date: {purchaseRequest.PurchaseOrder.PurchaseDate}"));
                document.Add(new Paragraph($"Status: {purchaseRequest.PurchaseOrder.Status}"));
                document.Add(new Paragraph($"Total Amount: {purchaseRequest.PurchaseOrder.Amount}"));
                document.Add(new Paragraph($"Valid Till: {purchaseRequest.PurchaseOrder.ValidTill}"));
                document.Add(new Paragraph("\nStock Details:"));

                // Create and populate the table
                var table = new PdfPTable(4);
                table.AddCell("Item Name");
                table.AddCell("Item Code");
                table.AddCell("Quantity");
                table.AddCell("Category");

                foreach (var stock in purchaseRequest.Stocks)
                {
                    table.AddCell(stock.ItemName);
                    table.AddCell(stock.ItemCode);
                    table.AddCell(stock.Quantity.ToString());
                    table.AddCell(stock.Category);
                }

                document.Add(table);
                document.Close();

                // Return the PDF file
                return File(stream.ToArray(), "application/pdf", $"Invoice_{purchaseOrderId}.pdf");
            }
        }


        // Doing Payment directly

        public IActionResult Payment(int purchaseOrderId)
        {
            ViewBag.PurchaseOrderId = purchaseOrderId;
            return View();
        }




        [HttpPost]
        public async Task<IActionResult> SubmitPayment(int purchaseOrderId, string paymentMethod)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                return BadRequest("Invalid payment method.");
            }

            // Create the payment request object
            var paymentRequest = new PaymentRequest
            {
                PurchaseOrderId = purchaseOrderId,
                PaymentMethod = paymentMethod
            };

            // API call to submit payment
            var jsonRequest = JsonConvert.SerializeObject(paymentRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var result = await client.PostAsync("https://localhost:44381/api/Purchase/SubmitPayment", content);

            if (result.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Payment processed successfully.";
                return RedirectToAction("ListPurchaseOrders"); // Redirect to the ListPurchaseOrders action
            }

            return BadRequest("Error processing payment.");
        }



        //This below is direct interaction with database , we only created the context file and folder
        //for his particular code , so we put it into api to hide db logic

        //[HttpPost]
        //public async Task<IActionResult> SubmitPayment(int purchaseOrderId, string paymentMethod)
        //{
        //    if (string.IsNullOrEmpty(paymentMethod))
        //    {
        //        return BadRequest("Invalid payment method.");
        //    }

        //    var purchaseOrder = await db.PurchaseOrders.FindAsync(purchaseOrderId);
        //    if (purchaseOrder == null)
        //    {
        //        return NotFound("Purchase order not found.");
        //    }

        //    purchaseOrder.Status = "Paid";
        //    await db.SaveChangesAsync(); // Save changes to the database

        //    TempData["SuccessMessage"] = "Payment processed successfully.";
        //    return RedirectToAction("ListPurchaseOrders");
        //}


    }
}
