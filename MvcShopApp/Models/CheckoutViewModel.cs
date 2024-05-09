using System.ComponentModel.DataAnnotations;

namespace MvcShopApp.Models
{
    public class CheckoutViewModel
    {
        [Required]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        [Required]
        [Display(Name = "Shipping City")]
        public string ShippingCity { get; set; }

        [Required]
        [Display(Name = "Shipping Zip Code")]
        [DataType(DataType.PostalCode)]
        public string ShippingZip { get; set; }

        [Required]
        [Display(Name = "Billing Address")]
        public string BillingAddress { get; set; }

        [Required]
        [Display(Name = "Billing City")]
        public string BillingCity { get; set; }

        [Required]
        [Display(Name = "Billing Zip Code")]
        [DataType(DataType.PostalCode)]
        public string BillingZip { get; set; }

        [Display(Name = "Coupon Code")]
        public string CouponCode { get; set; }

        [Required]
        //public string CartItems { get; set; }
        public List<CartItemViewModel> CartItems { get; set; }
    }

    // ViewModel to represent an item in the shopping cart
    public class CartItemViewModel
    {
        // Property to hold the ID of the CatalogItem
        public int CatalogItemId { get; set; }
        public int Quantity { get; set; }
        // Properties to hold the product name and price
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }
}
