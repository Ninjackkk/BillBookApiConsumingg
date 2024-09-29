using Azure;
using BillBookApiConsuming.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace BillBookApiConsuming.Controllers
{
    public class PurchaseController : Controller
    {
        HttpClient client;                      //Declaring global object of HttpClient class 

        public PurchaseController()
        {
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
            if (purchaseRequest == null)
            {
                return BadRequest("Invalid purchase request.");
            }
            purchaseRequest.PurchaseOrder.PurchaseDate = DateTime.Now;
            purchaseRequest.PurchaseOrder.Status = "Unpaid";
            purchaseRequest.PurchaseOrder.BusinessId = 1001;// we have to take through session
            // API call to create a new purchase order
            using (var apiClient = new HttpClient())
            {
                var jsonRequest = JsonConvert.SerializeObject(purchaseRequest);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var result = await apiClient.PostAsync("https://localhost:44381/api/Purchase/NewPurchase", content);

                if (result.IsSuccessStatusCode)
                {
                    return Ok("Purchase order created successfully.");
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










    }
}
