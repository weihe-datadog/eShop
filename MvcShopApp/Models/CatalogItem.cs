using System.ComponentModel.DataAnnotations;

namespace MvcShopApp.Models
{
    public class CatalogItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
    }
}
