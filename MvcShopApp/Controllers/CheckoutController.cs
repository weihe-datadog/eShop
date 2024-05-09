using Microsoft.AspNetCore.Mvc;
using MvcShopApp.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text;

class ApplyCouponRequest {
    [JsonProperty("coupon_code")]
    public string CouponCode { get; set; }
    [JsonProperty("items")]
    public Item[] Items { get; set; }
}

class ApplyCouponResponse {
    [JsonProperty("adjusted_items")]
    public AdjustedItem[] Items { get; set; }
    [JsonProperty("final_price")]
    public float FinalPrice { get; set; }
}

class AdjustedItem {
    [JsonProperty("id")]
    public string ProductId { get; set; }
    [JsonProperty("original_unit_price")]
    public float OriginalPrice { get; set; }
    [JsonProperty("adjusted_unit_price")]
    public float AdjustedPrice { get; set; }
    [JsonProperty("original_units")]
    public int OriginalUnits { get; set; }
    [JsonProperty("adjusted_units")]
    public int AdjustedUnits { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}

class Item {
    [JsonProperty("id")]
    public string ProductId { get; set; }
    [JsonProperty("name")]
    public string ProductName { get; set; }
    [JsonProperty("unit_price")]
    public float UnitPrice { get; set; }
    [JsonProperty("units")]
    public int Units { get; set; }
}


namespace MvcShopApp.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly HttpClient _httpClient = new HttpClient();
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetString("Cart");
            var cartItems = string.IsNullOrEmpty(cart) ? new List<CatalogItem>() : JsonConvert.DeserializeObject<List<CatalogItem>>(cart);
            if (cartItems.Count == 0)
            {
                // Redirect to the catalog page if the cart is empty
                return RedirectToAction("Index", "Home");
            }
            // ViewBag.CartItems = cartItems;
            var items = JsonConvert.SerializeObject(cartItems);
            return View(new CheckoutViewModel() {
                ShippingAddress = "123 Main St",
                ShippingCity = "Anytown",
                ShippingZip = "12345",
                BillingAddress = "123 Main St",
                BillingCity = "Anytown",
                BillingZip = "12345",
                CouponCode = "EXAMPLECODE",
                CartItems = items
            });
        }

        private ApplyCouponRequest buildApplyCouponRquest(CheckoutViewModel model) {            
            var items = JsonConvert.DeserializeObject<List<CatalogItem>>(model.CartItems);
            var quantityMap = items.GroupBy(item => item.Id).ToDictionary(group => group.Key, group => group.Count());
            var deduppedItems = items.DistinctBy(id => id.Id).ToList();
            return new ApplyCouponRequest() {
                CouponCode = model.CouponCode,
                Items = deduppedItems.Select(item => new Item() {
                    ProductId = item.Id.ToString(),
                    ProductName = item.Name,
                    UnitPrice = (float)item.Price,
                    Units = quantityMap[item.Id]
                }).ToArray()
            };
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            Console.WriteLine("Processing checkout");

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.CouponCode))
                {
                    Console.WriteLine("Applying coupon code");

                    var serializedJson = JsonConvert.SerializeObject(buildApplyCouponRquest(model));
                    var content = new StringContent(serializedJson, Encoding.UTF8, "application/json");

                    try
                    {
                        using var response = await _httpClient.PostAsync("http://coupon-django-api:8000/coupons/apply", content);
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Coupon applied successfully");
                        } else {
                            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                        }
                    } 
                    catch (Exception)
                    {
                        return StatusCode(500, "Internal server error");
                    }
                } 
                // Process the checkout
                // For now, we'll just clear the cart and redirect to a confirmation page
                HttpContext.Session.Remove("Cart");
                return RedirectToAction("Confirmation");
            } else {
                Console.WriteLine("Invalid model state");
            
            }

            // If we got this far, something failed; redisplay the form
            return View(model);
        }

        public IActionResult Confirmation()
        {
            return View();
        }
    }
}
