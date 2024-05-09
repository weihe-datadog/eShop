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
        public string CartItems { get; set; }
    }
}
