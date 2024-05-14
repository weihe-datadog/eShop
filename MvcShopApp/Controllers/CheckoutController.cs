using Microsoft.AspNetCore.Mvc;
using MvcShopApp.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

            var items = cartItems.DistinctBy(item => item.Id).Select(item => new CartItemViewModel() {
                CatalogItemId = item.Id,
                ProductName = item.Name,
                Price = item.Price,
                Quantity = cartItems.Count(i => i.Id == item.Id)
            }).ToList();
            
            return View(new CheckoutViewModel() {
                ShippingAddress = "123 Main St",
                ShippingCity = "Anytown",
                ShippingZip = "12345",
                BillingAddress = "123 Main St",
                BillingCity = "Anytown",
                BillingZip = "12345",
                CouponCode = "10OFF",
                CartItems = items
            });
        }

        private ApplyCouponRequest buildApplyCouponRquest(CheckoutViewModel model) {   
            return new ApplyCouponRequest() {
                CouponCode = model.CouponCode,
                Items = model.CartItems.Select(item => new Item() {
                    ProductId = item.CatalogItemId.ToString(),
                    ProductName = item.ProductName,
                    UnitPrice = (float)item.Price,
                    Units = item.Quantity
                }).ToArray()
            };
        }


        private void ValidateOrder(List<CartItemViewModel> items) {
            var numbers = new List<int> { 1, 2, 3, 4, 5 };
            foreach (var number in numbers)
            {
                if (number == 1) {
                    numbers.Add(6); // This will cause an exception
                } else if (number == 2) {
                    numbers.Add(7); // This will cause an exception
                } else {
                    Console.WriteLine(number);
                }
            }

            foreach (var item in items)
            {
                if (item.Quantity <= 0)
                {
                    throw new Exception("Invalid quantity");
                }
                if (item.Price <= 0)
                {
                    throw new Exception("Price can't be negative");
                }
                if (string.IsNullOrEmpty(item.ProductName))
                {
                    throw new Exception("Invalid product name");
                }
            }
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

                    using var response = await _httpClient.PostAsync("http://coupon-django-api:8000/coupons/apply", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var applyCouponResponse = JsonConvert.DeserializeObject<ApplyCouponResponse>(responseBody);

                        var updatedShopCartItems = applyCouponResponse.Items.Select(item => new CartItemViewModel() {
                            CatalogItemId = int.Parse(item.ProductId),
                            ProductName = item.Name,
                            Price = (decimal)item.AdjustedPrice,
                            Quantity = item.AdjustedUnits
                        }).ToList();

                        ValidateOrder(updatedShopCartItems);
                        model.CartItems = updatedShopCartItems;
                        Console.WriteLine("Coupon applied");
                    } else {
                        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
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
