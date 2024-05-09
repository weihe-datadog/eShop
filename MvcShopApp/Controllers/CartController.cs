using Microsoft.AspNetCore.Mvc;
using MvcShopApp.Models;
using MvcShopApp.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MvcShopApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetString("Cart");
            var cartItems = string.IsNullOrEmpty(cart) ? new List<CatalogItem>() : JsonConvert.DeserializeObject<List<CatalogItem>>(cart);
            return View(cartItems);
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.CatalogItems.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                var cart = HttpContext.Session.GetString("Cart");
                var cartItems = string.IsNullOrEmpty(cart) ? new List<CatalogItem>() : JsonConvert.DeserializeObject<List<CatalogItem>>(cart);
                cartItems.Add(product);
                HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cartItems));
            }
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetString("Cart");
            var cartItems = string.IsNullOrEmpty(cart) ? new List<CatalogItem>() : JsonConvert.DeserializeObject<List<CatalogItem>>(cart);
            var itemToRemove = cartItems.FirstOrDefault(item => item.Id == id);
            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cartItems));
            }
            return RedirectToAction("Index");
        }
    }
}
